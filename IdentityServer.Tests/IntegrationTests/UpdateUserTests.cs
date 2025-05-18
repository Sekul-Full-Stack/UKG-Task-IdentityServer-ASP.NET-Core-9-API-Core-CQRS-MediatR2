namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net; 
  
    using Xunit;
    using FluentAssertions;

    using IdentityServer.Application.Commands.UpdateUser;
    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Domain.Models; 

    public class UpdateUserTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public UpdateUserTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenValidData()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            var user = new User
            { 
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",  
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };

            await fakeUserManager.CreateAsync(user);

            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "ganka2@gmail.com",
                PhoneNumber = "+1234567899"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ganka2@gmail.com");
            content.Should().Contain("+1234567899");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var command = new UpdateUserCommand
            {
                Id = 999,
                Email = "nonexistent@gmail.com"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/999", command);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenInvalidEmail()
        {
            var command = new UpdateUserCommand
            {
                Email = "invalid-email"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("A valid email address is required.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenInvalidPhoneNumber()
        {
            var command = new UpdateUserCommand
            {
                PhoneNumber = "12345abc"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Phone number can only contain digits, spaces, '+', '(', ')', or '-' characters.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenEmailIsLessThenFiveChars()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            var user = new User
            {
                Email = "anastasiazagumena@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Nastia",
                PhoneNumber = "+1234567890"
            };

            await fakeUserManager.CreateAsync(user);
            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "a@d."
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Email must be at least 5 characters long.");
            content.Should().Contain("Email format is invalid.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenPhoneNumberTooShort()
        {
            var command = new UpdateUserCommand
            {
                PhoneNumber = "12345"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Phone number must be at least 10 characters long.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenInvalidIdFormat()
        {
            var command = new UpdateUserCommand
            {
                Email = "valid@gmail.com"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/abc", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ID must be a positive number.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenNoDataProvided()
        {
            var command = new UpdateUserCommand();

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ID must be a positive number."); 
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenOnlyPhoneNumberUpdated()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            var user = new User
            {
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            }; 
            await fakeUserManager.CreateAsync(user);

            var command = new UpdateUserCommand
            {
                Id = 1,
                PhoneNumber = "7777788888"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("7777788888");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenOnlyEmailUpdated()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            var user = new User
            {
                Email = "penka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Penka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateAsync(user);

            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "penkapenkova@gmail.com"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("penkapenkova@gmail.com");
        }
 
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenPhoneNumberIsLongButValid()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
             
            var user = new User
            {
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateAsync(user);
            var command = new UpdateUserCommand
            {
                Id = 1,
                PhoneNumber = "+1 (123) 456-7890 ext 123456789012345 11111111111111111"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Phone number can only contain digits, spaces, '+', '(', ')', or '-' characters.");
            content.Should().Contain("Phone number must not exceed 50 characters.");
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenEmailIsLongButValid()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var user = new User
            {
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateAsync(user);

            var longEmail = $"verylongemailaddress1234567890testuser{Guid.NewGuid().ToString("N").Substring(0, 10)}@gmail.com";
            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = longEmail
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(longEmail);
        }
         
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenPhoneNumberContainsAllowedSymbols()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>(); 
            var user = new User
            {
                Email = "ganka@gmail.com",
                Password = "sadsf!#F34",
                UserName = "Ganka",
                PhoneNumber = "+1234567890"
            };
            await fakeUserManager.CreateAsync(user);

            var command = new UpdateUserCommand
            {
                Id = 1,
                PhoneNumber = "+1 (555) 123-4567"
            };

            var response = await _client.PatchAsJsonAsync("/api/users/update-user/1", command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("+1 (555) 123-4567");
        }

    }
}
