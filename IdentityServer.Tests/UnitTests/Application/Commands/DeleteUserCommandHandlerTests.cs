namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Moq;
    using FluentAssertions;

    using IdentityServer.Application.Commands.DeleteUser;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class DeleteUserCommandHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly DeleteUserCommandHandler _handler;

        public DeleteUserCommandHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _handler = new DeleteUserCommandHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 1 };

            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock.Setup(x => x.DeleteAsync(user.Id))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 999 };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenDeleteThrowsException()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 1 };
            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock.Setup(x => x.DeleteAsync(user.Id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("An unexpected error occurred while deleting the user.");
        }


        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 1 };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () => await _handler.Handle(command, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }


        [Fact]
        public async Task Handle_ShouldFail_WhenUserIdIsZero()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 0 };

            // Act
            var validator = new DeleteUserCommandValidator();
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().HaveCount(2);

            // Verify each error message
            validationResult.Errors[0].ErrorMessage.Should().Be("User Id is required.");
            validationResult.Errors[1].ErrorMessage.Should().Be("User Id must be a positive number.");
        }


        [Fact]
        public async Task Handle_ShouldFail_WhenUserIdIsNegative()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = -1 };

            // Act
            var validator = new DeleteUserCommandValidator();
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("User Id must be a positive number.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserIdIsValidButNotFound()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 123 };
            _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found.");
        }


        [Fact]
        public async Task Handle_UserFoundAndDeleted_ReturnsSuccess()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 1 };
            _userManagerMock.Setup(m => m.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Success(new User { Id = command.UserId }));
            _userManagerMock.Setup(m => m.DeleteAsync(command.UserId))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 1 };
            _userManagerMock.Setup(m => m.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found.");
        }

        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var command = new DeleteUserCommand { UserId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cts.Token));
        }

        [Fact]
        public async Task Handle_InvalidUserId_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteUserCommand { UserId = 0 }; // invalid UserId (0)
            var validator = new DeleteUserCommandValidator();
            var validationResult = validator.Validate(command);

            // Act & Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(e => e.ErrorMessage == "User Id must be a positive number.");
        }
    }
}
