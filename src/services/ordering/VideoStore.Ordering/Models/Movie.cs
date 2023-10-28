namespace VideoStore.Ordering.Models
{
    public class Movie
    {
        public int MovieRefId { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
