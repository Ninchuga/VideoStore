using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Ordering.Models;

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
    }
}
