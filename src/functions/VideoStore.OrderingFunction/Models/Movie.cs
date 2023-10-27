namespace VideoStore.OrderingFunction.Models
{
    public class Movie
    {
        public int MovieRefId { get; set; }
        public string MovieTitle { get; set; } = null!;

        public override string ToString() =>
            $"Movie title: {MovieTitle}";
    }
}
