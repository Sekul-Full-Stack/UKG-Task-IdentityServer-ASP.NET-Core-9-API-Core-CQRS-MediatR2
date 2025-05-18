using IdentityServer.Application.Interfaces;
using IdentityServer.Application.Results;
using IdentityServer.Domain.Models;
using System.Collections.Concurrent;

namespace IdentityServer.Tests.IntegrationTests.Fakes
{   
    public class FakeRoleManager : IRoleManager
    {
        private readonly ConcurrentDictionary<int, Role> _roles = new();
        private readonly Dictionary<int, List<int>> _userRoles = new();  
        private int _nextRoleId = 1;

        public Task<IdentityResult<IEnumerable<string>>> GetRolesAsync(int userId)
        {
            if (_userRoles.TryGetValue(userId, out var roleIds))
            {
                var roleNames = roleIds
                    .Select(id => _roles.TryGetValue(id, out var role) ? role.Name : null)
                    .Where(name => name != null)
                    .ToList();

                return Task.FromResult(IdentityResult<IEnumerable<string>>.Success(roleNames));
            }

            return Task.FromResult(IdentityResult<IEnumerable<string>>.Failure("No roles found for user"));
        }

        public Task<IdentityResult<IEnumerable<Role>>> GetAllRolesAsync()
        {
            var roles = _roles.Values.ToList();
            return Task.FromResult(IdentityResult<IEnumerable<Role>>.Success(roles));
        } 

        public Task<IdentityResult<bool>> CreateRoleAsync(string roleName, string description)
        { 
            if (_roles.Values.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)))  
                return Task.FromResult(IdentityResult<bool>.Failure("Role already exists."));
              
            var id = _nextRoleId++;
            _roles[id] = new Role { Id = id, Name = roleName, Description = description };

            return Task.FromResult(IdentityResult<bool>.Success(true));
        }

        public Task<IdentityResult<bool>> UpdateRoleAsync(int id, string? name, string? description)
        {
            if (_roles.TryGetValue(id, out var role))
            {
                role.Name = name ?? role.Name;
                role.Description = description ?? role.Description;
                return Task.FromResult(IdentityResult<bool>.Success(true));
            }

            return Task.FromResult(IdentityResult<bool>.Failure("Role not found"));
        }

        public Task<IdentityResult<bool>> DeleteRoleAsync(int roleId)
        {
            if (_roles.TryRemove(roleId, out _))
            {
                foreach (var userRole in _userRoles.Values)
                {
                    userRole.Remove(roleId);
                }
                return Task.FromResult(IdentityResult<bool>.Success(true));
            }

            return Task.FromResult(IdentityResult<bool>.Failure("Role not found"));
        }

        public Task<IdentityResult<Role>> GetRoleByIdAsync(int roleId)
        {
            if (_roles.TryGetValue(roleId, out var role))
            {
                return Task.FromResult(IdentityResult<Role>.Success(role));
            }

            return Task.FromResult(IdentityResult<Role>.Failure("Role not found"));
        }

        public Task<IdentityResult<bool>> AddToRoleAsync(int userId, int roleId)
        {
            if (!_roles.ContainsKey(roleId)) 
                return Task.FromResult(IdentityResult<bool>.Failure("Role does not exist"));
              
            if (!_userRoles.ContainsKey(userId))
                _userRoles[userId] = new List<int>();

            _userRoles[userId].Add(roleId);
            return Task.FromResult(IdentityResult<bool>.Success(true));
        }

        public void AddRole(int roleId, Role role)
        {
            if (!_roles.ContainsKey(roleId))
            {
                _roles[roleId] = role;
            }
        }

        public void ClearAll() =>
            _roles.Clear();
    }
}
