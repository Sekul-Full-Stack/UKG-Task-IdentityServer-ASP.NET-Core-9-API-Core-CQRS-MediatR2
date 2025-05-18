namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;
    using Xunit;

    using IdentityServer.Application.Commands.SignIn;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;
     
    public class SignInCommandHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IRoleManager> _roleManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly SignInCommandHandler _handler;

        public SignInCommandHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _roleManagerMock = new Mock<IRoleManager>();
            _tokenServiceMock = new Mock<ITokenService>();
            _handler = new SignInCommandHandler(_userManagerMock.Object, _roleManagerMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task Handle_SuccessfulSignIn_ReturnsSuccess()
        {
            var command = new SignInCommand { Email = "test@example.com", Password = "password" };

            var userId = 1;
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow,
                Roles = new List<string> { "Admin", "User" }
            };

            _userManagerMock.Setup(m => m.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _roleManagerMock.Setup(r => r.GetRolesAsync(userId))
                .ReturnsAsync(IdentityResult<IEnumerable<string>>.Success(new[] { "Admin", "User" }));

            _tokenServiceMock.Setup(t => t.GenerateToken(userId.ToString(), user, It.IsAny<IEnumerable<string>>()))
                .Returns(IdentityResult<string>.Success("valid_token"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("valid_token", result.Data.Token);
            Assert.Equal(userId, result.Data.User.Id);
        }

        [Fact]
        public async Task Handle_InvalidCredentials_ReturnsFailure()
        {
            var command = new SignInCommand { Email = "test@example.com", Password = "password" };


            _userManagerMock.Setup(m => m.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Failure("Invalid credentials"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid credentials", result.Error);
        } 

        [Fact]
        public async Task Handle_NoRolesAssigned_ReturnsSuccessWithEmptyRoles()
        {
            var command = new SignInCommand { Email = "test@example.com", Password = "password" };
            var user = new User
            {
                Id = 2,
                UserName = "testuser2",
                Email = "test2@example.com",
                PhoneNumber = "9876543210",
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(m => m.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _roleManagerMock.Setup(r => r.GetRolesAsync(user.Id))
                .ReturnsAsync(IdentityResult<IEnumerable<string>>.Success(Enumerable.Empty<string>()));

            _tokenServiceMock.Setup(t => t.GenerateToken(user.Id.ToString(), user, It.IsAny<IEnumerable<string>>()))
                .Returns(IdentityResult<string>.Success("token_no_roles"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("token_no_roles", result.Data.Token);
            Assert.Empty(result.Data.User.Roles);
        } 
          
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var command = new SignInCommand { Email = "test@google.com", Password = "ValidPassword123" };
            var user = new User { Id = 1, Email = "test@google.com", UserName = "TestUser", PhoneNumber = "1234567890", DateCreated = DateTime.UtcNow };
            var roles = new List<string> { "Admin", "User" };
            var token = "generated_token";

            _userManagerMock
                .Setup(u => u.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _roleManagerMock
                .Setup(r => r.GetRolesAsync(user.Id))
                .ReturnsAsync(IdentityResult<IEnumerable<string>>.Success(roles));

            _tokenServiceMock
                .Setup(t => t.GenerateToken(user.Id.ToString(), user, roles))
                .Returns(IdentityResult<string>.Success(token));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data.Token);
            Assert.Equal(token, result.Data.Token);
            Assert.Equal(user.Id, result.Data.User.Id);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
        {
            // Arrange
            var command = new SignInCommand { Email = "test@google.com", Password = "InvalidPassword" };
             
            _userManagerMock
                .Setup(u => u.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Failure("Invalid credentials"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid credentials", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var command = new SignInCommand { Email = "notfound@google.com", Password = "SomePassword" };
             
            _userManagerMock
                .Setup(u => u.ValidateUserAsync(command.Email, command.Password))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid credentials", result.Error);   
        }
          
    }
}
