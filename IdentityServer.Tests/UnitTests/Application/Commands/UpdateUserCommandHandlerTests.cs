namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using System.Threading.Tasks;
    using IdentityServer.Domain.Models;

    using FluentAssertions;
    using Moq;
    using Xunit;

    using IdentityServer.Application.Commands.UpdateUser;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;

    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _handler = new UpdateUserCommandHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1 };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Failure("User not found"));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found.");
        }


        [Fact]
        public async Task Handle_ShouldUpdateUser_WhenValidCommand()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1, Email = "newemail@example.com", PhoneNumber = "1234567890" };
            var existingUser = new User { Id = 1, UserName = "testuser", Email = "oldemail@example.com", PhoneNumber = "0987654321" };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Success(existingUser));
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                            .ReturnsAsync(IdentityResult<User>.Success(existingUser));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Email.Should().Be("newemail@example.com");
            result.Data.PhoneNumber.Should().Be("1234567890");
        }

        [Fact]
        public async Task Handle_ShouldNotUpdateFields_WhenCommandHasNoNewData()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1 };
            var existingUser = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "sameemail@example.com",
                PhoneNumber = "5555555555"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(existingUser));

            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => IdentityResult<User>.Success(user));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Email.Should().Be("sameemail@example.com");
            result.Data.PhoneNumber.Should().Be("5555555555");
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1, Email = "updated@example.com" };
            var user = new User { Id = 1, Email = "old@example.com", UserName = "testuser" };

            _userManagerMock.Setup(um => um.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult<User>.Failure("Update failed"));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Update failed");
        }


        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancelled()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1 };
            var cancellationToken = new CancellationToken(true); // already cancelled

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationToken));
        }


        [Fact]
        public async Task Handle_ShouldOnlyUpdateProvidedFields()
        {
            // Arrange
            var command = new UpdateUserCommand { Id = 1, Email = "updatedemail@example.com" }; // PhoneNumber not provided
            var user = new User
            {
                Id = 1,
                Email = "oldemail@example.com",
                PhoneNumber = "unchanged",
                UserName = "testuser"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(command.Id))
                            .ReturnsAsync(IdentityResult<User>.Success(user));
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                            .ReturnsAsync((User u) => IdentityResult<User>.Success(u));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Email.Should().Be("updatedemail@example.com");
            result.Data.PhoneNumber.Should().Be("unchanged");
        }  
     






























     [Fact]
        public async Task Handle_UserFound_UpdatesUserSuccessfully()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "elena@google.com",
                PhoneNumber = "1234567890"
            };

            var user = new User
            {
                Id = 1,
                UserName = "Elenabiggirl",
                Email = "eli@google.com",
                PhoneNumber = "0987654321",
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => IdentityResult<User>.Success(u));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Email.Should().Be(command.Email);
            result.Data.PhoneNumber.Should().Be(command.PhoneNumber);
            result.Data.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 42,
                Email = "alabalatuka@google.com"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Error.Should().Be("User not found.");
        }

        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "grrrrrrrrr@gmail.com"
            };

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(command, cts.Token));
        }

        [Fact]
        public async Task Handle_UpdateThrowsException_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Email = "newoppa@gmail.com"
            };

            var user = new User
            {
                Id = 1,
                UserName = "user",
                Email = "oldoppa@gmail.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("DB update failed"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DB update failed");
        }
    }
}
