using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Bus.Messages;
using VideoStore.Movies.Constants;
using VideoStore.Movies.DTOs;
using VideoStore.Movies.Extensions;
using VideoStore.Movies.Infrastrucutre.Repositories;
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
        private readonly IPublishEndpoint _publishEndpoint;

        public MoviesController(IMovieRepository movieRepository, ILogger<MoviesController> logger, IPublishEndpoint publishEndpoint)
        {
            _movieRepository = movieRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
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
            _movieRepository.UpsertMovie(movie.ToEntity());
            await _movieRepository.SaveChanges(cancellationToken);

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
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("{ExceptionName} occurred for movie with id {MovieId}.", nameof(DbUpdateConcurrencyException), id);
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

            _movieRepository.DeleteMovie(movie);
            await _movieRepository.SaveChanges(cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("orderMovies")]
        public async Task<IActionResult> BuyMovie([FromBody] IEnumerable<int> moviesId, CancellationToken cancellationToken)
        {
            string userEmail = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Email, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userEmail))
                return BadRequest("User email must have a value.");

            string userName = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Subject, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userName))
                return BadRequest("User name must have a value.");

            string userId = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.UserId, StringComparison.OrdinalIgnoreCase))?.Value;

            var moviesToOrder = new List<Bus.Messages.Movie>();
            foreach (var movieId in moviesId)
            {
                var movie = await _movieRepository.GetMovieBy(movieId, cancellationToken);
                if (movie is null)
                    return NotFound($"Movie with id: {movieId} was not found.");

                moviesToOrder.Add(new Bus.Messages.Movie(movie.Id, movie.Title, movie.Price));
            }

            if (!int.TryParse(userId, out int parsedUserId))
                return BadRequest($"User id {userId} is not a valid integer value.");

            if (_publishEndpoint is null)
                throw new Exception($"{nameof(IPublishEndpoint)} argument in {nameof(MoviesController)} cannot be null.");

            var orderMovieMessage = new OrderMovieMessage(parsedUserId, userName, userEmail, moviesToOrder);
            await _publishEndpoint.Publish(orderMovieMessage, context =>
            {
                context.MessageId = Guid.NewGuid(); // new Guid("6ee234bb-c211-4756-bad1-c1c6e45c1b58");
                context.CorrelationId = Guid.NewGuid();
                context.TimeToLive = TimeSpan.FromMinutes(60); // if not consumed after this ttl, message will end up in dead-letter queue
            }, cancellationToken: cancellationToken);

            return Ok();
        }
    }
}