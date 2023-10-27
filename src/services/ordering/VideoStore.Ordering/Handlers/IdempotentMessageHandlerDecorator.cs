using MassTransit;
using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Infrastrucutre;
using VideoStore.Ordering.Infrastrucutre.Repositories;

namespace VideoStore.Ordering.Handlers
{
    public class IdempotentMessageHandlerDecorator<T> : IIdempotentMessageHandler<T> where T : class
    {
        private readonly OrderingContext _dbContext;
        private readonly ILogger<IdempotentMessageHandlerDecorator<T>> _logger;
        private readonly IMessageHandlersRepository _distributedMessageRepo;
        private static readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);

        public IdempotentMessageHandlerDecorator(OrderingContext context,
            ILogger<IdempotentMessageHandlerDecorator<T>> logger,
            IMessageHandlersRepository distributedMessageHandlerRepo)
        {
            _dbContext = context;
            _logger = logger;
            _distributedMessageRepo = distributedMessageHandlerRepo;
        }

        public async Task Handle(ConsumeContext<T> messageContext, string consumerName, CancellationTokenSource cancellationTokenSource,
            Func<OrderingContext, Task> function)
        {
            _logger.LogInformation("Processing message {@Message} with id {MessageId} and correlation id {Correlationid} received in {Consumer}",
                messageContext.Message, messageContext.MessageId, messageContext.CorrelationId, consumerName);

            if (string.IsNullOrWhiteSpace(consumerName))
                throw new ArgumentNullException($"Argument {nameof(consumerName)} must have a value.");

            if (messageContext.MessageId is null)
                throw new ArgumentNullException($"Message id must have a value in consumer {consumerName}");

            if (cancellationTokenSource is null)
                throw new ArgumentNullException("Cancellation token source cannot be null.");

            string distributedMessageKey = $"{messageContext.MessageId}_{consumerName}";

            try
            {
                await _semaphore.WaitAsync();

                string processingMessage = await _distributedMessageRepo.GetProcessingMessage(distributedMessageKey);
                if (processingMessage is not null)
                    return;

                await _distributedMessageRepo.InsertProcessingMessage(distributedMessageKey, DateTime.UtcNow);

                if (await HasBeenProcessed(messageContext, consumerName))
                    return;

                await function(_dbContext);

                _dbContext.IdempotentConsumers.Add(
                    new Models.IdempotentConsumer
                    {
                        MessageId = messageContext.MessageId.Value,
                        Consumer = consumerName,
                        MessageProcessed = DateTime.UtcNow
                    });

                await _dbContext.SaveChangesAsync(cancellationTokenSource.Token);

                _logger.LogInformation("Message with id {MessageId} successfully consumed in consumer {Consumer}", messageContext.MessageId, consumerName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Operation was cancelled in consumer {Consumer} with message id {MessageId}", consumerName, messageContext.MessageId);
                throw; // throw so the service bus can retry
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occurred while executing message consumer with message: {ErrorMessage}", ex.Message);
                throw; // throw so the service bus can retry
            }
            finally
            {
                await _distributedMessageRepo.DeleteProcessingMessage(distributedMessageKey);
                cancellationTokenSource.Dispose();
                _semaphore.Release();
            }
        }

        private async Task<bool> HasBeenProcessed(ConsumeContext<T> message, string consumerName)
        {
            bool hasBeenProcessed = await _dbContext.IdempotentConsumers.AnyAsync(idempMsg =>
                idempMsg.MessageId == message.MessageId && idempMsg.Consumer == consumerName);

            if (hasBeenProcessed)
                _logger.LogWarning("Message with id {MessageId} already consumed by {Consumer}", message.MessageId, consumerName);

            return hasBeenProcessed;
        }
    }
}
