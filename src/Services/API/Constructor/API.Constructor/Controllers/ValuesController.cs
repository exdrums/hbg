using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Constructor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Constructor API", "v1.0" };
        }

        // GET api/values/health
        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<string> Health()
        {
            return "Healthy";
        }
    }
}
