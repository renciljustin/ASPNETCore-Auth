using System.Collections.Generic;
using System.Threading.Tasks;
using API.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Core
{
    public interface IAccountRepository
    {
         Task<User> FindByUserNameAsync(string userName);
         Task<SignInResult> CheckPasswordAsync(User user, string password);
         Task<IEnumerable<string>> GetRolesAsync(User user);
         Task<IdentityResult> CreateUserAsync(User user, string password);
         Task<IdentityResult> AddToRoleAsync(User user, string role);
    }
}