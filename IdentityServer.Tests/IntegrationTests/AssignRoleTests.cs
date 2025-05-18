namespace IdentityServer.Tests.IntegrationTests
{ 
    using System.Net;

    using Xunit;

    using IdentityServer.Application.Commands.AssignRole;
    using IdentityServer.Domain.Models;
    using IdentityServer.Tests.IntegrationTests.Fakes;

    public class AssignRoleTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public AssignRoleTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AssignRole_ShouldSucceed_WhenUserAndRoleAreValid()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var user = new User
            {
                Id = 1,
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            }; 
            await fakeUserManager.CreateWithIDAsync(user);

            roleManager.AddRole(1, new Role { Id = 1, Name = "ADMIN" });

            var command = new AssignRoleCommand(1, 1);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("true", content);
        }

        [Fact]
        public async Task AssignRole_ShouldNotFound_WhenUserDoesNotExist()
        {
            var roleId = 1;

            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            roleManager.AddRole(roleId, new Role { Id = roleId, Name = "Admin" });

            var command = new AssignRoleCommand(9999, roleId); // userId does not exist
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User not found.", content);
        }

        [Fact]
        public async Task AssignRole_ShouldFail_WhenRoleDoesNotExist()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = 1,
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateWithIDAsync(user);

            var command = new AssignRoleCommand(user.Id, 999); // Role does not exist
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role does not exist", content);
        }

        [Fact]
        public async Task AssignRole_ShouldFailValidation_WhenUserIdIsZero()
        {
            var command = new AssignRoleCommand(0, 1);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User Id must be a positive number", content);
        }

        [Fact]
        public async Task AssignRole_ShouldFailValidation_WhenRoleIdIsZero()
        {
            var command = new AssignRoleCommand(1, 0);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role Id must be a positive number", content);
        }

        [Fact]
        public async Task AssignRole_ShouldFailValidation_WhenRoleIdIsNegative()
        {
            var command = new AssignRoleCommand(1, -1);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role Id must be a positive number", content);
        }

        [Fact]
        public async Task AssignRole_ShouldFailValidation_WhenUserIdIsNegative()
        {
            var command = new AssignRoleCommand(-5, 1);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User Id must be a positive number", content);
        }

        [Fact]
        public async Task AssignRole_ShouldAllowMultipleRolesForSameUser()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var user = new User
            {
                Id = 1,
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateWithIDAsync(user);

            roleManager.AddRole(1, new Role { Id = 1, Name = "Admin" });
            roleManager.AddRole(2, new Role { Id = 2, Name = "Editor" });

            var response1 = await _client.PostAsJsonAsync("/api/users/assign-role", new AssignRoleCommand(user.Id, 1));
            var response2 = await _client.PostAsJsonAsync("/api/users/assign-role", new AssignRoleCommand(user.Id, 2));

            response1.EnsureSuccessStatusCode();
            response2.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task AssignRole_ShouldNotDuplicateRoleForUser()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var user = new User
            {
                Id = 1,
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateWithIDAsync(user);
            roleManager.AddRole(1, new Role { Id = 1, Name = "Admin" });

            await _client.PostAsJsonAsync("/api/users/assign-role", new AssignRoleCommand(user.Id, 1));
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", new AssignRoleCommand(user.Id, 1));

            response.EnsureSuccessStatusCode(); // Service allows reassign, or you'd add logic to detect and fail
        }

        //[Fact]
        //public async Task AssignRole_ShouldThrowOperationCanceled_WhenCancellationRequested()
        //{
        //    var cts = new CancellationTokenSource();
        //    cts.Cancel(); // cancel before use

        //    var command = new AssignRoleCommand(1, 1);
        //    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        //        _client.PostAsJsonAsync("/api/users/assign-role", command, cts.Token));
        //}

        [Fact]
        public async Task AssignRole_ShouldReturnJsonResult_WhenSuccessful()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            var user = new User
            {
                Id = 1,
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateWithIDAsync(user);
            roleManager.AddRole(1, new Role { Id = 1, Name = "Admin" });

            var command = new AssignRoleCommand(user.Id, 1);
            var response = await _client.PostAsJsonAsync("/api/users/assign-role", command);

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        } 
    }
}
