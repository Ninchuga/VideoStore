namespace VideoStore.Ordering.Infrastrucutre.Repositories
{
    public interface IMessageHandlersRepository
    {
        Task DeleteProcessingMessage(string key);
        Task<string> GetProcessingMessage(string key);
        Task InsertProcessingMessage(string key, DateTime value);

    }
}