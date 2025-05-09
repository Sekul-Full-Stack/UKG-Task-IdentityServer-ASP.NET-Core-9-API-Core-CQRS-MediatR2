namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using Moq;
    using Xunit;
    using FluentAssertions;

    using IdentityServer.Application.Commands.CreateRole;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results; 

    public class CreateRoleCommandHandlerTests
    {
        private readonly Mock<IRoleManager> _roleManagerMock;
        private readonly CreateRoleCommandHandler _handler;
        private readonly CreateRoleCommandValidator _validator;

        public CreateRoleCommandHandlerTests()
        {
            _roleManagerMock = new Mock<IRoleManager>();
            _handler = new CreateRoleCommandHandler(_roleManagerMock.Object);
            _validator = new CreateRoleCommandValidator();
        }

        [Fact]
        public async Task Handle_SuccessfulRoleCreation_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            _roleManagerMock.Setup(r => r.CreateRoleAsync(command.Name, command.Description))
                            .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task Handle_InvalidRoleName_ReturnsValidationFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("", "Valid description");

            // Act
            var validationResult = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Name is required");
        }

        [Fact]
        public async Task Handle_InvalidRoleDescription_ReturnsValidationFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "");

            // Act
            var validationResult = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Description is required");
        }


        [Fact]
        public async Task Handle_RoleCreationFails_ReturnsFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            // Simulate failure in role creation 
            _roleManagerMock.Setup(r => r.CreateRoleAsync(command.Name, command.Description))
                            .ReturnsAsync(IdentityResult<bool>.Failure("Failed to create role"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to create role", result.Error);
        }

        [Fact]
        public async Task Handle_EmptyCommand_ReturnsFailure()
        {
            // Arrange
            CreateRoleCommand command = null; // Null command

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Contains("Object reference not set to an instance of an object", exception.Message);
        }



        [Fact]
        public async Task Handle_RoleAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            // Mock that the role already exists
            _roleManagerMock.Setup(r => r.CreateRoleAsync(command.Name, command.Description))
                            .ReturnsAsync(IdentityResult<bool>.Failure("Role already exists"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Role already exists", result.Error);
        }

        [Fact]
        public async Task Handle_LargeInputData_ReturnsFailure()
        {
            // Arrange
            var command = new CreateRoleCommand(
                new string('A', 51),
                new string('B', 201)
            );

            // Act
            var validationResult = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Name must not exceed 50 characters.");
            Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Description must not exceed 200 characters.");
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full access.");
            _roleManagerMock.Setup(m => m.CreateRoleAsync(command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true)); // Simulate success

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FailureInRoleCreation_ReturnsFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full access.");
            _roleManagerMock.Setup(m => m.CreateRoleAsync(command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Failure("Failed to create role"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Failed to create role");
        }

        [Fact]
        public async Task Handle_InvalidRoleDescription_ReturnsFailure()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "");

            var validator = new CreateRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert  
            validationResult.IsValid.Should().BeFalse();

            validationResult.Errors
                .Count(e => e.PropertyName == "Description")
                .Should().Be(2);

            validationResult.Errors
                .Where(e => e.PropertyName == "Description")
                .Select(e => e.ErrorMessage)
                .Should().Contain("Description is required")
                .And.Contain("Description must be at least 3 characters long.");
        }


        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var command = new CreateRoleCommand("Admin", "Administrator role with full access.");

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cts.Token));
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenRoleIsCreated()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            _roleManagerMock
                .Setup(rm => rm.CreateRoleAsync(command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleCreationFails()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            _roleManagerMock
                .Setup(rm => rm.CreateRoleAsync(command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Failure("Role creation failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Role creation failed");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
        {
            // Arrange
            var command = new CreateRoleCommand("Admin", "Administrator role with full permissions");

            _roleManagerMock
                .Setup(rm => rm.CreateRoleAsync(command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _handler.Handle(command, new CancellationToken(true)));
        }
    }

}
