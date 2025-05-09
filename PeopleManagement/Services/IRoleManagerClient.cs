namespace PeopleManagement.Services
{
    using PeopleManagement.Models;

    public interface IRoleManagerClient
    {
        Task<IdentityResult<IEnumerable<string>>> GetRolesAsync(int userId);
        Task<IdentityResult<bool>> CreateRoleAsync(string roleName, string description);
        Task<IdentityResult<bool>> DeleteRoleAsync(int roleId);
        Task<IdentityResult<RoleDTo>> GetRoleByNameAsync(string roleName);
        Task<IdentityResult<bool>> AddClaimAsync(int userId, ClaimDTo claim);
        Task<IdentityResult<bool>> RemoveClaimAsync(int userId, ClaimDTo claim);
        Task<IdentityResult<IEnumerable<ClaimDTo>>> GetClaimsAsync(int userId);
    }

    public class RoleManagerClient : IRoleManagerClient
    {
        private readonly HttpClient _http; 
        public RoleManagerClient(HttpClient http) => _http = http;

        public async Task<IdentityResult<IEnumerable<string>>> GetRolesAsync(int userId)
            => await _http.GetFromJsonAsync<IdentityResult<IEnumerable<string>>>($"/api/roles/user/{userId}");

        public async Task<IdentityResult<bool>> CreateRoleAsync(string roleName, string description)
        {
            var response = await _http.PostAsJsonAsync("/api/roles", new { roleName, description });
            return await response.Content.ReadFromJsonAsync<IdentityResult<bool>>();
        }

        public async Task<IdentityResult<bool>> DeleteRoleAsync(int roleId)
            => await _http.DeleteFromJsonAsync<IdentityResult<bool>>($"/api/roles/{roleId}");

        public async Task<IdentityResult<RoleDTo>> GetRoleByNameAsync(string roleName)
            => await _http.GetFromJsonAsync<IdentityResult<RoleDTo>>($"/api/roles/name/{roleName}");

        public async Task<IdentityResult<bool>> AddClaimAsync(int userId, ClaimDTo claim)
        {
            var response = await _http.PostAsJsonAsync($"/api/claims/{userId}/add", claim);
            return await response.Content.ReadFromJsonAsync<IdentityResult<bool>>();
        }

        public async Task<IdentityResult<bool>> RemoveClaimAsync(int userId, ClaimDTo claim)
        {
            var response = await _http.PostAsJsonAsync($"/api/claims/{userId}/remove", claim);
            return await response.Content.ReadFromJsonAsync<IdentityResult<bool>>();
        }

        public async Task<IdentityResult<IEnumerable<ClaimDTo>>> GetClaimsAsync(int userId)
            => await _http.GetFromJsonAsync<IdentityResult<IEnumerable<ClaimDTo>>>($"/api/claims/{userId}");
    }

}
