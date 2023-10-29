using Microsoft.Extensions.Caching.Distributed;

namespace VideoStore.Ordering.Infrastrucutre.Repositories
{
    public class MessageHandlersRepository : IMessageHandlersRepository
    {
        private readonly IDistributedCache _redisCache;

        public MessageHandlersRepository(IDistributedCache redisCache)
        {
            _redisCache = redisCache ?? throw new ArgumentNullException($"{nameof(MessageHandlersRepository)} argument {nameof(IDistributedCache)} is null.");
        }

        public async Task DeleteProcessingMessage(string key) =>
            await _redisCache.RemoveAsync(key);

        public async Task<string> GetProcessingMessage(string key) =>
            await _redisCache.GetStringAsync(key);

        public async Task InsertProcessingMessage(string key, DateTime value) =>
            await _redisCache.SetStringAsync(key, value.ToString());
    }
}
