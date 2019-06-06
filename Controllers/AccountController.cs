using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthDemo.Core;
using AuthDemo.Data.Dtos;
using AuthDemo.Data.Models;
using AuthDemo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthDemo.Controllers
{
    [ApiController]
    [Route(RouteText.API)]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _repo;

        public AccountController(IConfiguration config,
            IAccountRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            var userDb = await _repo.FindByUserNameAsync(model.UserName);

            if (userDb == null)
            {
                var user = _mapper.Map<User>(model);
                var result = await _repo.CreateUserAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _repo.AddToRoleAsync(user, RoleText.User);
                    var userDetail = _mapper.Map<UserDetailDto>(user);

                    return CreatedAtRoute("", userDetail);
                }
            }

            return BadRequest();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var userDb = await _repo.FindByUserNameAsync(model.UserName);

            if (userDb != null)
            {
                var passwordCheck = await _repo.CheckPasswordAsync(userDb, model.Password);

                if (passwordCheck.Succeeded)
                {
                    var claims = await RenderClaims(userDb);
                    var credentials = RenderCredentials();
                    var token = RenderToken(claims, credentials);

                    return Ok(token);
                }
            }

            return Unauthorized("Invalid signing credentials.");
        }

        private async Task<List<Claim>> RenderClaims(User userDb)
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

        
        private object RenderToken(List<Claim> claims, SigningCredentials credentials)
        {
            var token = new JwtSecurityToken(
                _config["Token:Issuer"],
                _config["Token:Audience"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials
            );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                validTo = token.ValidTo
            };
        }
    }
}