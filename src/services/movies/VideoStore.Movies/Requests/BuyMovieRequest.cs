namespace VideoStore.Movies.Requests
{
    public class BuyMovieRequest
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
    }
}
