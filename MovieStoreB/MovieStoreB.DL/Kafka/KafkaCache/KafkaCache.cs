using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MovieStoreB.Models.Configurations.CachePopulator;
using MovieStoreB.Models.DTO;
using MovieStoreB.Models.Serialization;

namespace MovieStoreB.DL.Kafka.KafkaCache
{
    public class KafkaCache<TKey, TValue, TConfiguration> : BackgroundService, IKafkaCache<TKey, TValue>
        where TKey : notnull
        where TValue : class
        where TConfiguration : CacheConfiguration
    {
        private readonly ConsumerConfig _config;
        private readonly ConcurrentDictionary<TKey, TValue> _cache = new();
        private readonly IOptionsMonitor<TConfiguration> _options;

        public KafkaCache(IOptionsMonitor<TConfiguration> options)
        {
            _options = options;
            _config = new ConsumerConfig
            {
                BootstrapServers = "kafka-193981-0.cloudclusters.net:10300",
                GroupId = $"KafkaCache-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "admin",
                SaslPassword = "CPxpKSRD",
                EnableSslCertificateVerification = false
            };
        }
        public IEnumerable<TValue> GetAll() => _cache.Values;

        public bool TryGetValue(TKey key, out TValue value) => _cache.TryGetValue(key, out value);


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => ConsumeMessages(stoppingToken), stoppingToken);
                
            return Task.CompletedTask;
        }

        private void ConsumeMessages(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<TKey, TValue>(_config)
                .SetValueDeserializer(new MessagePackDeserializer<TValue>())
                .Build();

            consumer.Subscribe(_options.CurrentValue.Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);
                if (consumeResult == null || consumeResult.IsPartitionEOF)
                {
                    continue;
                }
                _cache.AddOrUpdate(consumeResult.Message.Key, consumeResult.Message.Value, (_, __) => consumeResult.Message.Value);
            }
        }
    }
}
