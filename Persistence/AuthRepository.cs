using System.Collections.Generic;
using System.Threading.Tasks;
using API.Core;
using API.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Persistence
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signinManager;

        public AuthRepository(UserManager<User> userManager, SignInManager<User> signinManager)
        {
            _userManager = userManager;
            _signinManager = signinManager;
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<SignInResult> CheckPasswordAsync(User user, string password)
        {
            return await _signinManager.CheckPasswordSignInAsync(user, password, false);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User> FindByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<IEnumerable<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}