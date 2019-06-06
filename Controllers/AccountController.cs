using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signinManager;

        public AccountController(IConfiguration config,
            IMapper mapper,
            UserManager<User> userManager,
            SignInManager<User> signinManager)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var userDb = await _userManager.FindByNameAsync(model.UserName);

            if (userDb != null)
            {
                var passwordCheck = await _signinManager.CheckPasswordSignInAsync(userDb, model.Password, false);

                if (passwordCheck.Succeeded)
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userDb.Id));
                    claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userDb.UserName));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, userDb.Email));

                    var roles = await _userManager.GetRolesAsync(userDb);
                    claims.AddRange(roles.Select(r => new Claim("role", r)));

                    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Token:Key"]));
                    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                    
                    var token = new JwtSecurityToken(
                        _config["Token:Issuer"],
                        _config["Token:Audience"],
                        claims,
                        expires: DateTime.Now.AddHours(3),
                        signingCredentials: credentials
                    );

                    return Ok(new {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                    });
                }
            }

            return Unauthorized("Invalid signing credentials.");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            var userDb = await _userManager.FindByNameAsync(model.UserName);

            if (userDb == null)
            {
                var user = _mapper.Map<User>(model);
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleText.USER);
                    var userDetail = _mapper.Map<UserDetailDto>(user);

                    return CreatedAtRoute("", userDetail);
                }
            }

            return BadRequest();
        }
    }
}