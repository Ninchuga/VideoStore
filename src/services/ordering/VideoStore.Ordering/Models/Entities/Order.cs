namespace VideoStore.Ordering.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public decimal Price => Movies.Sum(movie => movie.Price);
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}
