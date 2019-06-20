using System;
using API.Core.Models;
using API.Shared;
using Microsoft.AspNetCore.Identity;

namespace API.Core.Seeds
{
    public class SeedUsersAndRoles
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        public SeedUsersAndRoles(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void BeginSeeding()
        {
            SeedRoles();
            SeedUsers();
        }

        private void SeedRoles()
        {
            if (!_roleManager.RoleExistsAsync(RoleText.Admin).Result)
            {
                var result = _roleManager.CreateAsync(new IdentityRole { Name = RoleText.Admin }).Result;
            }

            if (!_roleManager.RoleExistsAsync(RoleText.Moderator).Result)
            {
                var result = _roleManager.CreateAsync(new IdentityRole { Name = RoleText.Moderator }).Result;
            }

            if (!_roleManager.RoleExistsAsync(RoleText.User).Result)
            {
                var result = _roleManager.CreateAsync(new IdentityRole { Name = RoleText.User }).Result;
            }
        }

        private void SeedUsers()
        {
            const string adminEmail = "rencil@domain.com";

            if (_userManager.FindByEmailAsync(adminEmail).Result == null)
            {
                var userToCreate = new User
                {
                    UserName = "Rencil",
                    FirstName = "Rencil Justin",
                    LastName = "Evangelista",
                    BirthDate = new DateTime(2018, 10, 28),
                    Email = "rencil@domain.com"
                };

                var result = _userManager.CreateAsync(userToCreate, "P@ssw0rd").Result;

                if (!result.Succeeded)
                    return;

                _userManager.AddToRolesAsync(userToCreate, new string[]{ RoleText.Admin, RoleText.Moderator }).Wait();
            }

            const string userEmail = "johndoe@domain.com";

            if (_userManager.FindByEmailAsync(userEmail).Result == null)
            {
                var userToCreate = new User
                {
                    UserName = "John",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "johndoe@domain.com"
                };

                var result = _userManager.CreateAsync(userToCreate, "P@ssw0rd").Result;

                if (!result.Succeeded)
                    return;

                _userManager.AddToRoleAsync(userToCreate, RoleText.User).Wait();
            }
        }
    }
}