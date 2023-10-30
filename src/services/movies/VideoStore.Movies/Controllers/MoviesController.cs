using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Bus.Messages;
using VideoStore.Movies.Constants;
using VideoStore.Movies.DTOs;
using VideoStore.Movies.Extensions;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Shared;
using Movie = VideoStore.Movies.Models.Movie;

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

        public MoviesController(IMovieRepository movieRepository, ILogger<MoviesController> logger, IServiceProvider serviceProvider)
        {
            _movieRepository = movieRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
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
        [Route("updateMovie/{id}")]
        public async Task<IActionResult> UpdateMovie([FromRoute] int id, [FromBody] MovieDTO movie, CancellationToken cancellationToken)
        {
            if (id == 0)
                return BadRequest();

            _movieRepository.UpsertMovie(movie.ToEntity(id));

            try
            {
                await _movieRepository.SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{ExceptionName} occurred while trying to update the movie movie with id {MovieId}.", nameof(DbUpdateConcurrencyException), id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to update the movie with id {MovieId}.", id);
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
        public async Task<BaseResult> OrderMovies([FromBody] IEnumerable<int> moviesId, CancellationToken cancellationToken)
        {
            var result = new BaseResult();

            if (!moviesId.Any())
                return result.AddError("At least one movie id must be specified.");

            string userEmail = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Email, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userEmail))
                return result.AddError("User email must have a value.");

            string userName = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Subject, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userName))
                return result.AddError("User name must have a value.");

            string userId = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.UserId, StringComparison.OrdinalIgnoreCase))?.Value;
            if (!int.TryParse(userId, out int parsedUserId))
                return result.AddError($"User id {userId} is not a valid integer value.");

            var moviesToOrder = new List<Bus.Messages.Movie>();
            foreach (var movieId in moviesId)
            {
                var movie = await _movieRepository.GetMovieBy(movieId, cancellationToken);
                if (movie is null)
                {
                    result.AddWarning($"Movie with id {movieId} not found.");
                    continue;
                }

                moviesToOrder.Add(new Bus.Messages.Movie(movie.Id, movie.Title, movie.Price));
            }

            if(moviesToOrder.Count == 0)
                return result.AddWarning($"Movies with ids: '{string.Join(", ", moviesId)}' were not found.");

            var publishEndpoint = _serviceProvider.GetService<IPublishEndpoint>();
            if (publishEndpoint is null)
            {
                _logger.LogError("{Controller}.{Action} service {ServiceName} cannot be resolved.", nameof(MoviesController), nameof(OrderMovies) , nameof(IPublishEndpoint));
                return result.AddError($"Service '{nameof(IPublishEndpoint)}' could not be resolved.");
            }

            var orderMovieMessage = new OrderMovieMessage(parsedUserId, userName, userEmail, moviesToOrder);
            await publishEndpoint.Publish(orderMovieMessage, context =>
            {
                context.MessageId = Guid.NewGuid(); // new Guid("6ee234bb-c211-4756-bad1-c1c6e45c1b58");
                context.CorrelationId = Guid.NewGuid();
                context.TimeToLive = TimeSpan.FromMinutes(60); // if not consumed after this ttl, message will end up in dead-letter queue
            }, cancellationToken: cancellationToken);

            return result;
        }
    }
}