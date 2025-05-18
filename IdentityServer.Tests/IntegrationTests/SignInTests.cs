namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Text;

    using Xunit;

    using IdentityServer.Application.Commands.SignIn;
    using IdentityServer.Tests.IntegrationTests.Fakes;

    public class SignInTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public SignInTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        } 

        [Fact]
        public async Task SignIn_ShouldReturnSuccess_WhenValidCredentials()
        {
            // Arrange
            var email = "validemail@example.com";
            var password = "validpa1W$%^ssword";

            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            // Add a fake user that matches the credentials
            fakeUserManager.AddFakeUser(
                email: email,
                password: password,
                username: "validuser"
            );

            var command = new SignInCommand
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();  
            Assert.Contains("token", content);   
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenInvalidCredentials()
        {
            // Arrange
            var email = $"{(string.IsNullOrEmpty("") ? "user" + new Random().Next(1000, 9999) : "")}@gmail.com";
            var password = "validpa1W$%^ssword";

            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();

            // Add a fake user with a different password
            fakeUserManager.AddFakeUser(
                email: email,
                password: "qalidpa1W$%^ssword", // Intentionally wrong password
                username: "validuser"
            );

            var command = new SignInCommand
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Expect BadRequest for wrong credentials
            Assert.Contains("Invalid credentials", content); // Expect the error message
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistentemail@example.com";
            var password = "somepassword";

            var command = new SignInCommand
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Expect BadRequest if user does not exist
            Assert.Contains("Invalid credentials", content); // Expect the error message
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenEmptyCredentials()
        {
            // Arrange
            var command = new SignInCommand
            {
                Email = "",
                Password = ""  
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);  
            Assert.Contains("Email is required.", content); 
            Assert.Contains("Password must be at least 6 characters long.", content);  
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenEmailIsMissing()
        {
            // Arrange
            var command = new SignInCommand
            {
                Email = null, // Email is missing
                Password = "somepassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); 
            Assert.Contains("The Email field is required.", content);  
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenPasswordIsMissing()
        {
            // Arrange
            var command = new SignInCommand
            {
                Email = "validemail@example.com",
                Password = null // Password is missing
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Expect BadRequest if password is missing
            Assert.Contains("Password is required.", content); // Expect the error message
            Assert.Contains("The Password field is required.", content);
        }


        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenTokenGenerationFails()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            var tokenService = scope.ServiceProvider.GetRequiredService<FakeTokenService>();

            tokenService.ForceFailure = true;
            tokenService.FailureMessage = "Token generation failed";

            fakeUserManager.AddFakeUser(
                email: "fail@example.com",
                password: "SomeValidPassword1!",
                username: "failuser"
            );

            var command = new SignInCommand
            {
                Email = "fail@example.com",
                Password = "SomeValidPassword1!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/signin", command);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Token generation failed", content);

            // Reset after test
            tokenService.ForceFailure = false;
        } 
    }
}
