using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieStoreB.DL.Cache;
using MovieStoreB.DL.Gateways;
using MovieStoreB.DL.Interfaces;
using MovieStoreB.DL.Kafka;
using MovieStoreB.DL.Kafka.KafkaCache;
using MovieStoreB.DL.Repositories.MongoRepositories;
using MovieStoreB.Models.Configurations.CachePopulator;
using MovieStoreB.Models.DTO;
using Microsoft.Extensions.Hosting;

namespace MovieStoreB.DL
{
    public static class DependencyInjection
    {
        public static IServiceCollection
            AddDataDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IMovieRepository, MoviesRepository>();
            services.AddSingleton<IActorRepository, ActorMongoRepository>();
            services.AddSingleton<IActorBioGateway, ActorBioGateway>();

            //services.AddHostedService<MongoCacheDistributor>();
            //services.AddSingleton<ICacheRepository<Movie>, MoviesRepository>();

            services.AddCache<MoviesCacheConfiguration, MoviesRepository, Movie, string>(config);
            services.AddCache<ActorsCacheConfiguration, ActorMongoRepository, Actor, string>(config);

            services.AddKafkaConsumer<MoviesCacheConfiguration, string, Movie>();

            //services.AddCache<ComposerCacheConfiguration, ComposerRepository, Composer, int>(config);

            return services;
        }

        /// <summary>
        /// Adds the cache configuration to the service collection.
        /// </summary>
        /// <typeparam name="TCacheConfiguration"></typeparam>
        /// <typeparam name="TCacheRepository"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddCache<TCacheConfiguration, TCacheRepository, TData, TKey>(this IServiceCollection services, IConfiguration config)
           where TCacheConfiguration : CacheConfiguration
           where TCacheRepository : class, ICacheRepository<TKey, TData>
           where TData : ICacheItem<TKey>
           where TKey : notnull
        {
            var configSection = config.GetSection(typeof(TCacheConfiguration).Name);

            if (!configSection.Exists())
            {
                throw new ArgumentNullException(typeof(TCacheConfiguration).Name, "Configuration section is missing in appsettings!");
            }

            services.Configure<TCacheConfiguration>(configSection);

            services.AddSingleton<ICacheRepository<TKey, TData>, TCacheRepository>();
            services.AddSingleton<IKafkaProducer<TData>, KafkaProducer<TKey, TData, TCacheConfiguration>>();
            services.AddHostedService<MongoCachePopulator<TData, TCacheConfiguration, TKey>>();

            return services;
        }
        public static IServiceCollection AddKafkaConsumer<TCacheConfiguration, TKey, TValue>(this IServiceCollection services)
            where TCacheConfiguration : CacheConfiguration
            where TKey : notnull
            where TValue : class
        {
            services.AddSingleton<KafkaCache<TKey, TValue, TCacheConfiguration>>();
            services.AddSingleton<IKafkaCache<TKey, TValue>>(sp => sp.GetRequiredService<KafkaCache<TKey, TValue, TCacheConfiguration>>());
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<KafkaCache<TKey, TValue, TCacheConfiguration>>());

            return services;
        }
    }




}
       
     

