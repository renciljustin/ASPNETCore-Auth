using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Core;
using API.Persistence.Dtos;
using API.Core.Models;
using API.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [ApiController]
    [Route(RouteText.API)]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _repo;
        private readonly IUnitOfWork _uow;

        public AuthController(IConfiguration config,
            IAccountRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _config = config;
            _repo = repo;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            var userDb = await _repo.FindByUserNameAsync(model.UserName);

            if (userDb != null)
                return BadRequest("Username is already used.");

            var user = _mapper.Map<User>(model);
            var result = await _repo.CreateUserAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest("Registration Failed.");

            await _repo.AddToRoleAsync(user, RoleText.User);
            var userDetail = _mapper.Map<UserDetailDto>(user);

            return CreatedAtRoute("", userDetail);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var userDb = await _repo.FindByUserNameAsync(model.UserName);

            if (userDb == null)
                return Unauthorized("Invalid username.");

            var passwordCheck = await _repo.CheckPasswordAsync(userDb, model.Password);

            if (!passwordCheck.Succeeded)
                return Unauthorized("Invalid password.");

            var token = await RenderTokenAsync(userDb);

            return Ok(token);
        }

        private async Task<JwtSecurityToken> RenderTokenAsync(User user)
        {
            var claims = await RenderClaimsAsync(user);
            var credentials = RenderCredentials();

            var token = new JwtSecurityToken(
                _config["Token:Issuer"],
                _config["Token:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return token;
        }

        private async Task<List<Claim>> RenderClaimsAsync(User userDb)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userDb.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userDb.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, userDb.Email));

            var roles = await _repo.GetRolesAsync(userDb);
            claims.AddRange(roles.Select(r => new Claim("role", r)));

            return claims;
        }

        private SigningCredentials RenderCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Token:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            return credentials;
        }
    }
}