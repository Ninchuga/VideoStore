namespace VideoStore.Bus.Messages
{
    public class BaseMessage
    {
        public BaseMessage()
        {
            Id = Guid.NewGuid();
            MessageCreationDateTime = DateTime.UtcNow;
        }

        public BaseMessage(Guid id, DateTime creationDate)
        {
            Id = id;
            MessageCreationDateTime = creationDate;
        }

        public Guid Id { get; private set; }
        public Guid CorrelationId { get; set; }
        public DateTime MessageCreationDateTime { get; private set; }
    }
}
