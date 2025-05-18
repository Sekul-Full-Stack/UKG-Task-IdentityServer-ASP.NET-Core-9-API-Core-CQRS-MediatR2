namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Linq;

    using Xunit;
     
    using FluentAssertions;

    using IdentityServer.Domain.Models;
    using IdentityServer.Tests.IntegrationTests.Fakes;

    public class DeleteTest : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public DeleteTest(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenUserExists()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>(); 
            fakeUserManager.AddFakeUser(
                email: "sekul@gmail.com",
                password: "Strongpassword123!@#",
                username: "Sekul"
            );

            var response = await _client.DeleteAsync("/api/users/delete-user/1"); 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("true");
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var response = await _client.DeleteAsync("/api/users/delete-user/99999"); 
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenIdIsZero()
        {
            var response = await _client.DeleteAsync("/api/users/delete-user/0"); 
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenIdIsNegative()
        { 
            var response = await _client.DeleteAsync("/api/users/delete-user/-5"); 
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found");
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenIdIsMissing()
        {
            var response = await _client.DeleteAsync("/api/users/delete-user/"); 
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenIdIsNotANumber()
        {
            var response = await _client.DeleteAsync("/api/users/delete-user/abc"); 
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        } 
 
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteUser_ShouldReturnOk_ForMultipleExistingUsers(int userId)
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            
            var users = await fakeUserManager.GetAllUsersAsync();
            List<User> userList = new List<User>();
            if (users.IsSuccess && users.Data != null) userList = users.Data.ToList(); 
                
            if(userList.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    fakeUserManager.AddFakeUser(
                        email: $"galina{i}@gmail.com",
                        password: $"galinaprincess{i}",
                        username: $"Galina{i}"
                    );
                };
            }
           
            var response = await _client.DeleteAsync($"/api/users/delete-user/{userId}"); 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
         
        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDeletedTwice()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>(); 
            for (int i = 0; i < 4; i++) 
                fakeUserManager.AddFakeUser(email: $"galina{i}@gmail.com", password: $"galinaprincess{i}", username: $"Galina{i}");
             
            var first = await _client.DeleteAsync("/api/users/delete-user/4");
            first.StatusCode.Should().Be(HttpStatusCode.OK);

            var second = await _client.DeleteAsync("/api/users/delete-user/4");
            second.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var content = await second.Content.ReadAsStringAsync(); 
            Assert.Contains("User not found", content);  
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenMaxIntId()
        {
            var response = await _client.DeleteAsync($"/api/users/delete-user/{int.MaxValue}"); 
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    } 
}
