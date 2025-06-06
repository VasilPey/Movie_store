
using MovieStoreB.DL.Cache;
using MovieStoreB.Models.DTO;

namespace MovieStoreB.DL.Interfaces
{
    public interface IMovieRepository : ICacheRepository<string, Movie>
    {
        Task<List<Movie>> GetMovies();

        Task AddMovie(Movie movie);

        Task DeleteMovie(string id);

        Task<Movie?> GetMoviesById(string id);
    }
}
