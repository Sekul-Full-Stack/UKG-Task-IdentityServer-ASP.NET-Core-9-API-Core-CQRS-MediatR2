namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using Xunit;

    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Domain.Models;
    using IdentityServer.Application.Commands.UpdateRole;

    public class UpdateRoleTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public UpdateRoleTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpdateRole_ShouldSucceed_WhenValidInput()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            roleManager.AddRole(1, new Role { Id = 1, Name = "OldName", Description = "Old description" });

            var command = new UpdateRoleCommand { Id = 1, Name = "NewName", Description = "Updated description" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/1", command);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("true", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenNameIsMissing()
        {
            var command = new UpdateRoleCommand { Description = "Valid description" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/2", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Name is required", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenDescriptionIsMissing()
        {
            var command = new UpdateRoleCommand { Name = "ValidName" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/3", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Description is required", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenNameIsTooShort()
        {
            var command = new UpdateRoleCommand { Name = "A", Description = "Valid description" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/4", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("at least 2 characters", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenDescriptionIsTooShort()
        {
            var command = new UpdateRoleCommand { Name = "ValidName", Description = "AB" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/5", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("at least 3 characters", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenIdIsZero()
        {
            var command = new UpdateRoleCommand { Name = "Valid", Description = "Valid desc" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/0", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("ID must be a positive number", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenRoleDoesNotExist()
        {
            var command = new UpdateRoleCommand { Id = 999, Name = "SomeName", Description = "Some description" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/999", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Failed to update role", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenNameTooLong()
        {
            var command = new UpdateRoleCommand
            {
                Name = new string('A', 51),
                Description = "Valid description"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-role/8", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("must not exceed 50 characters", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldFail_WhenDescriptionTooLong()
        {
            var command = new UpdateRoleCommand
            {
                Name = "Valid Name",
                Description = new string('B', 201)
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-role/9", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("must not exceed 200 characters", content);
        }

        [Fact]
        public async Task UpdateRole_ShouldSucceed_WhenUpdatingToSameValues()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            roleManager.AddRole(10, new Role { Id = 10, Name = "SameName", Description = "SameDesc" });

            var command = new UpdateRoleCommand { Id = 10, Name = "SameName", Description = "SameDesc" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/10", command);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task UpdateRole_ShouldSucceed_WhenOnlyNameIsChanged()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            roleManager.AddRole(11, new Role { Id = 11, Name = "Old", Description = "Description" });

            var command = new UpdateRoleCommand { Id = 11, Name = "NewName", Description = "Description" };
            var response = await _client.PatchAsJsonAsync("/api/users/update-role/11", command);

            response.EnsureSuccessStatusCode();
        }


    }
}
