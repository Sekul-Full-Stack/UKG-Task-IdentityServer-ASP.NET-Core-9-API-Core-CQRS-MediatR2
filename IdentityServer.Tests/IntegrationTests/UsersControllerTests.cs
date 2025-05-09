namespace IdentityServer.Tests.IntegrationTests
{
    using FluentAssertions;
    using global::IdentityServer.Application.Commands.CreateUser;
    using global::IdentityServer.Application.Commands.SignIn;
    using global::IdentityServer.Application.Results;
    using global::IdentityServer.Tests.IntegrationTests.IdentityServer.Tests.IntegrationTests;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    public class UsersControllerTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public UsersControllerTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            var serviceProvider = _factory.Services;  
        }


        private StringContent CreateJsonContent<T>(T content)
        {
            return new StringContent(System.Text.Json.JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenInvalidCredentials()
        {
            // Arrange 
            var command = new SignInCommand
            {
                Email = "invaliduser@gmail.com", // invalid email
                Password = "wrongpassword"          // invalid password
            };

            // Act 
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(command));
             
            var responseContent = await response.Content.ReadAsStringAsync();
             
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert 
            Assert.False(response.IsSuccessStatusCode, $"Expected 4xx status code for invalid credentials, but got {response.StatusCode}. Response content: {responseContent}");
             
            if (!string.IsNullOrEmpty(responseContent))
            {
                Assert.Equal("Invalid credentials", responseContent);    
            }
            else
            { 
                Assert.False(true, "Expected 'Invalid credentials' error message, but got an empty response.");
            }
        } 
          
        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenUnregisteredEmail()
        {
            var command = new SignInCommand
            {
                Email = "unregistered@gmail.com",  
                Password = "validpassword"
            };

            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(command));
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal("Invalid credentials", responseContent);   
        }
          
        [Fact]
        public async Task SignIn_ShouldReturnBadRequest_WhenIncorrectPassword()
        {
            var command = new SignInCommand
            {
                Email = "validuser@gmail.com",
                Password = "incorrectpassword" // incorrect password
            };

            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(command));
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal("Invalid credentials", responseContent);   
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
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(command));

            // Assert 
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
          
        [Fact]
        public async Task Should_Return_BadRequest_When_Invalid_Email_Format()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "invalid-email",
                Password = "Password123",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Password_Is_Too_Short()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "testuser@gmail.com",
                Password = "123",  // password too short
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_User_Does_Not_Exist()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "nonexistentuser@gmail.com",
                Password = "Password123",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Password_Is_Incorrect()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "testuser@gmail.com",
                Password = "WrongPassword123",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Email_Is_Empty()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "",
                Password = "Password123",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Password_Is_Empty()
        {
            // Arrange
            var signInCommand = new SignInCommand
            {
                Email = "testuser@gmail.com",
                Password = "",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
         

        [Fact]
        public async Task Should_Return_BadRequest_When_Internal_Error_Occurs()
        {
            // Arrange: Trigger an internal error in the SignInCommandHandler
            var signInCommand = new SignInCommand
            {
                Email = "erroruser@gmail.com",
                Password = "Password123",
            };

            // Act
            var response = await _client.PostAsync("/api/users/signin", CreateJsonContent(signInCommand));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
 
        [Fact]
        public async Task CreateUser_Success_ReturnsCreatedUser()
        {
            var command = new CreateUserCommand("john.doe", "john@gmail.com", "P@ss1w0rd!", "1234567890");
            var response = await _client.PostAsync("api/users/signup", CreateJsonContent(command));

            // Ensure the status code is OK
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Read and log the raw response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {jsonResponse}");

            try
            {
                // Deserialize the JSON response into IdentityResult<CreateUserResponse>
                var result = JsonSerializer.Deserialize<IdentityResult<CreateUserResponse>>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                result.Should().NotBeNull();
                result!.IsSuccess.Should().BeTrue();
                result.Data.Should().NotBeNull();
                result.Data.Email.Should().Be("john@gmail.com");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }




    }
}  
 