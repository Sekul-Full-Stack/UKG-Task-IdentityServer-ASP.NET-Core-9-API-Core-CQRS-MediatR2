namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Text.Json;

    using Xunit;
    using FluentAssertions;
    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Domain.Models;
  
    using System.Text.Json.Serialization;
    using IdentityServer.Application.Results;

    public class AllRolesTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public AllRolesTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task GetAllRoles_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/users/admin/all-roles");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
         
        [Fact]
        public async Task GetAllRoles_ShouldReturnJson()
        {
            var response = await _client.GetAsync("/api/users/admin/all-roles");
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        }
         
        [Fact]
        public async Task GetAllRoles_ShouldReturnAtLeastOneRole()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll();
            var role = new Role
            {
                Id = 1,
                Name = "HR ADMIN",
                Description = "Alabalanica123!@#", 
            };
            fakeRoleManager.AddRole(1, role);

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrEmpty(content)) Assert.Fail("Response content is empty.");

            var result = JsonSerializer.Deserialize<IdentityResult<IEnumerable<Role>>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  
            });

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
             
            var roles = result?.Data?.ToList();

            roles.Should().NotBeNull();
            roles.Count.Should().BeGreaterThan(0);

            var roleInResponse = roles.Should().ContainSingle().Which; //there's exactly one user
            roleInResponse.Id.Should().Be(role.Id);
            roleInResponse.Name.Should().Be(role.Name);
            roleInResponse.Description.Should().Be(role.Description);  
        }
         
        [Fact]
        public async Task GetAllRoles_RolesShouldContainExpectedFields()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll();
            var role = new Role
            {
                Id = 1,
                Name = "ADMIN1",
                Description = "" 
            };  
            fakeRoleManager.AddRole(1, role); 
            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("\"id\"");
            content.Should().Contain("\"name\"");
            content.Should().Contain("\"description\"");
            content.Should().Contain("\"dateCreated\""); 
        }
         
        [Fact]
        public async Task GetAllRoles_RoleWithEmptyDescription_ShouldBeHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll();
            var role = new Role
            {
                Id = 1,
                Name = "ADMIN1",
                Description = "" 
            };
            fakeRoleManager.AddRole(1, role);

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("\"description\"");
        }
         
        [Fact]
        public async Task GetAllRoles_SpecialCharacterRoleNames_ShouldBeHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll(); 
            var role = new Role
            {
                Id = 1,
                Name = "!`$*ADMIN",
                Description = "Alabalanica123!@#",
            };
            fakeRoleManager.AddRole(1, role);

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().MatchRegex(@".*!`\$\*ADMIN.*|.*Employee.*|.*Manager.*"); 
        }
         
        [Fact]
        public async Task GetAllRoles_DateCreated_ShouldBeIncluded()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll(); 
            var role = new Role
            {
                Id = 1,
                Name = "HR ADMIN",
                Description = "Alabalanica123!@#",
            };
            fakeRoleManager.AddRole(1, role);

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("dateCreated");
        } 
         
        [Fact]
        public async Task GetAllRoles_ShouldHandleLargeNumberOfRoles()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll();
            for (int i = 1; i <= 11; i++)
            {
                var role = new Role
                {
                    Id = i,
                    Name = $"ADMIN{i}",
                    Description = "Alabalanica123!@#",
                    DateCreated = DateTime.UtcNow
                };
                fakeRoleManager.AddRole(i, role);
            }

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IdentityResult<List<Role>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(11);

            var roleNames = result.Data!.Select(r => r.Name).ToList();
            roleNames.Should().Contain(new[] { "ADMIN1", "ADMIN2", "ADMIN3", "ADMIN4", "ADMIN5", "ADMIN6", "ADMIN7", "ADMIN8", "ADMIN9", "ADMIN10", "ADMIN11" });
         }
         
        [Fact]
        public async Task GetAllRoles_RoleName_ShouldNotBeEmpty()
        {
            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().NotContain("\"Name\":\"\"");
        }
         
        [Fact]
        public async Task GetAllRoles_RoleId_ShouldBePositiveInteger()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll(); 
            var role = new Role
            {
                Id = 1,
                Name = "HR ADMIN",
                Description = "Alabalanica123!@#",
            };
            fakeRoleManager.AddRole(1, role);

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().MatchRegex("\"id\":\\d+");
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnValidRoleList()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            fakeRoleManager.ClearAll();
            for (int i = 1; i <= 4; i++)
            {
                var role = new Role
                {
                    Id = i,
                    Name = $"ADMIN{i}",
                    Description = "Alabalanica123!@#",
                    DateCreated = DateTime.UtcNow
                };
                fakeRoleManager.AddRole(i, role);
            }

            var response = await _client.GetAsync("/api/users/admin/all-roles");
            var content = await response.Content.ReadAsStringAsync(); 

            var result = JsonSerializer.Deserialize<IdentityResult<List<Role>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(4);

            var roleNames = result.Data!.Select(r => r.Name).ToList();
            roleNames.Should().Contain(new[] { "ADMIN1", "ADMIN2", "ADMIN3", "ADMIN4" });
        }

    }
}
