using MassTransit;
using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Infrastrucutre;

namespace VideoStore.Ordering.Handlers
{
    public class IdempotentMessageHandlerDecorator<T> : IIdempotentMessageHandler<T> where T : class
    {
        private readonly OrderingContext _dbContext;
        private readonly ILogger<IdempotentMessageHandlerDecorator<T>> _logger;
        private static readonly SemaphoreSlim _semaphore = new(initialCount: 0, maxCount: 1);

        public IdempotentMessageHandlerDecorator(OrderingContext context, ILogger<IdempotentMessageHandlerDecorator<T>> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task Handle(ConsumeContext<T> messageContext, string consumerName, Action<OrderingContext> action)
        {
            _logger.LogInformation("Processing message {@Message} with id {MessageId} and correlation id {Correlationid} received in {Consumer}",
                messageContext.Message, messageContext.MessageId, messageContext.CorrelationId, consumerName);

            if (messageContext.MessageId is null)
                throw new ArgumentNullException($"Message id must have a value in consumer {consumerName}");

            try
            {
                await _semaphore.WaitAsync();

                // Use Redis to check if message is executing in distributing system by lock mechanism with key-value pair

                _semaphore.Release();

                // in case message processing takes too long, new message will be sent from service bus as retry
                // and when checked if its processed it will not be found in database,
                // and when we call SaveChanges() exception can occur because previous message will be inserted in the meantime
                // or we can end up with duplicated data
                if (await HasBeenProcessed(messageContext, consumerName))
                    return;

                action(_dbContext);

                await _dbContext.IdempotentConsumers.AddAsync(
                    new Models.IdempotentConsumer 
                    { 
                        MessageId = messageContext.MessageId.Value,
                        Consumer = consumerName,
                        MessageProcessed = DateTime.UtcNow
                    });
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Message with id {MessageId} successfully consumed in consumer {Consumer}", messageContext.MessageId, consumerName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occurred while executing message consumer with message: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task<bool> HasBeenProcessed(ConsumeContext<T> message, string consumerName)
        {
            bool hasBeenProcessed = await _dbContext.IdempotentConsumers.AnyAsync(idempMsg =>
                idempMsg.MessageId == message.MessageId && idempMsg.Consumer == consumerName);

            if(hasBeenProcessed)
                _logger.LogWarning("Message with id {MessageId} already consumed by {Consumer}", message.MessageId, consumerName);

            return hasBeenProcessed;
        }
    }
}
