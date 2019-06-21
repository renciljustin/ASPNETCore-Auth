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
using API.Shared.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [ApiController]
    [Route(RoutePrefix.API)]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IAuthRepository _authRepo;
        private readonly IRefreshTokenRepository _tokenRepo;
        private readonly IUnitOfWork _uow;

        public AuthController(IConfiguration config,
            IAuthRepository authRepo,
            IRefreshTokenRepository tokenRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _config = config;
            _authRepo = authRepo;
            _tokenRepo = tokenRepo;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            var userDb = await _authRepo.FindByUserNameAsync(model.UserName);

            if (userDb != null)
                return BadRequest("Username is already used.");

            var user = _mapper.Map<User>(model);
            var result = await _authRepo.CreateUserAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest("Registration Failed.");

            await _authRepo.AddToRoleAsync(user, RolePrefix.User);
            var userDetail = _mapper.Map<UserDetailDto>(user);

            return CreatedAtRoute("", userDetail);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var userDb = await _authRepo.FindByUserNameAsync(model.UserName);

            if (userDb == null)
                return Unauthorized("Invalid username.");

            var passwordCheck = await _authRepo.CheckPasswordAsync(userDb, model.Password);

            if (!passwordCheck.Succeeded)
                return Unauthorized("Invalid password.");

            var accessToken = await CreateAccessTokenAsync(userDb);
            var refreshToken = await CreateRefreshTokenAsync(userDb.Id);
            
            await _uow.CompleteAsync();

            return Ok(new {
                accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                refreshToken = refreshToken.Value
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromForm] string refreshToken)
        {
            var refreshTokenDb = await _tokenRepo.FindByValueAsync(refreshToken);

            if (refreshTokenDb is null)
                return NotFound("Refresh Token not found.");

            _tokenRepo.RevokeToken(refreshTokenDb);
            await _uow.CompleteAsync();

            return Ok();
        }

        [HttpPut("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken)
        {
            var refreshTokenDb = await _tokenRepo.FindByValueAsync(refreshToken);

            if (refreshTokenDb is null)
                return NotFound("Refresh Token not found.");

            if (refreshTokenDb.ExpirationDate <= DateTime.UtcNow)
                return Unauthorized("Refresh Token is expired.");

            var userDb = await _authRepo.FindByIdAsync(refreshTokenDb.UserId);

            if (userDb is null)
                return NotFound("User not found.");

            _tokenRepo.Refresh(refreshTokenDb);

            var accessToken = await CreateAccessTokenAsync(userDb);

            await _uow.CompleteAsync();

            return Ok(new {
                accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                refreshToken = refreshTokenDb.Value
            });
        }

        #region Render Token
            
        private async Task<JwtSecurityToken> CreateAccessTokenAsync(User user)
        {
            var claims = await RenderClaimsAsync(user);
            var credentials = RenderCredentials();

            var accessToken = new JwtSecurityToken(
                _config["Token:Issuer"],
                _config["Token:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return accessToken;
        }

        private async Task<List<Claim>> RenderClaimsAsync(User userDb)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userDb.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userDb.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, userDb.Email));

            var roles = await _authRepo.GetRolesAsync(userDb);
            claims.AddRange(roles.Select(r => new Claim("role", r)));

            return claims;
        }

        private SigningCredentials RenderCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Token:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            return credentials;
        }

        private async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
        {
            var refreshToken = _tokenRepo.CreateRefreshToken(userId);
            await _uow.CompleteAsync();

            return refreshToken;
        }

        #endregion
    }
}