using System.Threading.Tasks;
using Common.Models.Charts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVC.Example.Model;

namespace Example.Controllers
{
    [ApiController]
    [Route("svc-example/[controller]")]
    public class ChartEntitiesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = ChartEntityList.Create(typeof(Location), typeof(Order), typeof(Product));
            return Ok(list);
        }
    }
}