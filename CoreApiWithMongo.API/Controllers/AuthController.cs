using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CoreApiWithMongo.API.Dtos;
using CoreApiWithMongo.API.Model;
using CoreApiWithMongo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoreApiWithMongo.API.Controllers
{
    
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController:ControllerBase
    {
        private readonly UserServices _services;
        private readonly IConfiguration _config;
        public AuthController(UserServices services, IConfiguration config)
        {
            _config = config;
           _services = services;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserforRegister user)
        {

            user.Username=user.Username.ToLower();
            if(!await _services.IsUserExists(user.Username))
            return BadRequest("Username Already Exists");

            var users=new User
            {
                Username=user.Username
            };

            var obj=await _services.Register(users,user.Password);
            return StatusCode(201);
        }
        [HttpPost("Login")]

        public async Task<IActionResult> Login(UserForLogin userForLogin)
        {
            var userfromrepo = await _services.Login(userForLogin.Username.ToLower(),userForLogin.Password);

            if(userfromrepo == null)
           return Unauthorized();

           var claims = new []
           {
               new Claim(ClaimTypes.NameIdentifier,userfromrepo.Id),
               new Claim(ClaimTypes.Name,userfromrepo.Username)
           };

           var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

             var cred= new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

             var tokendescriptor= new SecurityTokenDescriptor{
                 Subject=new ClaimsIdentity(claims),
                 Expires=System.DateTime.Now.AddDays(1),
                 SigningCredentials=cred
             };

             var handler= new JwtSecurityTokenHandler();

             var token=handler.CreateToken(tokendescriptor);
             return Ok(new{
                 token=handler.WriteToken(token)
             } );

        }

    }
}