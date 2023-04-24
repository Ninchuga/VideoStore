namespace VideoStore.Ordering.Models
{
    public class IdempotentConsumer
    {
        public Guid MessageId { get; set; }
        public string Consumer { get; set; } = null!;
        public DateTime MessageProcessed { get; set; }
    }
}
