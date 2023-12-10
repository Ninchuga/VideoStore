namespace VideoStore.Movies.DTOs
{
    public record MovieDTO(int Id, string Title, string Genre, DateTime ReleaseDate, decimal Price);
}
