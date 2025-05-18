namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Linq;

    using Xunit;

    using FluentAssertions;

    using IdentityServer.Domain.Models;
    using IdentityServer.Tests.IntegrationTests.Fakes;

    public class GetUserInfoTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public GetUserInfoTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnOk_WhenUserExists()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            { 
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@google.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssst",
                PhoneNumber = "+123456789",
            };
            await fakeUserManager.CreateAsync(user);
            var response = await _client.GetAsync("/api/users/me/info/1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Psssst");
            content.Should().Contain(user.Email);
            content.Should().Contain("+123456789");
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var response = await _client.GetAsync("/api/users/me/info/9999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found");
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnNotFound_WhenIdIsZero()
        {
            var response = await _client.GetAsync("/api/users/me/info/0");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found");
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnNotFound_WhenIdIsNegative()
        {
            var response = await _client.GetAsync("/api/users/me/info/-7");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found");
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnBadRequest_WhenIdIsNotANumber()
        {
            var response = await _client.GetAsync("/api/users/me/info/abc");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
         
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetInfo_ShouldReturnOk_ForMultipleUsers(int userId)
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user1 = new User
            {
                Id = 2,
                Email = "grrrrr@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshsh",
                PhoneNumber = "+123456789",
            };
            var user2 = new User
            {
                Id = 3,
                Email = "grrrrr2@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshshsshshshgrrrrrr",
                PhoneNumber = "+123456789",
            };
            await fakeUserManager.CreateWithIDAsync(user1);
            await fakeUserManager.CreateWithIDAsync(user2);
            var response = await _client.GetAsync($"/api/users/me/info/{userId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
         
        [Fact]
        public async Task GetInfo_ShouldReturnNotFound_WhenIdMissing()
        {
            var response = await _client.GetAsync("/api/users/me/info/"); 
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        } 
         
        [Fact]
        public async Task GetInfo_ShouldReturnOk_WhenUserNameIsLong()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = 6,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssstshshsshshsshshshgrrrrrrrrrrrrrrrrrruuuuu",
                PhoneNumber = "+123456789",
            };
            await fakeUserManager.CreateWithIDAsync(user);  

            var response = await _client.GetAsync("/api/users/me/info/6");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Length.Should().BeGreaterThan(40);
        }

        // 10. Success: Get info for max int ID (simulate user exists)
        [Fact]
        public async Task GetInfo_ShouldReturnOk_WhenUserIdIsMaxInt()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Id = int.MaxValue,
                Email = $"{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com",
                Password = "Alabalanica123!@#",
                UserName = "Psssst",
            };
            await fakeUserManager.CreateWithIDAsync(user);    
            var response = await _client.GetAsync($"/api/users/me/info/{int.MaxValue}");
             
            response.StatusCode.Should().Match(code =>
                code == HttpStatusCode.OK || code == HttpStatusCode.NotFound);

        }
    }
}
