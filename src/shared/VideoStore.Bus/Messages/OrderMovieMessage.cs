namespace VideoStore.Bus.Messages
{
    public record OrderMovieMessage(int UserId, string UserName, string UserEmail, IEnumerable<Movie> Movies) : BaseMessage;

    public record Movie(int Id, string Title, decimal Price);
}
