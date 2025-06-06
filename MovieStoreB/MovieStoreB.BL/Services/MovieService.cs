using MovieStoreB.BL.Interfaces;
using MovieStoreB.DL.Interfaces;
using MovieStoreB.Models.DTO;

namespace MovieStoreB.BL.Services
{
    internal class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IActorRepository _actorRepository;
        private readonly IActorBioGateway _actorBioGateway;

        public MovieService(IMovieRepository movieRepository, IActorRepository actorRepository, IActorBioGateway actorBioGateway)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _actorBioGateway = actorBioGateway;
        }

        public async Task<List<Movie>> GetMovies()
        {
            var test = await _actorBioGateway.GetBioByActorId("1234567890");

            var test1 = await _actorBioGateway.GetBioByActor(new Actor());
            return await _movieRepository.GetMovies();
        }

        public async Task AddMovie(Movie movie)
        {
            if (movie == null || movie.ActorIds == null) return;

            movie.DateInserted = DateTime.UtcNow;

            foreach (var actor in movie.ActorIds)
            {
                if (!Guid.TryParse(actor, out _)) return;
            }

            await Task.Run(() => _movieRepository.AddMovie(movie));
        }

        public async Task DeleteMovie(string id)
        {
            if (!string.IsNullOrEmpty(id)) return;

            await Task.Run(() => _movieRepository.DeleteMovie(id));
        }

        public async Task<Movie?> GetMoviesById(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var movieId))
            {
                return null;
            }

            return await Task.Run(() => _movieRepository.GetMoviesById(movieId.ToString()));
        }

        public async Task AddActor(string movieId, Actor actor)
        {
            if (string.IsNullOrEmpty(movieId) || actor == null) return;

            if (!Guid.TryParse(movieId, out _)) return;

            var movie = await Task.Run(() => _movieRepository.GetMoviesById(movieId));

            if (movie == null) return;

            if (movie.ActorIds == null)
            {
                movie.ActorIds = new List<string>();
            }

            if (actor.Id == null || string.IsNullOrEmpty(actor.Id) || Guid.TryParse(actor.Id, out _) == false) return;

            var existingActor = await _actorRepository.GetById(actor.Id);

            if (existingActor != null) return;

            movie.ActorIds.Add(actor.Id);
        }
    }
}
