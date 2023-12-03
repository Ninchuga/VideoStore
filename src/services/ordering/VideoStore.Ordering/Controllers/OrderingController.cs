using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoStore.Ordering.Extensions;
using VideoStore.Ordering.Infrastrucutre.Repositories;
using VideoStore.Ordering.Models;
using VideoStore.Ordering.Models.Entities;

namespace VideoStore.Ordering.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderingController : Controller
    {
        private readonly IOrderingRepository _orderingRepository;

        public OrderingController(IOrderingRepository orderingRepository)
        {
            _orderingRepository = orderingRepository;
        }

        [HttpGet]
        [Route("getOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _orderingRepository.GetOrders();

            return Ok(orders);
        }

        [HttpPost]
        [Route("placeOrder")]
        public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var order = request.Map();

            _orderingRepository.AddOrder(order);

            return Ok();
        }
    }
}
