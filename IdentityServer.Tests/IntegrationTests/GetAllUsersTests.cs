namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Text.Json;

    using Xunit;

    using FluentAssertions;

    using IdentityServer.Domain.Models;
    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Application.Results;
    using IdentityServer.Application.Queries.GetAllUsers;
    using System.Text.Json.Serialization;

    public class GetAllUsersTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public GetAllUsersTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task GetAllUsers_ShouldReturnOk()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshssh",
                PhoneNumber = "+123456789",
            };
            for(var i = 0; i < 30; i++)  
                await fakeUserManager.CreateAsync(user);

            var response = await _client.GetAsync("/api/users/all-users"); 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
         
        [Fact]
        public async Task GetAllUsers_ShouldReturnJson()
        {
            var response = await _client.GetAsync("/api/users/all-users");

            response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAtLeastOneUser()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
             
            var user = new User
            {
                Id = 1,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssswet",
                PhoneNumber = "+123456789",
            }; 
            await fakeUserManager.CreateWithIDAsync(user);
             
            var response = await _client.GetAsync("/api/users/all-users");
             
            var content = await response.Content.ReadAsStringAsync(); 
             
            if (string.IsNullOrEmpty(content)) 
                Assert.Fail("Response content is empty."); 
             
            var result = JsonSerializer.Deserialize<IdentityResult<IEnumerable<User>>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // ensures camel casing
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // handles null values
            }); 
             
            result.Should().NotBeNull(); 
            result.IsSuccess.Should().BeTrue();

            //extrakt the data from the result (which is an IEnumerable<User>)
            var users = result?.Data?.ToList(); //converts IEnumerable<User> to List<User>

            users.Should().NotBeNull();
            users.Count.Should().BeGreaterThan(0);

            var userInResponse = users.Should().ContainSingle().Which; //there's exactly one user
            userInResponse.Id.Should().Be(user.Id);
            userInResponse.Email.Should().Be(user.Email);
            userInResponse.UserName.Should().Be(user.UserName);
        } 
         
        [Fact]
        public async Task GetAllUsers_UsersShouldContainValidFields()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = 1,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshzzzzzz",
                PhoneNumber = ""
            };
            await fakeUserManager.CreateWithIDAsync(user);

            var response = await _client.GetAsync("/api/users/all-users");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("1");
            content.Should().Contain(user.Email);
            content.Should().Contain("Psssstshshzzzzzz");
            content.Should().Contain("\"roles\":");
            content.Should().Contain("\"dateCreated\":");
        }
         
        [Fact]
        public async Task GetAllUsers_ShouldIncludeRolesField()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = 1,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshshsshs",
                PhoneNumber = ""
            };
            await fakeUserManager.CreateWithIDAsync(user);
            var response = await _client.GetAsync("/api/users/all-users");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("\"roles\":");  
        }
         
        [Fact]
        public async Task GetAllUsers_UsersWithNoRoles_ShouldReturnEmptyArray()
        {
            var response = await _client.GetAsync("/api/users/all-users");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Match(c => c.Contains("[]") || c.Contains("\"Roles\":[]"), 
                "Response should contain either an empty array or users with empty roles.");
        }
         
        [Fact]
        public async Task GetAllUsers_UserWithNullPhoneNumber_ShouldBeHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>(); 
            var user = new User
            {
                Id = 1,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshshsshs",
                PhoneNumber = ""
            };
            await fakeUserManager.CreateWithIDAsync(user);
            
            var response = await _client.GetAsync("/api/users/all-users");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("1");
            content.Should().Contain("Psssstshshsshshsshs");
            content.Should().Contain(user.Email);
            content.Should().Contain("\"phoneNumber\":"); 
        } 
         
        [Fact]
        public async Task GetAllUsers_ShouldIncludeValidDateCreated()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = 1,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshshsshs",
                PhoneNumber = "+213456789"
            };
            await fakeUserManager.CreateWithIDAsync(user);
            var response = await _client.GetAsync("/api/users/all-users");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("\"dateCreated\":");   
        } 
         
        [Fact]
        public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        { 
            var response = await _client.GetAsync("/api/users/all-users");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Contains("[]") || content.Contains("\"Roles\":[]"),
                "Response should contain either an empty array or users with empty roles.");
        }
    } 
}
