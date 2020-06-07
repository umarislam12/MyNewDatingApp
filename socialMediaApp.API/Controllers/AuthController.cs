using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using socialMedia.API.Data;
using socialMedia.API.Dtos;
using socialMedia.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;
using socialMediaApp.API.Dtos;

namespace socialMedia.API.Controllers
{
  //1.apicontroller tells us where the data in parameter is coming from. also a lot about errors
  //2. Api controller automatically validates from our model and we dont need to manually validate state
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
    {
      _config = config;
          _mapper = mapper;
            _repo = repo;

    }
    [HttpPost("register")]
    //DTOs meet with model classes here in controller
    // FromBody helps to initialize parameter with value
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {
      //Inn case apicontroller is not present we use this method to validate from model
      // if(!ModelState.IsValid)
      // return BadRequest(ModelState);

      //so that capital/lower doesnt create a problem when comparing
      userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
      //Our authRepository takes in only username and password so we use dto to hide extensive information that shall be initialized after calculation in authrepository implementation
      if (await _repo.UserExist(userForRegisterDto.Username))
      {
        return BadRequest("user already exist");
      }

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

      //We initially made object to initialize fields now we will use mapping
      // initializing username field in model
      //var userToCreate = new User()
      //{
      //  //while AuthRepositor takes care of our password
      //  Username = userForRegisterDto.Username
      //};
      var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
            //validate Req
            //CreatedATRoute method we will use later now we are cheating
            return CreatedAtRoute("GetUser", new { controller = "User", id = createdUser.Id }, userToReturn);

    }
    [HttpPost("Login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {

      //throw new Exception("computer says no");
      //check for user existence
      var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.password);
      if (userFromRepo == null)
      {
        return Unauthorized();
      }
      //start building our token
      var claims = new[]{
        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        new Claim(ClaimTypes.Name, userFromRepo.Username)
      };
      //to check our tokn is valid
      //we also need a security key to sign our token
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
      //signin credentials
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
      //security token descriptor
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = creds
      };
      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserForListDto>(userFromRepo);
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            }) ;


    }
  }
}