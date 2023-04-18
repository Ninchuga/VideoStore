namespace VideoStore.Ordering.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int MovieId { get; set; }
        public string MovieName { get; set; }
    }
}
