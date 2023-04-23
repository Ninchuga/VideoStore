using MassTransit;
using VideoStore.Bus.Messages;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Ordering.Models;

namespace VideoStore.Ordering.Handlers
{
    public class OrderMovieMessageHandler : IConsumer<OrderMovieMessage>
    {
        private readonly IOrderingRepository _orderingRepository;
        private readonly ILogger<OrderMovieMessageHandler> _logger;
        private readonly IIdempotentMessageHandler<OrderMovieMessage> _idempotentMessageHandler;

        public OrderMovieMessageHandler(IOrderingRepository orderingRepository, ILogger<OrderMovieMessageHandler> logger, IIdempotentMessageHandler<OrderMovieMessage> idempotentMessageHandler)
        {
            _orderingRepository = orderingRepository;
            _logger = logger;
            _idempotentMessageHandler = idempotentMessageHandler;
        }

        public async Task Consume(ConsumeContext<OrderMovieMessage> context)
        {
            await _idempotentMessageHandler.Handle(context, consumerName: nameof(OrderMovieMessageHandler), (dbContext) =>
            {
                var message = context.Message;

                var order = new Order
                {
                    UserEmail = message.UserEmail,
                    UserName = message.UserName,
                    Price = message.Movies.Sum(movie => movie.Price),
                    Movies = MapToMoviesFrom(message.Movies)
                };

                dbContext.Orders.Add(order);
            });
        }

        private static List<Models.Movie> MapToMoviesFrom(IEnumerable<Bus.Messages.Movie> movies)
        {
            return movies.Select(movie => new Models.Movie 
            { 
                MovieRefId = movie.Id, 
                MovieTitle = movie.Title
            }).ToList();
        }
    }
}
