namespace VideoStore.Bus.Messages
{
    public class OrderMovieMessage : BaseMessage
    {
        public OrderMovieMessage(int userId, string userName, string userEmail, IEnumerable<Movie> movies)
        {
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
            Movies = movies;
        }

        public int UserId { get; }
        public string UserName { get; }
        public string UserEmail { get; }
        public IEnumerable<Movie> Movies { get; }
    }

    public class Movie
    {
        public Movie(int id, string title, decimal price)
        {
            Id = id;
            Title = title;
            Price = price;
        }

        public int Id { get; }
        public string Title { get; }
        public decimal Price { get; }
    }
}
