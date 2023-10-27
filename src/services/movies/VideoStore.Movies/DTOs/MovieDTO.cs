namespace VideoStore.Movies.DTOs
{
    public class MovieDTO
    {
        public string Title { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; } 
    }
}
