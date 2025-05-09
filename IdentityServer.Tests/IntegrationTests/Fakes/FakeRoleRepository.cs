using IdentityServer.Application.Interfaces;
using IdentityServer.Domain.Models;
using System.Collections.Concurrent;
 

namespace IdentityServer.Tests.IntegrationTests.Fakes
{
    public class FakeRoleRepository : IRoleRepository
    {
        private readonly ConcurrentDictionary<int, Role> _roles = new();
        private int _currentId = 1;

        public Task<bool> IsRoleNameUniqueAsync(string name)
        {
            var exists = _roles.Values.Any(r => r.Name == name);
            return Task.FromResult(!exists);
        }

        public Task CreateRoleAsync(string name, string description)
        {
            var id = _currentId++;
            _roles[id] = new Role
            {
                Id = id,
                Name = name,
                Description = description
            };
            return Task.CompletedTask;
        }

        public Task<bool> CreateUserRoleAsync(string roleName, string description)
        { 
            return Task.FromResult(true);
        }

        public Task<bool> UpdateUserRoleAsync(int id, string? roleName, string? description)
        {
            if (_roles.TryGetValue(id, out var role))
            {
                if (!string.IsNullOrEmpty(roleName)) role.Name = roleName;
                if (!string.IsNullOrEmpty(description)) role.Description = description;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteUserRoleAsync(int roleId)
        {
            return Task.FromResult(_roles.TryRemove(roleId, out _));
        }

        public Task<Role> FindRoleByIdAsync(int roleId)
        {
            _roles.TryGetValue(roleId, out var role);
            return Task.FromResult(role);
        }

        public Task<bool> AddUserToRoleAsync(int userId, int roleId)
        { 
            return Task.FromResult(true);
        }

        public Task<string> GetRoleForUserAsync(int userId)
        {
            return Task.FromResult("Admin");
        }

        public Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "Admin" });
        }

        public Task<IEnumerable<Role>> GetRolesAsync()
        {
            return Task.FromResult(_roles.Values.AsEnumerable());
        }
    }
}
