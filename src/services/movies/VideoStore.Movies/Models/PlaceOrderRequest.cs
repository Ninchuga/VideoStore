namespace VideoStore.Movies.Models
{
    public record PlaceOrderRequest(string UserName, string UserEmail, IEnumerable<OrderMovie> Movies);

    public record OrderMovie(int Id, string Title, decimal Price);
}
