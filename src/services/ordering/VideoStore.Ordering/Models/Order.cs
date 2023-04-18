namespace VideoStore.Ordering.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public decimal Price { get; set; }
    }
}
