using MassTransit;
using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Infrastrucutre;

namespace VideoStore.Ordering.Handlers
{
    public class IdempotentMessageHandlerDecorator<T> : IIdempotentMessageHandler<T> where T : class
    {
        private readonly OrderingContext _dbContext;
        private readonly ILogger<IdempotentMessageHandlerDecorator<T>> _logger;

        public IdempotentMessageHandlerDecorator(OrderingContext context, ILogger<IdempotentMessageHandlerDecorator<T>> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task Handle(ConsumeContext<T> messageContext, string consumerName, Action<OrderingContext> action)
        {
            _logger.LogInformation("Message with correlation id {Correlationid} received in {Consumer}",
                messageContext.CorrelationId, consumerName);

            if (messageContext.MessageId is null)
                throw new ArgumentNullException($"Message id must have a value in consumer {consumerName}");

            if (await HasBeenProcessed(messageContext, consumerName))
                return;

            try
            {
                action(_dbContext);

                await _dbContext.IdempotentConsumers.AddAsync(
                    new Models.IdempotentConsumer { MessageId = messageContext.MessageId.Value, Consumer = consumerName });
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
            bool hasBeenProcessed = (await _dbContext.IdempotentConsumers.ToListAsync()).Any(idempMsg =>
                            idempMsg.MessageId.Equals(message.MessageId) && idempMsg.Consumer.Equals(consumerName, StringComparison.OrdinalIgnoreCase));

            if(hasBeenProcessed)
                _logger.LogWarning("Message with id {MessageId} already consumed by {Consumer}", message.MessageId, consumerName);

            return hasBeenProcessed;
        }
    }
}
