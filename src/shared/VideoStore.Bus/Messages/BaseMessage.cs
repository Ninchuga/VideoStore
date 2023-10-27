namespace VideoStore.Bus.Messages
{
    public abstract record BaseMessage
    {
        public BaseMessage()
        {
            Id = Guid.NewGuid();
            MessageCreationDateTime = DateTime.UtcNow;
        }

        public Guid Id { get; init; }
        public DateTime MessageCreationDateTime { get; init; }
    }
}
