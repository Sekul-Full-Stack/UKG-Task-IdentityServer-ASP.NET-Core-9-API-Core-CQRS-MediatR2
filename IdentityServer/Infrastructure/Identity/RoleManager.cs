namespace IdentityServer.Infrastructure.Identity
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Exceptions;
    using IdentityServer.Domain.Models;

    public class RoleManager : IRoleManager
    {
        private readonly IRoleRepository _roleRepository; 
        public RoleManager(IRoleRepository roleRepository) => _roleRepository = roleRepository;

        public async Task<IdentityResult<IEnumerable<Role>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetRolesAsync();
                if (roles == null) return IdentityResult<IEnumerable<Role>>.Failure("No Roles");
                return IdentityResult<IEnumerable<Role>>.Success(roles);
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<IEnumerable<Role>>.Failure($"An error occurred while finding the roles.");
            }
            catch (Exception ex)
            {
                return IdentityResult<IEnumerable<Role>>.Failure($"An unexpected error occurred finding the roles.");
            }
        }

        public async Task<IdentityResult<IEnumerable<string>>> GetRolesAsync(int userId)   
        { 
            try
            {  
                var roles = await _roleRepository.GetUserRolesAsync(userId);
                if (roles == null || !roles.Any()) return IdentityResult<IEnumerable<string>>.Failure("No roles found for the given user.");
                else return IdentityResult<IEnumerable<string>>.Success(roles); 
            }
            catch (RepositoryException ex)
            { 
                return IdentityResult<IEnumerable<string>>.Failure($"An error occurred while getting user roles.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<IEnumerable<string>>.Failure($"An unexpected error occurred.");
            } 
        }

        public async Task<IdentityResult<bool>> AddToRoleAsync(int userId, int roleId)
        {
            try
            { 
                var success = await _roleRepository.AddUserToRoleAsync(userId, roleId);
                if (success) return IdentityResult<bool>.Success(true);
                else return IdentityResult<bool>.Failure("Failed to add user to the role.");
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<bool>.Failure($"Error occurred while adding the user to the role.");
            }
            catch (Exception ex)
            {
                return IdentityResult<bool>.Failure($"An unexpected error occurred.");
            }
        } 

        public async Task<IdentityResult<bool>> CreateRoleAsync(string name, string description)  
        { 
            try
            {
                var created = await _roleRepository.CreateUserRoleAsync(name, description); 
                if (!created) return IdentityResult<bool>.Failure("Failed to create role.");  
                return IdentityResult<bool>.Success(true);
            }
            catch (RepositoryException ex)
            { 
                return IdentityResult<bool>.Failure($"Failed to create role.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<bool>.Failure($"Unexpected error occurred while creating role.");
            }
        }
         
        public async Task<IdentityResult<bool>> UpdateRoleAsync(int id, string? name, string? description)
        {
            try
            {
                var role = await _roleRepository.FindRoleByIdAsync(id);
                if (role == null) return IdentityResult<bool>.Failure("Non existing role.");

                var updated = await _roleRepository.UpdateUserRoleAsync(id, name, description);
                if (!updated) return IdentityResult<bool>.Failure("Failed to update role.");

                return IdentityResult<bool>.Success(true);
            }
            catch (RepositoryException ex)
            { 
                return IdentityResult<bool>.Failure("Failed to update role.");
            }
            catch (Exception ex)
            {
                return IdentityResult<bool>.Failure("Unexpected error occurred while updating the role.");
            }
        } 

        public async Task<IdentityResult<bool>> DeleteRoleAsync(int roleId)  
        { 
            try
            {
                var deleted = await _roleRepository.DeleteUserRoleAsync(roleId); 
                if (!deleted) return IdentityResult<bool>.Failure("Failed to delete role."); 
                return IdentityResult<bool>.Success(true);
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<bool>.Failure($"Failed to delete role.");
            }
            catch (Exception ex)
            {
                return IdentityResult<bool>.Failure($"Unexpected error occurred while deleting role.");
            }
        }  

        public async Task<IdentityResult<Role>> GetRoleByIdAsync(int roleId)  
        {
            try
            {
                var role = await _roleRepository.FindRoleByIdAsync(roleId);
                if (role == null) return IdentityResult<Role>.Failure("No role found for the given user.");
                else return IdentityResult<Role>.Success(role); 
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<Role>.Failure($"Unexpected error occurred from the database.");
            }
            catch (Exception ex)
            {
                return IdentityResult<Role>.Failure($"Unexpected error occurred while retrieving role.");
            }
        }
    }
}
