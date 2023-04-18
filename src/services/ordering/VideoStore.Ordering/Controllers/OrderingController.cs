using Microsoft.AspNetCore.Mvc;

namespace VideoStore.Ordering.Controllers
{
    public class OrderingController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
