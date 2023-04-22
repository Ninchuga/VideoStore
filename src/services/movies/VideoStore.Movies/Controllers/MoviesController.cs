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
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetMovies()
        {
            var movies = await _movieRepository.GetMovies();
            return movies.Any() ? Ok(movies.ToDtos()) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDTO>> GetMovie(int id)
        {
            var movie = await _movieRepository.GetMovieBy(id);
            return movie is null ? NotFound() : Ok(movie.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<Movie>> AddMovie(MovieDTO movie)
        {
            _movieRepository.AddMovie(movie.ToEntity());
            await _movieRepository.SaveChanges();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int id, MovieDTO movie)
        {
            if (id == 0)
                return BadRequest();

            _movieRepository.UpdateMovie(movie.ToEntity(id));

            try
            {
                await _movieRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("{ExceptionName} occurred for movie with id {MovieId}.", nameof(DbUpdateConcurrencyException), id);
                throw;
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _movieRepository.GetMovieBy(id);
            if (movie is null)
                return NotFound();

            _movieRepository.DeleteMovie(movie);
            await _movieRepository.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("orderMovies")]
        public async Task<IActionResult> BuyMovie([FromBody] IEnumerable<int> moviesId)
        {
            string userEmail = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Email, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userEmail))
                return BadRequest($"User email must have a value.");

            string userName = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.Subject, StringComparison.OrdinalIgnoreCase))?.Value;
            if(string.IsNullOrWhiteSpace(userName))
                return BadRequest($"User name must have a value.");

            string userId = User.Claims?.FirstOrDefault(c => c.Type.Equals(MoviesConstants.TokenClaimTypes.UserId, StringComparison.OrdinalIgnoreCase))?.Value;

            var moviesToOrder = new List<Bus.Messages.Movie>();
            foreach (var movieId in moviesId)
            {
                var movie = await _movieRepository.GetMovieBy(movieId);
                if (movie is null)
                    return NotFound($"Movie with id: {movieId} was not found.");

                moviesToOrder.Add(new Bus.Messages.Movie(movie.Id, movie.Title));
            }

            if (!int.TryParse(userId, out int parsedUserId))
                return BadRequest($"User id {userId} is not a valid integer value.");

            var orderMovieMessage = new OrderMovieMessage(parsedUserId, userName, userEmail, moviesToOrder)
            {
                CorrelationId = Guid.NewGuid()
            };
            await _publishEndpoint.Publish(orderMovieMessage);

            return Ok();
        }
    }
}