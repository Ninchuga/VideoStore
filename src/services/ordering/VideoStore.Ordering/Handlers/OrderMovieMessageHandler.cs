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

        public OrderMovieMessageHandler(IOrderingRepository orderingRepository, ILogger<OrderMovieMessageHandler> logger)
        {
            _orderingRepository = orderingRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderMovieMessage> context)
        {
            var message = context.Message;

            var order = new Order
            {
                MovieId = message.MovieId,
                MovieTitle = message.MovieTitle,
                UserEmail = message.UserEmail,
                UserName = message.UserName,
                Price = 100
            };

            try
            {
                _orderingRepository.AddOrder(order);
                await _orderingRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occurred while handling the movie order with message: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
