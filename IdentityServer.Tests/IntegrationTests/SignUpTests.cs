namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using System.Text;

    using Xunit;

    using IdentityServer.Application.Commands.SignIn;
    using IdentityServer.Tests.IntegrationTests.Fakes;
    using IdentityServer.Application.Commands.CreateUser;
    using IdentityServer.Domain.Models;

    public class SignUpTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public SignUpTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnSuccess_WhenValidInput()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();

            // Add a fake role  
            var _role = new Role
            {
                Id = 3,
                Name = "Employee",
                Description = "Employee with little permissions",
                DateCreated = new DateTime(2023, 1, 1)
            };
            fakeRoleManager.AddRole(3, _role);

            // Arrange
            var command = new CreateUserCommand(
                UserName: "Maria",
                Email: "mimi@gmail.com",
                Password: "Mimi$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("1", content);
            Assert.Contains("Maria", content);
            Assert.Contains("mimi@gmail.com", content);
            Assert.Contains("1234567890", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailAlreadyInUse()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeUserManager = scope.ServiceProvider.GetRequiredService<FakeUserManager>();
            fakeUserManager.AddFakeUser("penka@gmail.com", "grrrGosho123!", "Penka");

            // Arrange
            var command = new CreateUserCommand(
                UserName: "Penka",
                Email: "penka@gmail.com",  
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Email already in use.", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenUsernameIsEmpty()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "",
                Email: "validemail@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Username is required", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordTooShort()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "newuser",
                Email: "newuser@example.com",
                Password: "short", // too short
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Password must be at least 8 characters long", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordLacksUppercaseLetter()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "newuser",
                Email: "newuser@example.com",
                Password: "validpassword123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Password must contain at least one uppercase letter", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: "invalidemail", // Invalid email format
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("A valid email address is required", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPhoneNumberIsNotNumeric()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: "user@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "123-456-7890" // Invalid phone number format
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Phone number must be numeric", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPhoneNumberTooShort()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: "user@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "12345" // Too short
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Phone number must be at least 10 characters long", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailIsMissing()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: null, // Email is missing
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Email is required", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordIsMissing()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: "user@example.com",
                Password: null, // Password is missing
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Password is required", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPhoneNumberIsMissing()
        {
            // Arrange
            var command = new CreateUserCommand(
                UserName: "user",
                Email: "user@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: null // Phone number is missing
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Phone number is required", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            
            using var scope2 = _factory.Services.CreateScope();
            var fakeUserManager = scope2.ServiceProvider.GetRequiredService<FakeUserManager>(); 
            fakeUserManager.AddFakeUser("grrrGosho123!@gmail.com", "grrrGosho123!", "Gosho");

            using var scope = _factory.Services.CreateScope(); 
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();     
            var _role = new Role
            {
                Id = 3,
                Name = "Employee",
                Description = "Employee with little permissions",
                DateCreated = new DateTime(2023, 1, 1)
            };
            fakeRoleManager.AddRole(3, _role);

            // Arrange
            var command = new CreateUserCommand(
                UserName: "Georgi",  
                Email: "grrrGosho123!@gmail.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Email already in use.", content);
        } 
         
        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenRoleAssignmentFails()
        {
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>(); 
            await fakeRoleManager.DeleteRoleAsync(3);

            var command = new CreateUserCommand(
                UserName: "newuser",
                Email: "newuser@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Failed to assign role to user.", content);
        }
         
        [Fact]
        public async Task CreateUser_ShouldReturnSuccess_WhenUsernameHasSpecialCharacters()
        { 
            using var scope = _factory.Services.CreateScope();
            var fakeRoleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            var _role = new Role
            {
                Id = 3,
                Name = "Employee",
                Description = "Employee with little permissions",
                DateCreated = new DateTime(2023, 1, 1)
            };
            fakeRoleManager.AddRole(3, _role);

            // Arrange
            var command = new CreateUserCommand(
                UserName: "user@name",
                Email: "user@example.com",
                Password: "ValidPa$$word123",
                PhoneNumber: "1234567890"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/signup", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
           Assert.Contains("1", content);
            Assert.Contains("user@name", content);
            Assert.Contains("user@example.com", content);
            Assert.Contains("1234567890", content);
        }
    } 
}
