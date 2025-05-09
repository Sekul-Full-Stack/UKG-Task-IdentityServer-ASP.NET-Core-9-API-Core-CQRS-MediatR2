namespace PeopleManagementAPI.Tests.Unit.Controllers
{
    using System.Security.Claims;
    using System.Threading;

    using Microsoft.AspNetCore.Mvc;

    using Moq;
    using Xunit;
    using FluentAssertions;

    using PeopleManagementAPI.Services;
    using PeopleManagementAPI.Models;
    using PeopleManagementAPI.Controllers; 

    public class PeopleControllerTests
    {
        private readonly Mock<IUserHttpClient> _mockHttpClient;
        private readonly PeopleController _controller;
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        public PeopleControllerTests()
        {
            _mockHttpClient = new Mock<IUserHttpClient>();
            _controller = new PeopleController(_mockHttpClient.Object);
             
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "EMPLOYEE")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            SetUserRoles("HR ADMIN");
        }

        private void SetUserRoles(params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        } 
        private List<UserResponse> GetSampleUsers()
        {
            return new List<UserResponse>
            {
                new UserResponse
                {
                    Id = 1,
                    Email = "jane.fox@gmail.com",
                    UserName = "janefox",
                    PhoneNumber = "555-1234",
                    DateCreated = DateTime.UtcNow,
                    Roles = new List<string> { "EMPLOYEE" }
                },
                new UserResponse
                {
                    Id = 2,
                    Email = "john.smith@gmail.com",
                    UserName = "johnsmith",
                    PhoneNumber = "555-5678",
                    DateCreated = DateTime.UtcNow,
                    Roles = new List<string> { "EMPLOYEE" }
                }
            };
        }

        [Fact]
        public async Task SignUp_UserIsHRAdmin_ReturnsOk()
        {
            // Arrange
            var user = new SignUpRequestDTO("TestUser", "test@gmail.com", "Password123", "1234567890");
            var cancellationToken = new System.Threading.CancellationToken();
             
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var userResponse = new UserResponse
            {
                Id = 1,
                Email = "test@gmail.com",
                UserName = "TestUser",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.Now,
                Roles = new List<string> { "HR ADMIN" }
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<SignUpRequestDTO, UserResponse>(
                    HttpMethod.Post, "/api/users/signup", cancellationToken, user))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.SignUp(user, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
        }

        [Fact]
        public async Task SignUp_UserIsNotHRAdmin_ReturnsForbid()
        {
            // Arrange
            var user = new SignUpRequestDTO("TestUser", "test@gmail.com", "Password123", "1234567890");
            var cancellationToken = new System.Threading.CancellationToken();
             
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "EMPLOYEE")
            }, "mock"));

            // Act
            var result = await _controller.SignUp(user, cancellationToken);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SignUp_HttpClientReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var user = new SignUpRequestDTO("TestUser", "test@gmail.com", "Password123", "1234567890");
            var cancellationToken = new System.Threading.CancellationToken();
             
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));
             
            _mockHttpClient.Setup(client => client.SendAsync<SignUpRequestDTO, UserResponse>(
                    HttpMethod.Post, "/api/users/signup", cancellationToken, user))
                .ReturnsAsync(IdentityResult<UserResponse>.Failure("Error occurred"));

            // Act
            var result = await _controller.SignUp(user, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<UserResponse>>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Error occurred", returnValue.Error);
        }

        [Fact]
        public async Task SignUp_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var request = new SignUpRequestDTO("Nika Blond", "nikoleta@gmail.com", "Pass123!", "1234567890");

            var response = new UserResponse
            {
                Id = 1,
                UserName = "Nika Blond",
                Email = "nikoleta@gmail.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "HR ADMIN" }
            };

            _mockHttpClient
                .Setup(x => x.SendAsync<SignUpRequestDTO, UserResponse>(
                    HttpMethod.Post, "/api/users/signup", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(response));


            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),  
                        new Claim(ClaimTypes.Name, "Nika Blond"),   
                        new Claim(ClaimTypes.Role, "HR ADMIN")     
                    }, "mock"))
                }
            };


            // Act
            var result = await controller.SignUp(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);  
            var returned = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            returned.IsSuccess.Should().BeTrue(); 
        }

        [Fact]
        public async Task SignIn_ReturnsOk_WhenCredentialsAreCorrect()
        {
            // Arrange
            var request = new SignInRequestDTO { Email = "silvia@google.com", Password = "Pass123!" };
            var userDto = new UserDto(
                Id: 1,
                UserName: "silvia",
                Email: "silvia@google.com",
                Password: "Pass123!",
                PhoneNumber: "123-456-7890",
                Roles: new List<string> { "EMPLOYEE" }
            );

            var response = new SignInResponseDTO
            {
                Token = "mock-token",
                User = new UserDto(
                    Id: 1,
                    UserName: "silvia",
                    Email: "silvia@google.com",
                    Password: "Pass123!",
                    PhoneNumber: "123-456-7890",
                    Roles: new List<string> { "EMPLOYEE" }
                )
            };

            _mockHttpClient
                .Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), request)
                )
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Success(response));

            // Act
            var result = await _controller.SignIn(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<SignInResponseDTO>>(okResult.Value);
            value.Data.Token.Should().Be("mock-token");
        }

        [Fact]
        public async Task SignUp_ReturnsForbidden_IfUserIsNotHRAdmin()
        { 
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "EMPLOYEE")  
                    }, "mock"))
                }
            };

            var result = await controller.SignUp(new SignUpRequestDTO("test", "user", "email", "password"), CancellationToken.None);
             
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }
         
        [Fact]
        public async Task SignUp_UserNotHrAdmin_ReturnsForbid()
        {
            SetUserRoles("EMPLOYEE");

            var request = new SignUpRequestDTO("joe", "joe@gmail.com", "Pwd123", "555");

            var result = await _controller.SignUp(request, _cancellationToken);

            Assert.IsType<ForbidResult>(result);
        } 
         
        [Fact]
        public async Task SignUp_UserHasMultipleRolesIncludingHrAdmin_ReturnsOk()
        {
            SetUserRoles("EMPLOYEE", "HR ADMIN");

            var request = new SignUpRequestDTO("multi", "multi@gmail.com", "Pwd123", "321");

            var response = new UserResponse
            {
                Id = 3,
                Email = "multi@gmail.com",
                UserName = "multi",
                PhoneNumber = "321",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "EMPLOYEE" }
            };

            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, UserResponse>(
                HttpMethod.Post, "/api/users/signup", _cancellationToken, request))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(response));

            var result = await _controller.SignUp(request, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(ok.Value);
            data.Data.UserName.Should().Be("multi");
        }
          
        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenHRAdminDeletesUser()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "10"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                }
            };

            var userIdToDelete = 3;

            _mockHttpClient
                .Setup(x => x.SendAsync<object, bool>(
                    HttpMethod.Delete, $"/api/users/delete-user/{userIdToDelete}", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await controller.DeleteAsync(userIdToDelete, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            returnedValue.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ServerReturnsNull_ReturnsBadRequest()
        {
            // Arrange
            var userId = 10;

            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                HttpMethod.Delete, $"/api/users/delete-user/{userId}", It.IsAny<CancellationToken>(), It.IsAny<DelUserBindingDTO>()))
                .ReturnsAsync((IdentityResult<bool>?)null); // Edge case

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            // Act
            var result = await _controller.DeleteAsync(userId, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Unexpected null result.", returnValue.Error);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnOk_WhenDeletionIsSuccessful()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = CancellationToken.None;
            var successResult = IdentityResult<bool>.Success(true);

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenResultIsNull()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = CancellationToken.None;

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync((IdentityResult<bool>)null);

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        } 

        [Fact]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenApiReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = CancellationToken.None;
            var failureResult = IdentityResult<bool>.Failure("API Error");

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenDeserializationFails()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = CancellationToken.None;
            var apiErrorResult = IdentityResult<bool>.Failure("Deserialization error");

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync(apiErrorResult);

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenHttpClientReturnsApiError()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = CancellationToken.None;
            var errorResult = IdentityResult<bool>.Failure("API Error: 500 Internal Server Error");

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync(errorResult);

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
         
        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 9999;   
            var cancellationToken = CancellationToken.None;

            _mockHttpClient
                .Setup(client => client.SendAsync<DelUserBindingDTO, bool>(
                    HttpMethod.Delete,
                    $"/api/users/delete-user/{userId}",
                    cancellationToken, null))
                .ReturnsAsync(IdentityResult<bool>.Failure("User not found"));

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var userId = 0;  
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _controller.DeleteAsync(userId, cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        } 
         
        [Fact]
        public async Task GetAllUsers_WhenUserManagerReturnsUnexpectedNull_ReturnsBadRequest()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            _mockHttpClient.Setup(client => client.SendAsync<List<UserResponse>>(
                    HttpMethod.Get, "/api/users/all-users", cancellationToken))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Failure("Unexpected null result."));

            // Act
            var result = await _controller.GetAllUsers(cancellationToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var identityResult = Assert.IsAssignableFrom<IdentityResult<List<UserResponse>>>(badRequestResult.Value);
            Assert.False(identityResult.IsSuccess);
            Assert.Equal("Unexpected null result.", identityResult.Error);
        }

        [Fact] 
        public async Task GetAllUsers_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var users = new List<UserResponse>
            {
                new UserResponse
                {
                    Id = 1,
                    UserName = "lilia",
                    Email = "lilia@google.com",
                    PhoneNumber = "123-456-7890",
                    DateCreated = DateTime.UtcNow,
                    Roles = new List<string> { "Admin" }
                },
                new UserResponse
                {
                    Id = 2,
                    UserName = "silveto",
                    Email = "silveto@google.com",
                    PhoneNumber = null,
                    DateCreated = DateTime.UtcNow,
                    Roles = new List<string> { "User" }
                }
            };
             

            _mockHttpClient.Setup(x => x.SendAsync<object, List<UserResponse>>(
                    HttpMethod.Get,
                    "/api/users/all-users",
                    It.IsAny<CancellationToken>(),
                    null
            ))
            .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(users));


            // Act
            var result = await _controller.GetAllUsers(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<List<UserResponse>>>(okResult.Value);
            value.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUsers_AsHrAdmin_ReturnsOk()
        {
            var users = GetSampleUsers();
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(users));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(ok.Value);
            data.IsSuccess.Should().BeTrue();
            data.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUsers_AsManager_ReturnsOk()
        {
            SetUserRoles("MANAGER");

            var users = GetSampleUsers();
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(users));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(ok.Value);
            data.Data.Count.Should().Be(2);
        }
         
        [Fact]
        public async Task GetAllUsers_ApiReturnsFailure_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Failure("API failure"));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(badRequest.Value);
            data.Error.Should().Be("API failure");
        }

        [Fact]
        public async Task GetAllUsers_ApiReturnsNull_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync((IdentityResult<List<UserResponse>>?)null);

            var result = await _controller.GetAllUsers(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(badRequest.Value);
            data.IsSuccess.Should().BeFalse();
            data.Error.Should().Be("Unexpected null result.");
        }

        [Fact]
        public async Task GetAllUsers_ApiReturnsSuccessWithNullData_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(null));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(badRequest.Value);
            data.Error.Should().Be("Result was successful, but no data was returned.");
        }

        [Fact]
        public async Task GetAllUsers_EmptyListReturned_ReturnsOkWithEmptyList()
        {
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(new List<UserResponse>()));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(ok.Value);
            data.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUsers_ExceptionThrown_ReturnsException()
        {
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ThrowsAsync(new Exception("Unexpected error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllUsers(_cancellationToken));
        }

        [Fact]
        public async Task GetAllUsers_UserWithMultipleRolesStillWorks()
        {
            SetUserRoles("MANAGER", "HR ADMIN");

            var users = GetSampleUsers();
            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(users));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(ok.Value);
            data.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUsers_ResponseContainsExpectedUserFields()
        {
            var users = new List<UserResponse>
            {
                new UserResponse
                {
                    Id = 1,
                    Email = "a@gmail.com",
                    UserName = "alpha",
                    PhoneNumber = "123",
                    DateCreated = DateTime.UtcNow,
                    Roles = new List<string> { "EMPLOYEE" }
                }
            };

            _mockHttpClient.Setup(x => x.SendAsync<SignUpRequestDTO, List<UserResponse>>(
                HttpMethod.Get, "/api/users/all-users", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<List<UserResponse>>.Success(users));

            var result = await _controller.GetAllUsers(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<UserResponse>>>(ok.Value);
            data.Data[0].UserName.Should().Be("alpha");
            data.Data[0].Email.Should().Be("a@gmail.com");
        }
         
        [Fact]
        public async Task ResetUserPasswordAsync_ReturnsBadRequest_IfUserTriesToResetOthersPassword()
        {
            // Arrange 
            var request = new ResetPasswordRequest(2, "Alabala123!@#");

            // Act
            var result = await _controller.ResetUserPasswordAsync(request, CancellationToken.None);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You are not admin and can change your own password only.", badResult.Value);
        }
         
        [Fact]
        public async Task SignIn_Successful_ReturnsOk()
        {
            // Arrange
            var signInRequest = new SignInRequestDTO
            {
                Email = "TestUser",
                Password = "Password123"
            };

            var signInResponse = new SignInResponseDTO
            {
                Token = "mock_token",
                User = new UserDto(1, "TestUser", "test@gmail.com", "Password123", "1234567890", new[] { "EMPLOYEE" }.ToList())
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Success(signInResponse));

            // Act
            var result = await _controller.SignIn(signInRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<SignInResponseDTO>>(okResult.Value);

            Assert.True(returnValue.IsSuccess);
            Assert.Equal("mock_token", returnValue.Data.Token);
            Assert.Equal("TestUser", returnValue.Data.User.UserName);
            Assert.Equal("test@gmail.com", returnValue.Data.User.Email);
        }

        [Fact]
        public async Task SignIn_Failed_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = new SignInRequestDTO
            {
                Email = "InvalidUser",
                Password = "WrongPassword"
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("Invalid username or password"));

            // Act
            var result = await _controller.SignIn(signInRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<SignInResponseDTO>>(badRequestResult.Value);

            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Invalid username or password", returnValue.Error);
        }


        [Fact]
        public async Task SignIn_NullSignInRequest_ReturnsBadRequest()
        {
            // Arrange
            SignInRequestDTO signInRequest = null;
             
            _mockHttpClient.Setup(client => client.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("Invalid request"));

            // Act
            var result = await _controller.SignIn(signInRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<SignInResponseDTO>>(badRequestResult.Value);

            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Invalid request", returnValue.Error);
        }

        [Fact]
        public async Task SignIn_EmptyUsernameOrPassword_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = new SignInRequestDTO
            {
                Email = string.Empty,  
                Password = string.Empty   
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("Username and password are required"));

            // Act
            var result = await _controller.SignIn(signInRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<SignInResponseDTO>>(badRequestResult.Value);

            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Username and password are required", returnValue.Error);
        }

        [Fact]
        public async Task SignIn_InternalServerError_ReturnsServerError()
        {
            // Arrange
            var signInRequest = new SignInRequestDTO
            {
                Email = "TestUser",
                Password = "Password123"
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                    HttpMethod.Post, "/api/users/signin", It.IsAny<CancellationToken>(), signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("Internal server error"));

            // Act
            var result = await _controller.SignIn(signInRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<SignInResponseDTO>>(badRequestResult.Value);

            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Internal server error", returnValue.Error);
        }

        private SignInRequestDTO GetValidSignInRequest()
        {
            return new SignInRequestDTO
            {
                Email = "test@gmail.com",
                Password = "Password123"
            };
        }

        private SignInResponseDTO GetValidSignInResponse()
        {
            return new SignInResponseDTO
            {
                Token = "valid-token",
                User = new UserDto(1, "TestUser", "test@gmail.com", "Password123", "1234567890", new List<string> { "EMPLOYEE" })
            };
        }

        [Fact]
        public async Task SignIn_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            var signInResponse = GetValidSignInResponse();
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Success(signInResponse));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(okResult.Value);
            data.IsSuccess.Should().BeTrue();
            data.Data.Token.Should().Be("valid-token");
        }

        [Fact]
        public async Task SignIn_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("Invalid credentials"));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(badRequest.Value);
            data.Error.Should().Be("Invalid credentials");
        }
   
        [Fact]
        public async Task SignIn_ApiReturnsNullResult_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync((IdentityResult<SignInResponseDTO>?)null);

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(badRequest.Value);
            data.IsSuccess.Should().BeFalse();
            data.Error.Should().Be("Unexpected null result.");
        }

        [Fact]
        public async Task SignIn_ApiReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("API error"));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(badRequest.Value);
            data.Error.Should().Be("API error");
        }

        [Fact]
        public async Task SignIn_ValidTokenInResponse_ReturnsOk()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            var signInResponse = new SignInResponseDTO
            {
                Token = "valid-token",
                User = new UserDto(1, "TestUser", "test@gmail.com", "Password123", "1234567890", new List<string> { "EMPLOYEE" })
            };
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Success(signInResponse));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(okResult.Value);
            data.Data.Token.Should().Be("valid-token");
        }
          
        [Fact]
        public async Task SignIn_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Failure("User not found"));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(badRequest.Value);
            data.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task SignIn_SuccessfulSignIn_ReturnsUserDetails()
        {
            // Arrange
            var signInRequest = GetValidSignInRequest();
            var signInResponse = new SignInResponseDTO
            {
                Token = "valid-token",
                User = new UserDto(1, "TestUser", "test@gmail.com", "Password123", "1234567890", new List<string> { "EMPLOYEE" })
            };
            _mockHttpClient.Setup(x => x.SendAsync<SignInRequestDTO, SignInResponseDTO>(
                HttpMethod.Post, "/api/users/signin", _cancellationToken, signInRequest))
                .ReturnsAsync(IdentityResult<SignInResponseDTO>.Success(signInResponse));

            // Act
            var result = await _controller.SignIn(signInRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<SignInResponseDTO>>(okResult.Value);
            data.Data.User.UserName.Should().Be("TestUser");
        }  

        [Fact]
        public async Task UpdateAsync_ValidRequest_ReturnsOk()
        {
            // Arrange
            var id = 1;
            var updateRequest = new UpdateUserRequest("updatedemail@gmail.com", "1234567890");
            var userResponse = new UserResponse
            {
                Id = 1,
                Email = "updatedemail@gmail.com",
                UserName = "TestUser",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.Now,
                Roles = new List<string> { "MANAGER" }
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<UpdateUserRequest, UserResponse>(
                    HttpMethod.Patch, $"/api/users/update-user/{id}", It.IsAny<CancellationToken>(), updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.UpdateAsync(id, updateRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<UserResponse>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(id, returnValue.Data.Id);
            Assert.Equal("updatedemail@gmail.com", returnValue.Data.Email);
            Assert.Equal("1234567890", returnValue.Data.PhoneNumber);
        }

        [Fact]
        public async Task UpdateAsync_UserSuccessfullyUpdated_ReturnsOk()
        {
            // Arrange
            var id = 1;  
            var updateRequest = new UpdateUserRequest("updatedemail@gmail.com", "9876543210");

            var updatedUser = new UserResponse
            {
                Id = id,
                UserName = "TestUser",
                Email = "updatedemail@gmail.com",
                PhoneNumber = "9876543210",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "HR ADMIN" }
            };
             
            _mockHttpClient.Setup(client => client.SendAsync<UpdateUserRequest, UserResponse>(
                    HttpMethod.Patch, $"/api/users/update-user/{id}", It.IsAny<CancellationToken>(), updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(updatedUser));
             
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            // Act
            var result = await _controller.UpdateAsync(id, updateRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);  
            var returnValue = Assert.IsAssignableFrom<IdentityResult<UserResponse>>(okResult.Value);    

            Assert.True(returnValue.IsSuccess); // Assert success
            Assert.Equal("updatedemail@gmail.com", returnValue.Data.Email); 
            Assert.Equal("9876543210", returnValue.Data.PhoneNumber);  
        }

        [Fact]
        public async Task UpdateAsync_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var id = 1; // Existing user
            var updateRequest = new UpdateUserRequest(null, null);  
             
            _mockHttpClient.Setup(client => client.SendAsync<UpdateUserRequest, UserResponse>(
                    HttpMethod.Patch, $"/api/users/update-user/{id}", It.IsAny<CancellationToken>(), updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Failure("Invalid input"));

            // Simulate HR ADMIN role
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            // Act
            var result = await _controller.UpdateAsync(id, updateRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);   
            var returnValue = Assert.IsAssignableFrom<IdentityResult<UserResponse>>(badRequestResult.Value);       

            Assert.False(returnValue.IsSuccess);  
            Assert.Equal("Invalid input", returnValue.Error);  
        }

        [Fact]
        public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            var updateRequest = new UpdateUserRequest("", "1234567890"); // Invalid email (empty)
             
            _mockHttpClient.Setup(client => client.SendAsync<UpdateUserRequest, UserResponse>(
                    HttpMethod.Patch, $"/api/users/update-user/{id}", It.IsAny<CancellationToken>(), updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Failure("Invalid input"));

            // Act
            var result = await _controller.UpdateAsync(id, updateRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<UserResponse>>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Invalid input", returnValue.Error);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOk_WhenManagerUpdatesUser()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                }
            };

            var updateRequest = new UpdateUserRequest("gergana@google.com", "555-0000");
            var updatedUser = new UserResponse
            {
                Id = 2,
                UserName = "gergana",
                Email = "gergana@google.com",
                PhoneNumber = "555-0000",
                DateCreated = DateTime.Now,
                Roles = new List<string> { "EMPLOYEE" }
            };

            _mockHttpClient
                .Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                    HttpMethod.Patch,
                    "/api/users/update-user/2",
                    It.IsAny<CancellationToken>(),
                    updateRequest)
                ).ReturnsAsync(IdentityResult<UserResponse>.Success(updatedUser));

            // Act
            var result = await controller.UpdateAsync(2, updateRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            value.Data.UserName.Should().Be("gergana");
        }

        private UpdateUserRequest GetValidUpdateRequest() =>
            new UpdateUserRequest("newemail@gmail.com", "1234567890"); 

        private UserResponse GetValidUserResponse()
        {
            return new UserResponse
            {
                Id = 1,
                UserName = "TestUser",
                Email = "newemail@gmail.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "EMPLOYEE" }
            };
        }

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ReturnsOk()
        {
            // Arrange
            var updateRequest = GetValidUpdateRequest();
            var userResponse = GetValidUserResponse();
            _mockHttpClient.Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                HttpMethod.Patch, "/api/users/update-user/1", _cancellationToken, updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.UpdateAsync(1, updateRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            data.IsSuccess.Should().BeTrue();
            data.Data.Email.Should().Be("newemail@gmail.com");
        }
 
        [Fact]
        public async Task UpdateAsync_UpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = GetValidUpdateRequest();
            _mockHttpClient.Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                HttpMethod.Patch, "/api/users/update-user/1", _cancellationToken, updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Failure("Failed to update user"));

            // Act
            var result = await _controller.UpdateAsync(1, updateRequest, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(badRequest.Value);
            data.Error.Should().Be("Failed to update user");
        }

        [Fact]
        public async Task UpdateAsync_PhoneNumberUpdated_ReturnsUpdatedUser()
        {
            // Arrange
            var updateRequest = new UpdateUserRequest("newemail@gmail.com", "9876543210");
            var userResponse = new UserResponse
            {
                Id = 1,
                UserName = "TestUser",
                Email = "newemail@gmail.com",
                PhoneNumber = "9876543210",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "EMPLOYEE" }
            };
            _mockHttpClient.Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                HttpMethod.Patch, "/api/users/update-user/1", _cancellationToken, updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.UpdateAsync(1, updateRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            data.Data.PhoneNumber.Should().Be("9876543210");
        }

        [Fact]
        public async Task UpdateAsync_EmailAndPhoneNumberUpdated_ReturnsUpdatedUser()
        {
            // Arrange
            var updateRequest = new UpdateUserRequest("updated@gmail.com", "9876543210");
            var userResponse = new UserResponse
            {
                Id = 1,
                UserName = "TestUser",
                Email = "updated@gmail.com",
                PhoneNumber = "9876543210",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "EMPLOYEE" }
            };
            _mockHttpClient.Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                HttpMethod.Patch, "/api/users/update-user/1", _cancellationToken, updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.UpdateAsync(1, updateRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            data.Data.Email.Should().Be("updated@gmail.com");
            data.Data.PhoneNumber.Should().Be("9876543210");
        }

        [Fact]
        public async Task UpdateAsync_UserRoleUpdated_ReturnsUpdatedUser()
        {
            // Arrange
            var updateRequest = new UpdateUserRequest ("updated@gmail.com", "9876543210");
            var userResponse = new UserResponse
            {
                Id = 1,
                UserName = "TestUser",
                Email = "updated@gmail.com",
                PhoneNumber = "9876543210",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "HR ADMIN", "EMPLOYEE" }
            };
            _mockHttpClient.Setup(x => x.SendAsync<UpdateUserRequest, UserResponse>(
                HttpMethod.Patch, "/api/users/update-user/1", _cancellationToken, updateRequest))
                .ReturnsAsync(IdentityResult<UserResponse>.Success(userResponse));

            // Act
            var result = await _controller.UpdateAsync(1, updateRequest, _cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<UserResponse>>(okResult.Value);
            data.Data.Roles.Should().Contain("HR ADMIN");
        } 
         
        [Fact]
        public async Task GetUserInfo_ShouldReturnUnauthorized_WhenUserTriesToAccessAnotherUsersInfo()
        {
            // Arrange
            var userId = 2;  
            var currentUserId = 1;  

            // Act
            var result = await _controller.GetUserInfo(userId, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You can only see your own profile information.", actionResult.Value);
        } 

        [Fact]
        public async Task GetUserInfo_SameUserId_ReturnsOk()
        {
            // Arrange
            var userInfo = new UserInfoResponse(1, "User1", "user1@gmail.com", "1234567890", DateTime.UtcNow);
            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(userInfo));

            // Act
            var result = await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserInfo_DifferentUserId_ReturnsUnauthorized()
        {
            // Act
            var result = await _controller.GetUserInfo(2, CancellationToken.None);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>()
                  .Which.Value.Should().Be("You can only see your own profile information.");
        }
         
        [Fact]
        public async Task GetUserInfo_SendAsyncReturnsNull_ReturnsBadRequest()
        {
            // Arrange
            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync((IdentityResult<UserInfoResponse>?)null);

            // Act
            var result = await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUserInfo_SendAsyncReturnsSuccessButNullData_ReturnsBadRequest()
        {
            // Arrange
            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(null));

            // Act
            var result = await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUserInfo_SendAsyncReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var failedResult = IdentityResult<UserInfoResponse>.Failure("User not found");
            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUserInfo_ManagerRole_ReturnsOk()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Role, "MANAGER")
            }, "mock"));

            var userInfo = new UserInfoResponse(2, "ManagerUser", "manager@gmail.com", "1111111111", DateTime.UtcNow);

            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/2", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(userInfo));

            // Act
            var result = await _controller.GetUserInfo(2, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        } 
      

        [Fact]
        public async Task GetUserInfo_CallsSendAsyncWithCorrectUrl()
        {
            // Arrange
            var userInfo = new UserInfoResponse(1, "User1", "user1@gmail.com", "1234567890", DateTime.UtcNow);

            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(userInfo))
                .Verifiable();

            // Act
            var result = await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            _mockHttpClient.Verify();
        }

        [Fact]
        public async Task GetUserInfo_WhenExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            _mockHttpClient.Setup(client => client.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(), null))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            Func<Task> act = async () => await _controller.GetUserInfo(1, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
         
        [Fact]
        public async Task GetUserInfo_ResultIsNull_ReturnsBadRequest()
        {
            _mockHttpClient
                .Setup(x => x.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", _cancellationToken, null))
                .ReturnsAsync((IdentityResult<UserInfoResponse>?)null);

            // Act
            var result = await _controller.GetUserInfo(1, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<IdentityResult<UserInfoResponse>>(badRequest.Value);
            response.IsSuccess.Should().BeFalse();
            response.Error.Should().Be("Unexpected null result.");
        }

        [Fact]
        public async Task GetUserInfo_ResultSuccessButDataNull_ReturnsBadRequest()
        {
            _mockHttpClient
                .Setup(x => x.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(null));

            // Act
            var result = await _controller.GetUserInfo(1, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<IdentityResult<UserInfoResponse>>(badRequest.Value);
            response.IsSuccess.Should().BeFalse();
            response.Error.Should().Be("Result was successful, but no data was returned.");
        }

        [Fact]
        public async Task GetUserInfo_ResultFailed_ReturnsBadRequest()
        {
            _mockHttpClient
                .Setup(x => x.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<UserInfoResponse>.Failure("API failure"));

            // Act
            var result = await _controller.GetUserInfo(1, _cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<IdentityResult<UserInfoResponse>>(badRequest.Value);
            response.IsSuccess.Should().BeFalse();
            response.Error.Should().Be("API failure");
        } 

        [Fact]
        public async Task GetUserInfo_HrAdminRole_ReturnsOk()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var expected = new UserInfoResponse(1, "hr", "hr@gmail.com", "999999", DateTime.UtcNow);
            _mockHttpClient
                .Setup(x => x.SendAsync<DelUserBindingDTO, UserInfoResponse>(
                    HttpMethod.Get, "/api/users/me/info/1", It.IsAny<CancellationToken>(),null))
            .ReturnsAsync(IdentityResult<UserInfoResponse>.Success(expected)); 


            var result = await _controller.GetUserInfo(1, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<IdentityResult<UserInfoResponse>>(okResult.Value);
            response.Data.Should().BeEquivalentTo(expected);
        } 
          

        [Fact]
        public async Task GetUserInfo_ReturnsUnauthorized_IfAccessingOthersInfo()
        {
            var result = await _controller.GetUserInfo(2, CancellationToken.None);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You can only see your own profile information.", unauthorized.Value);
        }
          
        [Fact]
        public async Task AssignRoleToUser_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "10"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                        }
            };

            var request = new AssignRoleRequest(1, 2);

            _mockHttpClient
                .Setup(x => x.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await controller.AssignRoleToUser(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();

            if (okResult.Value is IdentityResult<bool> identityResult)
            {
                identityResult.IsSuccess.Should().BeTrue();
                identityResult.Data.Should().BeTrue();
            }
            else if (okResult.Value is bool b)
            {
                b.Should().BeTrue();
            }
            else
            {
                throw new Exception($"Unexpected return type: {okResult.Value.GetType().Name}");
            }
        } 

        [Fact]
        public async Task CreateRole_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "10"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                        }
            };

            var role = new RoleBindingDTO("TEAM_LEAD", "TEAM_LEAD DESCRIPTION ALABALA");

            _mockHttpClient
                .Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                    HttpMethod.Post, "/api/users/create-role", It.IsAny<CancellationToken>(), role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await controller.CreateRole(role, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();

            if (okResult.Value is IdentityResult<bool> identityResult)
            {
                identityResult.IsSuccess.Should().BeTrue();
                identityResult.Data.Should().BeTrue();
            }
            else if (okResult.Value is bool b)
            {
                b.Should().BeTrue();
            }
            else
            {
                throw new Exception($"Unexpected return type: {okResult.Value.GetType().Name}");
            }
        }
          
        [Fact]
        public async Task CreateRole_ValidRole_ReturnsOk()
        {
            var role = new RoleBindingDTO("Admin", "Has all privileges");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
            Assert.True(identityResult.Data);
        }

        [Fact]
        public async Task CreateRole_ApiReturnsFailure_ReturnsBadRequest()
        {
            var role = new RoleBindingDTO("Reader", "Can only read");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Failure("API Error"));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(identityResult.IsSuccess);
            Assert.Equal("API Error", identityResult.Error);
        }

        [Fact]
        public async Task CreateRole_ResultIsNull_ReturnsBadRequest()
        {
            var role = new RoleBindingDTO("Manager", "Manages teams");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync((IdentityResult<bool>?)null);

            var result = await _controller.CreateRole(role, _cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(identityResult.IsSuccess);
            Assert.Equal("Unexpected null result.", identityResult.Error);
        }

        [Fact]
        public async Task CreateRole_SuccessButDataIsFalse_ReturnsOkWithFalse()
        {
            var role = new RoleBindingDTO("Observer", "Read-only role");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(false));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
            Assert.False(identityResult.Data);
        }

        [Fact]
        public async Task CreateRole_NameIsNull_ReturnsOk_IfApiReturnsSuccess()
        {
            var role = new RoleBindingDTO(null, "Some description");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
        }

        [Fact]
        public async Task CreateRole_DescriptionIsNull_ReturnsOk()
        {
            var role = new RoleBindingDTO("Viewer", null);

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
        }

        [Fact]
        public async Task CreateRole_EmptyRoleName_ReturnsOk()
        {
            var role = new RoleBindingDTO(string.Empty, "Some role");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.CreateRole(role, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
        }

        [Fact]
        public async Task CreateRole_HttpClientThrowsException_ReturnsBadRequest()
        {
            var role = new RoleBindingDTO("Backup", "Handles backups");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ThrowsAsync(new Exception("Unexpected error"));
             
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateRole(role, _cancellationToken));
        }

        [Fact]
        public async Task CreateRole_NotHRAdmin_ShouldFailAuthorization_WhenPolicyApplied()
        { 
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "2"),
                        new Claim(ClaimTypes.Role, "EMPLOYEE")  
                    }, "mock"))
                }
            };

            var role = new RoleBindingDTO("EmployeeRole", "Some description");
             
            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await controller.CreateRole(role, _cancellationToken);
            var ok = Assert.IsType<OkObjectResult>(result);
            var identityResult = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(identityResult.IsSuccess);
        }
         
        [Fact]
        public async Task UpdateRole_ReturnsOk_WhenHRAdminUpdatesRole()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"))
                }
            };

            var roleDto = new RoleBindingDTO("TEAM_LEAD", "Updated Description");
             
            _mockHttpClient
                .Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                    HttpMethod.Patch, "/api/users/update-role/3", It.IsAny<CancellationToken>(), roleDto))
                .ReturnsAsync(IdentityResult<bool>.Success(true));   

            // Act
            var result = await controller.UpdateRole(3, roleDto, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);  
            Assert.NotNull(okResult);  
             
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
             
            value.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateRole_ValidInput_ReturnsOk()
        {
            var role = new RoleBindingDTO("UpdatedRole", "Updated Description");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/1", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.UpdateRole(1, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
            Assert.True(value.Data);
        }

        [Fact]
        public async Task UpdateRole_ApiReturnsFalse_ReturnsOkWithFalse()
        {
            var role = new RoleBindingDTO("RoleX", "Some Description");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/2", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(false));

            var result = await _controller.UpdateRole(2, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
            Assert.False(value.Data);
        }

        [Fact]
        public async Task UpdateRole_ApiFailure_ReturnsBadRequest()
        {
            var role = new RoleBindingDTO("ErrorRole", "Fail desc");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/3", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Failure("Update failed"));

            var result = await _controller.UpdateRole(3, role, _cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("Update failed", value.Error);
        }

        [Fact]
        public async Task UpdateRole_NullResult_ReturnsBadRequest()
        {
            var role = new RoleBindingDTO("NullRole", "Null return");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/4", _cancellationToken, role))
                .ReturnsAsync((IdentityResult<bool>?)null);

            var result = await _controller.UpdateRole(4, role, _cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("Unexpected null result.", value.Error);
        }

        [Fact]
        public async Task UpdateRole_NameIsNull_ReturnsOkIfApiSucceeds()
        {
            var role = new RoleBindingDTO(null, "Just description");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/5", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.UpdateRole(5, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
        }

        [Fact]
        public async Task UpdateRole_DescriptionIsNull_ReturnsOkIfApiSucceeds()
        {
            var role = new RoleBindingDTO("RoleOnly", null);

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/6", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.UpdateRole(6, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
        }

        [Fact]
        public async Task UpdateRole_EmptyRoleName_ReturnsOkIfApiSucceeds()
        {
            var role = new RoleBindingDTO(string.Empty, "Empty name");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/7", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.UpdateRole(7, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
        }

        [Fact]
        public async Task UpdateRole_IdIsZero_StillCallsApi()
        {
            var role = new RoleBindingDTO("ZeroId", "Edge case");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/0", _cancellationToken, role))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.UpdateRole(0, role, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            Assert.True(value.IsSuccess);
        }

        [Fact]
        public async Task UpdateRole_HttpClientThrowsException_ShouldThrow()
        {
            var role = new RoleBindingDTO("ThrowRole", "Throws");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Patch, "/api/users/update-role/8", _cancellationToken, role))
                .ThrowsAsync(new Exception("Unexpected failure"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateRole(8, role, _cancellationToken));
        }
  
        [Fact]
        public async Task DeleteRole_ReturnsForbidden_IfNotHRAdmin()
        {
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "2"),
                        new Claim(ClaimTypes.Role, "EMPLOYEE")
                    }, "mock"))
                }
            };

            var result = await controller.DeleteRole(3, CancellationToken.None);

            Assert.IsType<ForbidResult>(result);  
        }

        [Fact]
        public async Task ResetUserPasswordAsync_ReturnsOk_WhenUserResetsOwnPassword()
        {
            var request = new ResetPasswordRequest(1, "MyNewPassword123!");

            _mockHttpClient
                .Setup(x => x.SendAsync<ResetPasswordRequest, bool>(
                    HttpMethod.Post, "/api/users/reset-password", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _controller.ResetUserPasswordAsync(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            value.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsOk_ForHRAdmin()
        {
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "99"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                }
            };

            var request = new ResetPasswordRequest(5, "SecurePass123!");

            _mockHttpClient
                .Setup(x => x.SendAsync<ResetPasswordRequest, bool>(
                    HttpMethod.Post, "/api/users/admin/reset-password", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await controller.ResetPasswordAsync(request, CancellationToken.None);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            value.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ResetUserPasswordAsync_ValidRequest_ReturnsOk()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_UserIdDoesNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(2, "newPassword123");

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_NullResultFromSendAsync_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync((IdentityResult<bool>)null);

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_FailedResultFromSendAsync_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync(IdentityResult<bool>.Failure("API Error"));

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_SuccessfulButNullData_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync(IdentityResult<bool>.Failure(null));

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_SuccessfulResult_ReturnsOk()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }
          
        [Fact]
        public async Task ResetUserPasswordAsync_UserNotAuthorizedToResetPassword_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(2, "newPassword123");
            _mockHttpClient.Setup(m => m.SendAsync<ResetPasswordRequest, bool>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                _cancellationToken,
                resetPasswordRequest
            )).ReturnsAsync(IdentityResult<bool>.Failure("API Error"));

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_NoUserInClaims_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "newPassword123");
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = null };

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }


        [Fact]
        public async Task ResetUserPasswordAsync_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest(1, "");

            // Act
            var result = await _controller.ResetUserPasswordAsync(resetPasswordRequest, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        } 

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_ReturnsOk()
        {
            var request = new ResetPasswordRequest(1, "NewSecurePassword123");

            _mockHttpClient.Setup(c => c.SendAsync<ResetPasswordRequest, bool>(
                    HttpMethod.Post, "/api/users/admin/reset-password", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<bool>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.True(returnValue.Data);
        }
         
        [Fact]
        public async Task ResetPasswordAsync_EmptyPassword_ReturnsBadRequest()
        {
            var request = new ResetPasswordRequest(1, "");

            _controller.ModelState.AddModelError("NewPassword", "Password is required");

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidUserId_ReturnsBadRequest()
        {
            var request = new ResetPasswordRequest(-1, "ValidPassword123");

            _controller.ModelState.AddModelError("Id", "Invalid user ID");

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_WeakPassword_ReturnsBadRequest()
        {
            var request = new ResetPasswordRequest(1, "123");

            _mockHttpClient.Setup(c => c.SendAsync<ResetPasswordRequest, bool>(
                    HttpMethod.Post, "/api/users/admin/reset-password", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Failure("Password too weak"));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IdentityResult<bool>>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Password too weak", returnValue.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_NullResult_ReturnsBadRequest()
        {
            var request = new ResetPasswordRequest(1, "NewPassword");

            _mockHttpClient.Setup(c => c.SendAsync<ResetPasswordRequest, bool>(
                HttpMethod.Post, "/api/users/admin/reset-password", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync((IdentityResult<bool>?)null);

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Unexpected null result.", returnValue.Error);
        }
         
        [Fact]
        public async Task ResetPasswordAsync_NullPassword_ReturnsBadRequest()
        {
            var request = new ResetPasswordRequest(1, null);

            _controller.ModelState.AddModelError("NewPassword", "Password must not be null");

            var result = await _controller.ResetPasswordAsync(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        }
         
        [Fact]
        public async Task DeleteRole_ReturnsForbidden_WhenUserIsNotHRAdmin()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "2"),
                        new Claim(ClaimTypes.Role, "EMPLOYEE")  
                    }, "mock"))
                }
            };

            var roleId = 1;   

            // Act
            var result = await controller.DeleteRole(roleId, CancellationToken.None);
             
            var forbidResult = Assert.IsType<ForbidResult>(result);  
            Assert.NotNull(forbidResult);
        }

          
        [Fact]
        public async Task AssignRoleToUser_ReturnsBadRequest_WhenAssignmentFails()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                }
            };

            var request = new AssignRoleRequest(1, 3);

            _mockHttpClient
                .Setup(x => x.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Failure("Assignment failed"));

            // Act
            var result = await controller.AssignRoleToUser(request, CancellationToken.None);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
             
            badResult.Value.Should().NotBeNull();
             
            if (badResult.Value is IdentityResult<bool> identityResult)
            {
                identityResult.IsSuccess.Should().BeFalse();
                identityResult.Error.Should().Contain("Assignment failed");
            }
            else
            { 
                var errorMessage = badResult.Value?.ToString();
                errorMessage.Should().Contain("Assignment failed");
            }
        } 

        [Fact]
        public async Task AssignRoleToUser_ValidRequest_ReturnsOk()
        {
            var request = new AssignRoleRequest(1, 2);

            _mockHttpClient.Setup(c => c.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(value.IsSuccess);
            Assert.True(value.Data);
        }

        [Fact]
        public async Task AssignRoleToUser_UserNotFound_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(999, 1);

            _mockHttpClient.Setup(c => c.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Failure("User not found"));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("User not found", value.Error);
        }


        [Fact]
        public async Task AssignRoleToUser_RoleNotFound_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(1, 999);

            _mockHttpClient.Setup(c => c.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Failure("Role not found"));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("Role not found", value.Error);
        }
         
        [Fact]
        public async Task AssignRoleToUser_InvalidUserId_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(-1, 1);
            _controller.ModelState.AddModelError("UserId", "UserId must be positive");

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AssignRoleToUser_InvalidRoleId_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(1, -1);
            _controller.ModelState.AddModelError("RoleId", "RoleId must be positive");

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AssignRoleToUser_NullResult_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(1, 1);

            _mockHttpClient.Setup(c => c.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync((IdentityResult<bool>?)null);

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("Unexpected null result.", value.Error);
        } 

        [Fact]
        public async Task AssignRoleToUser_RoleAlreadyAssigned_ReturnsBadRequest()
        {
            var request = new AssignRoleRequest(1, 1);

            _mockHttpClient.Setup(c => c.SendAsync<AssignRoleRequest, bool>(
                    HttpMethod.Post, "/api/users/assign-role", It.IsAny<CancellationToken>(), request))
                .ReturnsAsync(IdentityResult<bool>.Failure("Role already assigned"));

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            var result = await _controller.AssignRoleToUser(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(badRequest.Value);
            Assert.False(value.IsSuccess);
            Assert.Equal("Role already assigned", value.Error);
        }
         
        [Fact]
        public async Task GetUserInfo_EmptyUserId_ReturnsUnauthorized()
        { 
            var result = await _controller.GetUserInfo(0, CancellationToken.None);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You can only see your own profile information.", unauthorized.Value);
        }
          
        [Fact]
        public async Task GetAllRoles_WhenCalledByHRAdmin_ReturnsOkWithRoles()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var roles = new List<RolesResponse>
            {
                new RolesResponse
                {
                    Id = 1,
                    Name = "HR ADMIN",
                    Description = "Full access",
                    DateCreated = DateTime.UtcNow
                },
                new RolesResponse
                {
                    Id = 2,
                    Name = "EMPLOYEE",
                    Description = "Limited access",
                    DateCreated = DateTime.UtcNow
                }
            };

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            _mockHttpClient.Setup(client => client.SendAsync<List<RolesResponse>>(
                    HttpMethod.Get,
                    "/api/users/admin/all-roles",
                    cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            // Act
            var result = await _controller.GetAllRoles(cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<List<RolesResponse>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count);
            Assert.Contains(returnValue.Data, r => r.Name == "HR ADMIN");
        }

        [Fact]
        public async Task GetAllRoles_WhenUserManagerFails_ReturnsBadRequest()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            const string errorMessage = "Failed to fetch roles";

            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR ADMIN")
            }, "mock"));

            _mockHttpClient.Setup(client => client.SendAsync<List<RolesResponse>>(
                    HttpMethod.Get,
                    "/api/users/admin/all-roles",
                    cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Failure(errorMessage));

            // Act
            var result = await _controller.GetAllRoles(cancellationToken);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<IdentityResult<List<RolesResponse>>>(badRequest.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal(errorMessage, returnValue.Error);
        }

        private List<RolesResponse> GetSampleRoles()
        {
            return new List<RolesResponse>
            {
                new RolesResponse { Name = "HR ADMIN", Description = "Admin role for HR" },
                new RolesResponse { Name = "EMPLOYEE", Description = "Basic user role" },
                new RolesResponse { Name = "MANAGER", Description = "Manager role with higher privileges" }
            };
        }

        [Fact]
        public async Task GetAllRoles_AsHrAdmin_ReturnsOk()
        {
            var roles = GetSampleRoles();
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.IsSuccess.Should().BeTrue();
            data.Data.Should().HaveCount(3);
        }
         
        [Fact]
        public async Task GetAllRoles_ApiReturnsFailure_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Failure("API failure"));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(badRequest.Value);
            data.Error.Should().Be("API failure");
        }

        [Fact]
        public async Task GetAllRoles_ApiReturnsNull_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync((IdentityResult<List<RolesResponse>>?)null);

            var result = await _controller.GetAllRoles(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(badRequest.Value);
            data.IsSuccess.Should().BeFalse();
            data.Error.Should().Be("Unexpected null result.");
        }

        [Fact]
        public async Task GetAllRoles_ApiReturnsSuccessWithNullData_ReturnsBadRequest()
        {
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(null));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(badRequest.Value);
            data.Error.Should().Be("Result was successful, but no data was returned.");
        }

        [Fact]
        public async Task GetAllRoles_EmptyListReturned_ReturnsOkWithEmptyList()
        {
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(new List<RolesResponse>()));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllRoles_ExceptionThrown_ReturnsException()
        {
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ThrowsAsync(new Exception("Unexpected error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllRoles(_cancellationToken));
        }

        [Fact]
        public async Task GetAllRoles_UserWithMultipleRolesStillWorks()
        {
            SetUserRoles("HR ADMIN", "MANAGER");

            var roles = GetSampleRoles();
            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllRoles_ResponseContainsExpectedRoleFields()
        {
            var roles = new List<RolesResponse>
            {
                new RolesResponse
                {
                    Name = "HR ADMIN",
                    Description = "Admin role for HR"
                }
            };

            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.Data[0].Name.Should().Be("HR ADMIN");
            data.Data[0].Description.Should().Be("Admin role for HR");
        }

        [Fact]
        public async Task GetAllRoles_LargeRolesListReturned_ReturnsCorrectCount()
        {
            var roles = new List<RolesResponse>();
            for (int i = 0; i < 100; i++)
            {
                roles.Add(new RolesResponse
                {
                    Name = $"Role {i}",
                    Description = $"Description for Role {i}"
                });
            }

            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.Data.Should().HaveCount(100);
        }

        [Fact]
        public async Task GetAllRoles_ResponseContainsCorrectRoleName()
        {
            var roles = new List<RolesResponse>
            {
                new RolesResponse { Name = "HR ADMIN", Description = "Admin role for HR" }
            };

            _mockHttpClient.Setup(x => x.SendAsync<List<RolesResponse>>(
                HttpMethod.Get, "/api/users/admin/all-roles", _cancellationToken))
                .ReturnsAsync(IdentityResult<List<RolesResponse>>.Success(roles));

            var result = await _controller.GetAllRoles(_cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<List<RolesResponse>>>(ok.Value);
            data.Data[0].Name.Should().Be("HR ADMIN");
        }
         
        [Fact]
        public async Task CreateRole_ReturnsOk_WhenCreationIsSuccessful()
        {
            // Arrange
            var controller = new PeopleController(_mockHttpClient.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "HR ADMIN")
                    }, "mock"))
                }
            };

            var roleDto = new RoleBindingDTO("TestRole", "Description");

            _mockHttpClient.Setup(x => x.SendAsync<RoleBindingDTO, bool>(
                HttpMethod.Post, "/api/users/create-role", It.IsAny<CancellationToken>(), roleDto))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await controller.CreateRole(roleDto, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<IdentityResult<bool>>(okResult.Value);
            value.IsSuccess.Should().BeTrue();
        } 
         
        [Fact]
        public async Task DeleteRole_UserNotInRole_HR_ADMIN_ReturnsForbid()
        {
            SetUserRoles("EMPLOYEE");

            var result = await _controller.DeleteRole(5, _cancellationToken);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteRole_UserWithNoRoles_ReturnsForbid()
        {
            SetUserRoles();  

            var result = await _controller.DeleteRole(6, _cancellationToken);

            Assert.IsType<ForbidResult>(result);
        }
          
        [Fact]
        public async Task DeleteRole_MultipleRolesIncludingHrAdmin_ReturnsOk()
        {
            SetUserRoles("HR ADMIN", "EMPLOYEE");

            _mockHttpClient.Setup(x => x.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete, "/api/users/delete-role/9", _cancellationToken, null))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var result = await _controller.DeleteRole(9, _cancellationToken);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<IdentityResult<bool>>(ok.Value);
            Assert.True(data.IsSuccess);
        }

        [Fact]
        public async Task DeleteRole_HRAdminCanDeleteRole_ReturnsOk()
        {
            // Arrange
            var roleId = 1;
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "HR ADMIN")
                }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        } 

        [Fact]
        public async Task DeleteRole_APIError_ReturnsBadRequest()
        {
            // Arrange
            var roleId = 1;
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "HR ADMIN")
        }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync(IdentityResult<bool>.Failure("API Error"));

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }

        [Fact]
        public async Task DeleteRole_NullResultFromAPI_ReturnsBadRequest()
        {
            // Arrange
            var roleId = 1;
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "HR ADMIN")
        }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync((IdentityResult<bool>)null);

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }
         
        [Fact]
        public async Task DeleteRole_SuccessfulWithData_ReturnsOk()
        {
            // Arrange
            var roleId = 1;
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "HR ADMIN")
                }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }


        [Fact]
        public async Task DeleteRole_InvalidRoleId_ReturnsBadRequest()
        {
            // Arrange
            var roleId = -1; // invalid roleId
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "HR ADMIN")
                }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync(IdentityResult<bool>.Failure("Invalid Role"));

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
        }
        [Fact]
        public async Task DeleteRole_HandlesErrorDuringDeletion_ReturnsBadRequest()
        {
            // Arrange
            var roleId = 1;
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "HR ADMIN")
                }, "mock"))
            };

            _mockHttpClient.Setup(m => m.SendAsync<DelRoleBindingDTO, bool>(
                HttpMethod.Delete,
                $"/api/users/delete-role/{roleId}",
                _cancellationToken,
                It.IsAny<DelRoleBindingDTO>()
            )).ReturnsAsync(IdentityResult<bool>.Failure("Role deletion error"));

            // Act
            var result = await _controller.DeleteRole(roleId, _cancellationToken);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, actionResult.StatusCode);
            }
         
        }
}