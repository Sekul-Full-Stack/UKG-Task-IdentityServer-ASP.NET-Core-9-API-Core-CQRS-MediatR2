namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using Moq;
    using Xunit;
    using FluentAssertions;

    using IdentityServer.Application.Commands.AssignRole;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models; 

    public class AssignRoleCommandHandlerTests
    {
        private readonly Mock<IRoleManager> _roleManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly AssignRoleCommandHandler _handler;

        public AssignRoleCommandHandlerTests()
        {
            _roleManagerMock = new Mock<IRoleManager>();
            _userManagerMock = new Mock<IUserManager>();
            _handler = new AssignRoleCommandHandler(_roleManagerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_Success_ReturnsSuccess()
        {
            // Arrange
            var userId = 1;
            var roleId = 100;
            var command = new AssignRoleCommand(userId, roleId);

            var user = new User { Id = userId };
            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(IdentityResult<User>.Success(user));
            _roleManagerMock.Setup(r => r.AddToRoleAsync(userId, roleId)).ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(true, result.Data);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var roleId = 100;
            var command = new AssignRoleCommand(userId, roleId);

            // Simulate user not found
            _userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(IdentityResult<User>.Failure("User not found"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task AssignRoleCommand_WithInvalidUserId_FailsValidation()
        {
            // Arrange
            var command = new AssignRoleCommand(0, 100);
            var validator = new AssignRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "User Id must be a positive number.");
        }


        [Fact]
        public async Task Handle_InvalidRoleId_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var roleId = 0;
            var command = new AssignRoleCommand(userId, roleId);

            var validator = new AssignRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Role Id must be a positive number.");
        }


        [Fact]
        public async Task Handle_RoleAssignmentError_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var roleId = 100;
            var command = new AssignRoleCommand(userId, roleId);

            var user = new User { Id = userId };
            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(IdentityResult<User>.Success(user));

            // Simulate role assignment failure
            _roleManagerMock.Setup(r => r.AddToRoleAsync(userId, roleId)).ReturnsAsync(IdentityResult<bool>.Failure("Role assignment failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Role assignment failed", result.Error);
        }


        [Fact]
        public async Task Handle_UserFoundAndRoleAssigned_ReturnsSuccess()
        {
            // Arrange
            var command = new AssignRoleCommand(UserId: 1, RoleId: 1);
            _userManagerMock.Setup(m => m.FindByIdAsync(command.UserId))
                .ReturnsAsync(IdentityResult<User>.Success(new User { Id = command.UserId }));
            _roleManagerMock.Setup(m => m.AddToRoleAsync(command.UserId, command.RoleId))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }



        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var command = new AssignRoleCommand(UserId: 1, RoleId: 1);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cts.Token));
        }

        [Fact]
        public async Task Handle_InvalidUserId_ReturnsFailure()
        {
            // Arrange
            var command = new AssignRoleCommand(UserId: 0, RoleId: 1); // Invalid UserId (0)
            var validator = new AssignRoleCommandValidator();
            var validationResult = validator.Validate(command);

            // Act & Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(e => e.ErrorMessage == "User Id must be a positive number.");
        }


    }

}
