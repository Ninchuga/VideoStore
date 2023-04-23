namespace VideoStore.Bus.Messages
{
    public record BaseMessage
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

        public Guid Id { get; init; }
        public DateTime MessageCreationDateTime { get; init; }
    }
}
