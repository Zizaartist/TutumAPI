using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TutumAPI.Helpers;
using TutumAPI.Models;

namespace TutumAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly Functions _functions;

        public UsersController(DatabaseContext context, Functions functions) 
        {
            _context = context;
            _functions = functions;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<User> GetMyData() 
        {
            var myId = int.Parse(User.Identity.Name);

            var user = _context.Users.Include(user => user.Subscription)
                                    .FirstOrDefault(user => user.UserId == myId);

            return user;
        }

        // PUT: api/Users/ChangeNumber/?newNumber={newNumber}&code={code}
        [Route("ChangeNumber")]
        [HttpPut]
        public ActionResult ChangeNumber([Phone] string newNumber, string code) 
        {
            var validationResponse = _functions.ValidateCode(newNumber, code);
            if (validationResponse != null) 
            {
                return BadRequest(new { errorText = validationResponse });
            }

            var myId = int.Parse(User.Identity.Name);
            var mySelf = _context.Users.Find(myId);

            mySelf.Phone = newNumber;

            _context.SaveChanges();

            return Ok();
        }

        // PUT: api/Users/ChangePassword/?newPassword={newPassword}&code={code}
        [Route("ChangePassword")]
        [HttpPut]
        public ActionResult ChangePassword(string newPassword, string code)
        {
            var myId = int.Parse(User.Identity.Name);
            var mySelf = _context.Users.Find(myId);
            var myPhone = mySelf.Phone;

            var validationResponse = _functions.ValidateCode(myPhone, code);
            if (validationResponse != null)
            {
                return BadRequest(new { errorText = validationResponse });
            }

            mySelf.Salt = _functions.GenerateSalt();
            mySelf.Password = _functions.SprinkleSomeSalt(newPassword, mySelf.Salt);

            _context.SaveChanges();

            return Ok();
        }

        // POST: api/Users/Subscribe
        /// <summary>
        /// Производит покупку подписки через сбербанк
        /// </summary>
        /// <param name="orderId">id заказа сгенерированный сбером</param>
        [Route("Subscribe")]
        [HttpPost]
        public ActionResult CreateSubscription(string orderId) 
        {
            var myId = int.Parse(User.Identity.Name);

            var user = _context.Users.Include(user => user.Subscription)
                                    .FirstOrDefault(user => user.UserId == myId);

            if (user.HasSubscription) 
            {
                return BadRequest(new { errorText = "У вас уже есть активная подписка. Произведите возврат средств в личном кабинете сбербанка." });
            }

            //Здесь должен быть проверяльщик существования оплаты
            if (true) 
            {
                return BadRequest(new { errorText = "Ошибка при получении номера заказа. Произведите возврат средств в личном кабинете сбербанка." });
            }

            user.Subscription = new Subscription 
            {
                ActivationDate = DateTime.UtcNow.Date,
                Expires = DateTime.UtcNow.AddMonths(1).Date
            };

            _context.SaveChanges();

            //Здесь мы вырубаем возможность возврата средств

            return Ok();
        }
    }
}
