using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVC.Example.Data;

namespace Example.Controllers
{
    [ApiController]
    // [Authorize(Roles ="svc-example")]
    // [Authorize]
    [Route("svc-example")]
    public class ChartDataController : ControllerBase
    {
        private readonly Context context;

        public ChartDataController(Context context)
        {
            this.context = context;
        }

        [HttpGet("location")]
        public async Task<IActionResult> GetLocations()
        {
            var list = await this.context.Locations.ToListAsync();
            return Ok(list);
        }

        [HttpGet("order")]
        public async Task<IActionResult> GetOrders()
        {
            var list = await this.context.Orders.ToListAsync();
            return Ok(list);
        }

        [HttpGet("product")]
        public async Task<IActionResult> GetProducts()
        {
            var list = await this.context.Products.ToListAsync();
            return Ok(list);
        }
    }
}