namespace VideoStore.Ordering.Models.Entities
{
    public class Movie
    {
        public int MovieRefId { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
