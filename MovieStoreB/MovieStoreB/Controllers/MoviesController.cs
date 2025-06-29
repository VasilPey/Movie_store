using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using MovieStoreB.BL.Interfaces;
using MovieStoreB.Models.DTO;
using MovieStoreB.Models.Requests;

namespace MovieStoreB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IMapper _mapper;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            IMovieService movieService,
            IMapper mapper,
            ILogger<MoviesController> logger)
        {
            _movieService = movieService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Movie>> GetAll()
        {
            try
            {
                await _movieService.GetMovies();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in GetAll {e.Message}-{e.StackTrace}");
            }
            return await _movieService.GetMovies();
        }

        [HttpGet("GetById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var result =
                await _movieService.GetMoviesById(id);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("AddMovie")]
        public async Task AddMovie([FromBody] AddMovieRequest movieRequest)
        {
            var movie = _mapper.Map<Movie>(movieRequest);

            await _movieService.AddMovie(movie);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!string.IsNullOrEmpty(id)) return BadRequest($"Wrong id:{id}");

            await _movieService.DeleteMovie(id);

            return Ok();
        }
    }
}
