namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using Moq;
    using Xunit;

    using IdentityServer.Application.Commands.ResetUserPassword;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class ResetUserPasswordCommandHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly ResetUserPasswordCommandHandler _handler;

        public ResetUserPasswordCommandHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _handler = new ResetUserPasswordCommandHandler(_userManagerMock.Object);
        }


        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var userId = 123;
            var newPassword = "NewValidPassword123";
            var request = new ResetUserPasswordCommand { Id = userId, NewPassword = newPassword };
            var user = new User { Id = userId, UserName = "TestUser" };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(IdentityResult<User>.Success(user));
            _userManagerMock.Setup(u => u.ResetPasswordAsync(userId, newPassword)).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(true, result.Data);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = 123;
            var newPassword = "NewValidPassword123";
            var request = new ResetUserPasswordCommand { Id = userId, NewPassword = newPassword };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.Error);
        }


        [Fact]
        public async Task Handle_NullRequest_ReturnsFailure()
        {
            // Arrange
            ResetUserPasswordCommand request = null;

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Request cannot be null.", result.Error);
        }

        [Fact]
        public async Task Handle_ExceptionDuringReset_ReturnsFailure()
        {
            // Arrange
            var userId = 123;
            var newPassword = "NewValidPassword123";
            var request = new ResetUserPasswordCommand { Id = userId, NewPassword = newPassword };
            var user = new User { Id = userId, UserName = "TestUser" };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(IdentityResult<User>.Success(user));
            _userManagerMock.Setup(u => u.ResetPasswordAsync(userId, newPassword))
                .ThrowsAsync(new Exception("Some unexpected error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Unexpected error occurred while resetting the password.", result.Error);
        }




























        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "NewPassword123!" };

            _userManagerMock
                .Setup(u => u.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenResetPasswordFails()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "NewPassword123!" };
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = "oldHash" };

            _userManagerMock
                .Setup(u => u.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(user.Id, command.NewPassword))
                .ReturnsAsync(IdentityResult<bool>.Failure("Failed to reset password."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to reset password.", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenPasswordIsResetSuccessfully()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "NewPassword123!" };
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = "oldHash" };

            _userManagerMock
                .Setup(u => u.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(user.Id, command.NewPassword))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);  // No error expected
        }

        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "NewPassword123!" };
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = "oldHash" };

            _userManagerMock
                .Setup(u => u.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(user.Id, command.NewPassword))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();  // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }


        [Fact]
        public void Validator_ShouldReturnFailure_WhenPasswordIsTooShort()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "short" };
            var validator = new ResetUserPasswordCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be at least 8 characters long.");
        }


        [Fact]
        public void Validator_ShouldReturnFailure_WhenPasswordIsNull()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = null };  // Null password
            var validator = new ResetUserPasswordCommandValidator();

            // Act
            var validationResult = validator.Validate(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Validator_ShouldReturnFailure_WhenPasswordContainsNoUppercase()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "password123!" };  // No uppercase
            var validator = new ResetUserPasswordCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must contain at least one uppercase letter.");
        }


        [Fact]
        public void Validator_ShouldReturnFailure_WhenPasswordContainsNoSpecialCharacter()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "Password123" };  // No special character
            var validator = new ResetUserPasswordCommandValidator();

            // Act
            var validationResult = validator.Validate(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Password must contain at least one special character.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAnUnhandledExceptionOccurs()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "NewPassword123!" };
            var user = new User { Id = 1, Email = "test@gmail.com", PasswordHash = "oldHash" };

            _userManagerMock
                .Setup(u => u.FindByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<User>.Success(user));

            // Simulate an unexpected error during password reset
            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(user.Id, command.NewPassword))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert the error message starts with the expected message
            Assert.False(result.IsSuccess);
            Assert.StartsWith("Unexpected error occurred while resetting the password. Details:", result.Error);
            Assert.Contains("Unexpected error", result.Error);  // Check that the original exception message is included
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenPasswordIsEmpty()
        {
            // Arrange
            var command = new ResetUserPasswordCommand { Id = 1, NewPassword = "" };  // Empty password

            var validator = new ResetUserPasswordCommandValidator();
            var validationResult = validator.Validate(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCommandIsNull()
        {
            // Act
            var result = await _handler.Handle(null, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Request cannot be null.", result.Error);  // Ensure the exact error string
        }
    }
} 
