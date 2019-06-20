using System.Collections.Generic;
using System.Threading.Tasks;
using API.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Core
{
    public interface IAuthRepository
    {
         Task<IdentityResult> AddToRoleAsync(User user, string role);
         Task<SignInResult> CheckPasswordAsync(User user, string password);
         Task<IdentityResult> CreateUserAsync(User user, string password);
         Task<User> FindByIdAsync(string id);
         Task<User> FindByUserNameAsync(string userName);
         Task<IEnumerable<string>> GetRolesAsync(User user);
    }
}