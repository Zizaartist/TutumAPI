using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TutumAPI.Helpers;
using TutumAPI.Models;

namespace TutumAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly Functions _functions;

        public AuthController(DatabaseContext context, IConfiguration config, IMemoryCache cache, IHttpClientFactory clientFactory, Functions functions)
        {
            _context = context;
            _config = config;
            _cache = cache;
            _clientFactory = clientFactory;
            _functions = functions;
        }

        // POST: api/Auth/Login
        [Route("Login")]
        [HttpPost]
        public ActionResult<string> Login(UserCredentials credentials)
        {
            var existingUser = _context.Users.FirstOrDefault(user => user.Phone == credentials.Phone);

            if (existingUser == null) 
            {
                return NotFound();
            }
            //Если пользователь с таким номером был найден - проверить пароль
            var salt = existingUser.Salt;
            var hashedPassword = _functions.SprinkleSomeSalt(credentials.Password, existingUser.Salt);

            if (existingUser.Password != hashedPassword) 
            {
                return NotFound(); //NotFound в 2х местах чтобы не выдавать какая строка неверная
            }

            var encodedJwt = GetJwtFromUser(existingUser);

            return Json(new { AccessToken = encodedJwt });
        }

        private string GetJwtFromUser(User user)
        {
            //Выдача токена
            var identity = GetIdentity(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtKey"]));

            var jwt = new JwtSecurityToken(
                    claims: identity.Claims,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        // POST: api/Auth/Register?code={code}
        [Route("Register")]
        [HttpPost]
        public ActionResult<string> Register(string code, [Bind("Phone, Name, Password")] User userData)
        {
            var formattedPhone = Functions.convertNormalPhoneNumber(userData.Phone);

            //Проверка кода СМС
            var codeValidationErrorText = _functions.ValidateCode(formattedPhone, code);
            if (codeValidationErrorText != null)
            {
                return BadRequest(new { errorText = codeValidationErrorText });
            }

            //Проверка на существование такого пользователя
            var existingUser = _context.Users.FirstOrDefault(user => user.Phone == formattedPhone);
            if (existingUser != null)
            {
                return Forbid();
            }

            userData.Salt = _functions.GenerateSalt();
            userData.Password = _functions.SprinkleSomeSalt(userData.Password, userData.Salt);

            _context.Users.Add(userData);
            _context.SaveChanges();

            var encodedJwt = GetJwtFromUser(userData);

            return Json(new { AccessToken = encodedJwt });
        }

        /// <summary>
        /// Отправляет СМС код на указанный номер и создает временный кэш с кодом для проверки
        /// </summary>
        /// <param name="phone">Неотформатированный номер</param>
        // POST: api/Auth/SmsCheck/?phone=79991745473
        [Route("SmsCheck")]
        [HttpPost]
        public async Task<IActionResult> SmsCheck([Phone] string phone, bool registrationCheck = false)
        {
            string PhoneLoc = Functions.convertNormalPhoneNumber(phone);

            if (registrationCheck)
            {
                var existingUser = _context.Users.FirstOrDefault(user => user.Phone == PhoneLoc);
                if (existingUser != null)
                {
                    return BadRequest(new { errorText = "Пользователь с таким номером уже существует. Пожалуйста, авторизуйтесь." });
                }
            }

            Random rand = new Random();
            string generatedCode = rand.Next(1000, 9999).ToString();

            if (phone != null)
            {
                if (Functions.IsPhoneNumber(PhoneLoc))
                {
                    //Позволяет получать ip отправителя, можно добавить к запросу sms api для фильтрации спаммеров
                    var senderIp = Request.HttpContext.Connection.RemoteIpAddress;
                    string moreReadable = senderIp.ToString();

                    HttpClient client = _clientFactory.CreateClient();
                    HttpResponseMessage response = await client.GetAsync($"https://smsc.ru/sys/send.php?login=syberia&psw=K1e2s3k4i5l6&phones={PhoneLoc}&mes={generatedCode}");
                    if (response.IsSuccessStatusCode)
                    {
                        //Добавляем код в кэш на 5 минут
                        _cache.Set(Functions.convertNormalPhoneNumber(phone), generatedCode, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                    }
                }
                else
                {
                    return BadRequest(new { errorText = "Проблемы с отправкой СМС" });
                }
            }

            return Ok();
        }

        ///// <summary>
        ///// Отправляет СМС код на указанный номер и создает временный кэш с кодом для проверки
        ///// </summary>
        ///// <param name="phone">Неотформатированный номер</param>
        //// POST: api/Auth/SmsCheck/?phone=79991745473
        //[Route("SmsCheck")]
        //[HttpPost]
        //public async Task<IActionResult> SmsCheck(string phone)
        //{
        //    string PhoneLoc = Functions.convertNormalPhoneNumber(phone);
        //    Random rand = new Random();
        //    string generatedCode = rand.Next(1000, 9999).ToString();
        //    if (phone != null)
        //    {
        //        if (Functions.IsPhoneNumber(PhoneLoc))
        //        {
        //            //Позволяет получать ip отправителя, можно добавить к запросу sms api для фильтрации спаммеров
        //            var senderIp = Request.HttpContext.Connection.RemoteIpAddress;
        //            string moreReadable = senderIp.ToString();

        //            HttpClient client = _clientFactory.CreateClient();
        //            HttpResponseMessage response = await client.GetAsync($"https://smsc.ru/sys/send.php?login=syberia&psw=K1e2s3k4i5l6&phones={PhoneLoc}&mes={generatedCode}");
        //            if (response.IsSuccessStatusCode)
        //            {
        //                //Добавляем код в кэш на 5 минут
        //                _cache.Set(Functions.convertNormalPhoneNumber(phone), generatedCode, new MemoryCacheEntryOptions
        //                {
        //                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        //                });
        //            }
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }

        //    return Ok();
        //}

        ///// <summary>
        ///// Проверяет активность (сущ.) кода
        ///// </summary>
        ///// <param name="code">СМС код</param>
        ///// <param name="phone">Номер получателя</param>
        //// POST: api/Auth/CodeCheck/?code=3344&phone=79991745473
        //[Route("CodeCheck")]
        //[HttpPost]
        //public IActionResult CodeCheck(string code, string phone)
        //{
        //    if (code == _cache.Get(Functions.convertNormalPhoneNumber(phone)).ToString())
        //    {
        //        return Ok();
        //    }

        //    return BadRequest();
        //}

        ///// <summary>
        ///// Подтверждает валидность токена
        ///// </summary>
        //// GET: api/Auth/ValidateToken
        //[Route("ValidateToken")]
        //[HttpGet]
        //public ActionResult ValidateToken()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return Unauthorized(); //"Токен недействителен или отсутствует"
        //    }

        //    return Ok();
        //}

        //identity with user rights
        private ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserId.ToString()),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "User")
            };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}
