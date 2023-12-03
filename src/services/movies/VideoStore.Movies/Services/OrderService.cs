using MassTransit;
using System.Text.Json;
using System.Text;
using VideoStore.Bus.Messages;
using VideoStore.Movies.Constants;
using VideoStore.Movies.Models;
using VideoStore.Shared;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Movies.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace VideoStore.Movies.Services
{
    public class OrderService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(IMovieRepository movieRepository, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _movieRepository = movieRepository;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResult> PlaceOrder(IEnumerable<int> moviesId, CancellationToken cancellationToken,
            string userName, string userEmail)
        {
            var result = new BaseResult();

            var moviesToOrder = await GetMoviesToOrder(moviesId, result, cancellationToken);
            if (moviesToOrder.Count == 0)
                return result.AddWarning($"Movies with ids: '{string.Join(", ", moviesId)}' were not found.");

            
            var httpClient = _httpClientFactory.CreateClient(MoviesConstants.OrderingApiHttpClientName);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetAccessToken());
            var request = new PlaceOrderRequest(userName, userEmail, moviesToOrder.Map());
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync("api/ordering/placeorder", requestContent, cancellationToken);
            if (!httpResponseMessage.IsSuccessStatusCode)
                result.AddError($"Error happened while placing the order with reason: {httpResponseMessage.ReasonPhrase}");

            return result;
        }

        public async Task<BaseResult> PublishPlaceOrderMessage(IEnumerable<int> moviesId, CancellationToken cancellationToken,
            string userName, string userEmail, IPublishEndpoint publishEndpoint)
        {
            var result = new BaseResult();

            var moviesToOrder = await GetMoviesToOrder(moviesId, result, cancellationToken);
            if (moviesToOrder.Count == 0)
                return result.AddWarning($"Movies with ids: '{string.Join(", ", moviesId)}' were not found.");

            var orderMovieMessage = new OrderMovieMessage(userName, userEmail, moviesToOrder.ToBusMessages());
            await publishEndpoint.Publish(orderMovieMessage, context =>
            {
                context.MessageId = Guid.NewGuid(); // new Guid("6ee234bb-c211-4756-bad1-c1c6e45c1b58");
                context.CorrelationId = Guid.NewGuid();
                context.TimeToLive = TimeSpan.FromMinutes(60); // if not consumed after this ttl, message will end up in dead-letter queue
            }, cancellationToken: cancellationToken);

            return result;
        }

        private async Task<List<Models.Movie>> GetMoviesToOrder(IEnumerable<int> moviesId, BaseResult result, CancellationToken cancellationToken)
        {
            var movies = new List<Models.Movie>();
            foreach (var movieId in moviesId)
            {
                var movie = await _movieRepository.GetMovieBy(movieId, cancellationToken);
                if (movie is null)
                {
                    result.AddWarning($"Movie with id {movieId} not found.");
                    continue;
                }

                movies.Add(movie);
            }

            return movies;
        }

        private async Task<string> GetAccessToken() =>
            await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

    }
}
