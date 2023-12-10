using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Constants;
using VideoStore.Movies.DTOs;
using VideoStore.Movies.Extensions;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Shared;
using Movie = VideoStore.Movies.Models.Movie;
using VideoStore.Movies.Services;

namespace VideoStore.Movies.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<MoviesController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OrderService _orderService;

        public MoviesController(IMovieRepository movieRepository, ILogger<MoviesController> logger,
            IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, OrderService orderService)
        {
            _movieRepository = movieRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _orderService = orderService;
        }

        [HttpGet]
        [Route("getMovies")]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetMovies(CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetMovies(cancellationToken);
            return movies.Any() ? Ok(movies.ToDtos()) : NotFound();
        }

        [HttpGet]
        [Route("getMovie/{id}")]
        public async Task<ActionResult<MovieDTO>> GetMovie([FromRoute] int id, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetMovieBy(id, cancellationToken);
            return movie is null ? NotFound() : Ok(movie.ToDto());
        }

        [HttpPost]
        [Route("addMovie")]
        public async Task<ActionResult<Movie>> AddMovie([FromBody] MovieDTO movie, CancellationToken cancellationToken)
        {
            try
            {
                _movieRepository.UpsertMovie(movie.ToEntity());
                await _movieRepository.SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{ExceptionName} occurred while trying to add the movie movie with title {MovieTitle}.", nameof(DbUpdateConcurrencyException), movie.Title);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add the movie with id {MovieTitle}.", movie.Title);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpPut]
        [Route("updateMovie")]
        public async Task<IActionResult> UpdateMovie([FromBody] MovieDTO movie, CancellationToken cancellationToken)
        {
            if (movie is null || movie.Id == 0)
                return BadRequest();

            _movieRepository.UpsertMovie(movie.ToEntity());

            try
            {
                await _movieRepository.SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{ExceptionName} occurred while trying to update the movie movie with id {MovieId}.", nameof(DbUpdateConcurrencyException), movie.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to update the movie with id {MovieId}.", movie.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("deleteMovie/{id}")]
        public async Task<IActionResult> DeleteMovie([FromRoute] int id, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetMovieBy(id, cancellationToken);
            if (movie is null)
                return NotFound();

            try
            {
                _movieRepository.DeleteMovie(movie);
                await _movieRepository.SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{ExceptionName} occurred while trying to delete the movie with id {MovieId}.", nameof(DbUpdateConcurrencyException), id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to delete the movie with id {MovieId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpPost]
        [Route("orderMovies")]
        public async Task<Result> OrderMovies([FromBody] IEnumerable<int> moviesId, CancellationToken cancellationToken)
        {
            var result = new Result();

            if (!moviesId.Any())
                return result.AddError("At least one movie id must be specified.");

            string userEmail = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Email, StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
                return result.AddError("User email must have a value.");

            string userName = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Subject, StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrWhiteSpace(userName))
                return result.AddError("User name must have a value.");

            string userId = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.UserId, StringComparison.OrdinalIgnoreCase))?.Value;
            if (!int.TryParse(userId, out int parsedUserId))
                return result.AddError($"User id {userId} is not a valid integer value.");

            var publishEndpoint = _serviceProvider.GetService<IPublishEndpoint>();
            if (publishEndpoint is null) // Use HTTP communication
            {
                _logger.LogWarning("{Controller}.{Action} service {ServiceName} cannot be resolved.", nameof(MoviesController), nameof(OrderMovies), nameof(IPublishEndpoint));

                return await _orderService.PlaceOrder(moviesId, cancellationToken, userName, userEmail);
            }

            return await _orderService.PublishPlaceOrderMessage(moviesId, cancellationToken, userName, userEmail, publishEndpoint);
        }
    }
}