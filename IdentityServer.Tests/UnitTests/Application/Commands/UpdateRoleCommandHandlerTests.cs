namespace IdentityServer.Tests.UnitTests.Application.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;
    using Xunit;
    using FluentAssertions;

    using IdentityServer.Application.Commands.UpdateRole;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class UpdateRoleCommandHandlerTests
    {
        private readonly Mock<IRoleManager> _roleManagerMock;
        private readonly UpdateRoleCommandHandler _handler;

        public UpdateRoleCommandHandlerTests()
        {
            _roleManagerMock = new Mock<IRoleManager>();
            _handler = new UpdateRoleCommandHandler(_roleManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleDoesNotExist()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Administrator role" };

            _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Failure("Role not found."));


            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Failed to update role.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleUpdateFails()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Administrator role" };
            var role = new Role { Id = 1, Name = "User", Description = "Regular user" };

            _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.Id))
            .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock.Setup(x => x.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Failure("Update failed."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Failed to update role.");
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenRoleUpdateSucceeds()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Administrator role" };
            var role = new Role { Id = 1, Name = "User", Description = "Regular user" };

            _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock.Setup(x => x.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUpdateThrowsException()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Administrator role" };
            var role = new Role { Id = 1, Name = "User", Description = "Regular user" };

            _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));  // Return successful result with role

            _roleManagerMock.Setup(x => x.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ThrowsAsync(new Exception("Database error"));  // Simulate exception

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Failed to update role.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCommandIsInvalid()
        {
            // Arrange
            var command = new UpdateRoleCommand
            {
                Id = 0,
                Name = new string('X', 51), // Too long
                Description = new string('Y', 1) // Too short
            };
            var validator = new UpdateRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "ID is required.");
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "ID must be a positive number.");
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Name must not exceed 50 characters.");
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Description must be at least 3 characters long.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleIdIsNegative()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = -1, Name = "Admin", Description = "Administrator role" };
            var validator = new UpdateRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "ID must be a positive number.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleNameExceedsMaxLength()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = new string('A', 51), Description = "Administrator role" };
            var validator = new UpdateRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Name must not exceed 50 characters.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoleDescriptionExceedsMaxLength()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = new string('A', 201) };
            var validator = new UpdateRoleCommandValidator();

            // Act
            var validationResult = await validator.ValidateAsync(command);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Description must not exceed 200 characters.");
        }
























        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Admin role" };
            var role = new Role { Id = 1, Name = "Old Name", Description = "Old Description" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock
                .Setup(r => r.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Failure("Failed to update role."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to update role.", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCancelled()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Admin role" };
            var role = new Role { Id = 1, Name = "Old Name", Description = "Old Description" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock
                .Setup(r => r.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Cancel the token immediately

            // Act
            var result = await _handler.Handle(command, cancellationTokenSource.Token);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to update role.", result.Error); // Ensure the error message remains consistent even with cancellation
        }



        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenOnlyDescriptionIsUpdated()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = null, Description = "Updated role description" };
            var role = new Role { Id = 1, Name = "Old Name", Description = "Old Description" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock
                .Setup(r => r.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(true, result.Data); // Ensure the result is successful
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenRoleIsUnchanged()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Old Name", Description = "Old Description" }; // Same data as existing role
            var role = new Role { Id = 1, Name = "Old Name", Description = "Old Description" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock
                .Setup(r => r.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(true, result.Data); // Ensure the result is successful even if no change is made
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Admin role" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to update role.", result.Error); // Ensure the generic failure message is returned
        }

        [Fact]
        public async Task Handle_ShouldHandleMultipleCallsForSameRole()
        {
            // Arrange
            var command = new UpdateRoleCommand { Id = 1, Name = "Admin", Description = "Admin role" };
            var role = new Role { Id = 1, Name = "Old Name", Description = "Old Description" };

            _roleManagerMock
                .Setup(r => r.GetRoleByIdAsync(command.Id))
                .ReturnsAsync(IdentityResult<Role>.Success(role));

            _roleManagerMock
                .Setup(r => r.UpdateRoleAsync(command.Id, command.Name, command.Description))
                .ReturnsAsync(IdentityResult<bool>.Success(true));

            // Act
            var result1 = await _handler.Handle(command, CancellationToken.None);
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result1.IsSuccess);
            Assert.True(result2.IsSuccess); // Ensure both calls succeed
        }
    }
}