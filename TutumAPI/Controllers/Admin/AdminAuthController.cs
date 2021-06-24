using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TutumAPI.Models;

namespace TutumAPI.Controllers.Admin
{
    [Route("[controller]")]
    [ApiController]
    public class AdminAuthController : Controller
    {
        private readonly IConfiguration _config;

        public AdminAuthController(IConfiguration config)
        {
            _config = config;
        }

        //AdminAuth
        [HttpPost]
        public ActionResult<string> GetAdminToken(AdminLoginModel loginModel)
        {
            var correctLogin = _config["Jwt:Login"];
            var correctPassword = _config["Jwt:Password"];

            var inputLogin = loginModel.Login;
            var inputPassword = loginModel.Password;

            if (correctLogin != inputLogin || correctPassword != inputPassword)
            {
                return Forbid();
            }

            var identity = GetIdentity();

            var jwt = new JwtSecurityToken(
                    claims: identity.Claims,
                    expires: DateTime.Now.AddYears(1),
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890"/*_config["Jwt:Key"]*/)), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Json(new { AccessToken = encodedJwt });
        }

        [Authorize]
        [HttpGet]
        public ActionResult test() => Ok();

        [HttpPut]
        public ActionResult test2()
        {
            var dataToRead = User;
            return Ok();
        }

        private ClaimsIdentity GetIdentity()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, "AdminName"),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "Admin")
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}
