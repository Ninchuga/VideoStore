namespace VideoStore.Movies.Requests
{
    public class OrderMovieRequest
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
    }
}
