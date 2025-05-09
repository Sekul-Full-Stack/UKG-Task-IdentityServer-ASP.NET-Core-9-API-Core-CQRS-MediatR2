namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Moq;
    using FluentAssertions;

    using IdentityServer.Application.Commands.CreateUser;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Domain.Models;
    using IdentityServer.Application.Results;

    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IRoleManager> _roleManagerMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _roleManagerMock = new Mock<IRoleManager>();

            _handler = new CreateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateUserAndAssignRole_WhenDataIsValid()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            var user = new User
            {
                Id = 1,
                UserName = command.UserName,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                DateCreated = DateTime.UtcNow
            };

            var identityResult = IdentityResult<User>.Success(user);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ReturnsAsync(identityResult);

            _roleManagerMock.Setup(x => x.AddToRoleAsync(user.Id, 3))
                .ReturnsAsync(IdentityResult<bool>.Success(true));


            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.UserName.Should().Be(command.UserName);

            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _roleManagerMock.Verify(x => x.AddToRoleAsync(user.Id, 3), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserManagerThrowsException()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("An unexpected error occurred while creating the user.");
        }

        [Fact]
        public async Task Handle_ShouldNotAssignRole_IfUserCreationFails()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            var failedResult = IdentityResult<User>.Failure("User creation failed");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ReturnsAsync(failedResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User creation failed");

            _roleManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleAssignmentFails()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            var user = new User
            {
                Id = 1,
                UserName = command.UserName,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                DateCreated = DateTime.UtcNow
            };

            var userResult = IdentityResult<User>.Success(user);
            var roleResult = IdentityResult<bool>.Failure("Role assignment failed");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ReturnsAsync(userResult);

            _roleManagerMock.Setup(x => x.AddToRoleAsync(user.Id, 3))
                            .ReturnsAsync(roleResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _roleManagerMock.Verify(x => x.AddToRoleAsync(user.Id, 3), Times.Once);
        }


        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () => await _handler.Handle(command, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserCreationReturnsNullData()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            var identityResult = IdentityResult<User>.Success(null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ReturnsAsync(identityResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("An unexpected error occurred while creating the user.");

        }



        [Fact]
        public async Task Handle_ShouldReturnSpecificFailureMessage_WhenUserCreationFails()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "test@gmail.com", "P@ssw0rd!", "1234567890");

            var failedResult = IdentityResult<User>.Failure("Email already exists");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                            .ReturnsAsync(failedResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Email already exists");
        }

         
        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "testuser@gmail.com", "1234567890");
             
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(command, cts.Token));
        }


        [Fact]
        public async Task Handle_UserCreationSuccessful_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "testuser@gmail.com", "1234567890");
             
            var createdUser = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "testuser@gmail.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.Now
            };

            var createUserResponse = new CreateUserResponse(
                createdUser.Id, createdUser.UserName, createdUser.Email, createdUser.PhoneNumber, createdUser.DateCreated
            );
             
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult<User>.Success(createdUser));
             
            _roleManagerMock.Setup(r => r.AddToRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);   
            Assert.NotNull(result.Data);    
            Assert.Equal(createUserResponse.UserName, result.Data.UserName);   
            Assert.Equal(createUserResponse.Email, result.Data.Email);        
            Assert.Equal(createUserResponse.PhoneNumber, result.Data.PhoneNumber);  
        }

        [Fact]
        public async Task Handle_UserCreationFailure_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "testuser@gmail.com", "1234567890");

            // Mock the CreateAsync method to return a failure result
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult<User>.Failure("Email already exists."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);   
            Assert.Null(result.Data);   
            Assert.Equal("Email already exists.", result.Error);   
        }

        [Fact]
        public async Task Handle_InvalidEmail_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "invalid-email", "1234567890");
             
            var createdUser = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "invalid-email",  
                PhoneNumber = "1234567890",
                DateCreated = DateTime.Now
            };
             
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult<User>.Failure("Invalid email format."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess); 
            Assert.Null(result.Data);  
            Assert.Equal("Invalid email format.", result.Error); 
        }

        [Fact]
        public async Task Handle_UserAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "testuser@gmail.com", "1234567890");
             
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult<User>.Failure("Email already exists."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);   
            Assert.Null(result.Data);        
            Assert.Equal("Email already exists.", result.Error);  
        }

        [Fact]
        public async Task Handle_CreateUserThrowsException_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand("Gosho", "ossspa@google.com", "pE4!%hbnJassword", "1234567890");

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("Custom unexpected error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal("An unexpected error occurred while creating the user.", result.Error);
        }
    }
}
