using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using API.Core.Models;
using API.Shared;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace API.Core.Seeds
{
    public class SeedUsersAndRoles
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        public SeedUsersAndRoles(UserManager<User> userManager, RoleManager<Role> roleManager)
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
            if (!_roleManager.Roles.Any())
            {
                var data = File.ReadAllText("Core/Seeds/roles.json");
                var roles = JsonConvert.DeserializeObject<List<Role>>(data);

                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }
            }
        }

        private void SeedUsers()
        {
            if (!_userManager.Users.Any())
            {
                var data = File.ReadAllText("Core/Seeds/users.json");
                var users = JsonConvert.DeserializeObject<List<User>>(data);

                var roles = _roleManager.Roles.ToList();
                var roleIdx = 0;

                foreach (var user in users)
                {
                    var result = _userManager.CreateAsync(user, "P@ssw0rd").Result;

                    if (result.Succeeded)
                    {
                        _userManager.AddToRoleAsync(user, roles[roleIdx].Name).Wait();

                        if (roleIdx + 1 != roles.Count())
                            roleIdx++;
                    }
                }
            }
        }
    }
}