namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using Xunit;
     
    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Domain.Models;  

    public class DeleteRoleTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public DeleteRoleTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task DeleteRole_ShouldSucceed_WhenRoleExists()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 1, Name = "TestRole", Description = "To be deleted" };
            roleManager.AddRole(1, role);

            var response = await _client.DeleteAsync("/api/users/delete-role/1");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("true", content);
        }

        [Fact]
        public async Task DeleteRole_ShouldFail_WhenRoleIdIsZero()
        {
            var response = await _client.DeleteAsync("/api/users/delete-role/0");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role not found.", content);
        }

        [Fact]
        public async Task DeleteRole_ShouldFail_WhenRoleIdIsNegative()
        {
            var response = await _client.DeleteAsync("/api/users/delete-role/-5");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role not found.", content);
        }

        [Fact]
        public async Task DeleteRole_ShouldFail_WhenRoleNotFound()
        {
            var response = await _client.DeleteAsync("/api/users/delete-role/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role not found.", content);
        }

        [Fact]
        public async Task DeleteRole_ShouldFail_WhenRoleAlreadyDeleted()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 2, Name = "OldRole", Description = "Was deleted" };
            roleManager.AddRole(2, role);
            await roleManager.DeleteRoleAsync(2);

            var response = await _client.DeleteAsync("/api/users/delete-role/2");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role not found", content);
        }
         
        [Fact]
        public async Task DeleteRole_ShouldSucceed_WhenRoleIdIsLarge()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 99999, Name = "BigIdRole", Description = "Big ID role" };
            roleManager.AddRole(99999, role);

            var response = await _client.DeleteAsync("/api/users/delete-role/99999");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnNotFound_WhenIdIsNonNumeric()
        {
            var response = await _client.DeleteAsync("/api/users/delete-role/abc"); 
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task DeleteRole_ShouldThrow_WhenRequestCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _client.DeleteAsync("/api/users/delete-role/1", cts.Token));
        }

        [Fact]
        public async Task DeleteRole_ShouldSucceedFirstTime_ThenFailSecondTime()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 10, Name = "TempRole", Description = "Temp" };
            roleManager.AddRole(10, role);

            var first = await _client.DeleteAsync("/api/users/delete-role/10");
            var second = await _client.DeleteAsync("/api/users/delete-role/10");

            Assert.True(first.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        }

        [Fact]
        public async Task DeleteRole_ShouldRemoveRoleFromUserAssignments()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 11, Name = "Orphaned", Description = "To unassign" };
            roleManager.AddRole(11, role);
            await roleManager.AddToRoleAsync(userId: 1, roleId: 11);

            var response = await _client.DeleteAsync("/api/users/delete-role/11");

            response.EnsureSuccessStatusCode();

            var rolesResult = await roleManager.GetRolesAsync(1);
            Assert.DoesNotContain("Orphaned", rolesResult.Data);
        }

        [Fact]
        public async Task DeleteRole_ShouldSucceed_WhenRoleIdIsOne()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var role = new Role { Id = 1, Name = "Root", Description = "Lowest ID" };
            roleManager.AddRole(1, role);

            var response = await _client.DeleteAsync("/api/users/delete-role/1");

            response.EnsureSuccessStatusCode();
        }

    }
}
