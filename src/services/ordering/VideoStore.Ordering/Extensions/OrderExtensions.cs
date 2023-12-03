using VideoStore.Ordering.Models;
using VideoStore.Ordering.Models.Entities;

namespace VideoStore.Ordering.Extensions
{
    public static class OrderExtensions
    {
        public static Order Map(this PlaceOrderRequest request)
        {
            return new Order
            {
                UserEmail = request.UserEmail,
                UserName = request.UserEmail,
                Movies = request.Movies.Map()
            };
        }
    }
}
