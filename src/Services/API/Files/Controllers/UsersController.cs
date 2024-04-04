using System.IO;
using System.Threading.Tasks;
using API.Files.Model;
using API.Files.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Files.Controllers
{
    [Route("files/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly AuthService auth;
        public UsersController(AuthService auth)
        {
            this.auth = auth;
        }

        // [HttpGet]
        // [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
		// [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> GetUserImage() 
        // {
        //     var image = new UserImage(this.auth.GetUserId());
        //     var file = image.Stored;
        //     if(!file.Exists)
        //         return NotFound();
        //     return base.File(file.CreateReadStream(), image.ContentType);
        // }

        // [HttpPost]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // public async Task<IActionResult> PostUserImage([FromQuery] IFormFile file) 
        // {

        //     var image = new UserImage(file, this.auth.GetUserId());
        //     await image.SaveFile();
        //     return NoContent();
        // }

    }
}