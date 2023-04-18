namespace VideoStore.Bus.Messages
{
    public class OrderMovieMessage : BaseMessage
    {
        public OrderMovieMessage(int userId, string userName, string userEmail, int movieId, string movieTitle)
        {
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
            MovieId = movieId;
            MovieTitle = movieTitle;
        }

        public int UserId { get; }
        public string UserName { get; }
        public string UserEmail { get; }
        public int MovieId { get; }
        public string MovieTitle { get; }
    }
}
