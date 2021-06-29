using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutumAPI.Models;

namespace TutumAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly DatabaseContext _context;

        public UsersController(DatabaseContext context) 
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<User> GetMyData() 
        {
            var myId = int.Parse(User.Identity.Name);

            var user = _context.Users.Find(myId);

            return user;
        }
    }
}
