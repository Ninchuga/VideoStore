using MassTransit;
using VideoStore.Bus.Messages;
using VideoStore.Ordering.Extensions;
using VideoStore.Ordering.Models.Entities;

namespace VideoStore.Ordering.Handlers
{
    public class OrderMovieMessageHandler : IConsumer<OrderMovieMessage>
    {
        private readonly IIdempotentMessageHandler<OrderMovieMessage> _idempotentMessageHandler;

        public OrderMovieMessageHandler(IIdempotentMessageHandler<OrderMovieMessage> idempotentMessageHandler)
        {
            _idempotentMessageHandler = idempotentMessageHandler;
        }

        public async Task Consume(ConsumeContext<OrderMovieMessage> context)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await _idempotentMessageHandler.Handle(context, consumerName: nameof(OrderMovieMessageHandler), cancellationTokenSource,
                function: (dbContext) =>
                {
                    var message = context.Message;

                    var order = new Order
                    {
                        UserEmail = message.UserEmail,
                        UserName = message.UserName,
                        Movies = message.Movies.Map()
                    };

                    if (cancellationTokenSource.IsCancellationRequested)
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    dbContext.Orders.Add(order);

                    return Task.CompletedTask;
                });
        }

       
    }
}
