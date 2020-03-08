using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper; // for main photo on nav bar 
            _config = config;
            _repo = repo;
        }
        
        //register a new user with the below API
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //convert username field to lower
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            //does this username already exist, go to 'UserExist' method which will return true
            //if the username exists in the database  
            if (await _repo.UserExist(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            //mapping 'userForRegisterDto to the 'User' model
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            //go to the register method and create user object with its password. 
            //the password will be hashed
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            //convert back from 'User' to DTo and assign it to object 'userToReturn' 
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            //201 response sent with user object created. 
            return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id},userToReturn);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username, userForLoginDto.Password); //user has logged in, get all users photos and information

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo); //this DTO gets the main photo url.

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                //to allow main photo to be updated on nav bar without needing multiple API requests
                //this will pass down the user information alongside the token, so not inside the token. 
                user
            });
        }
    }
}