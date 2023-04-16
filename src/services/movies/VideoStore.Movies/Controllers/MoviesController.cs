using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.DTOs;
using VideoStore.Movies.Extensions;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Movies.Models;
using VideoStore.Movies.Requests;

namespace VideoStore.Movies.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IMovieRepository movieRepository, ILogger<MoviesController> logger)
        {
            _movieRepository = movieRepository;
            _logger = logger;
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
        [Route("buyMovie")]
        public async Task<IActionResult> BuyMovie([FromBody] BuyMovieRequest request)
        {
            // get all the data and send the request to Ordering/Subscription service via message bus
            string userEmail = User.Claims?.FirstOrDefault(c => c.Type.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;
            string userName = User.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value;
            string userId = User.Claims?.FirstOrDefault(c => c.Type.Equals("userId", StringComparison.OrdinalIgnoreCase))?.Value;

            // publish the message

            return Ok();
        }
    }
}