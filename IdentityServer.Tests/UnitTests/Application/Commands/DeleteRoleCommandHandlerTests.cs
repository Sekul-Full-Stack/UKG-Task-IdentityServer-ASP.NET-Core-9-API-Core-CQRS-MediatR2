namespace IdentityServer.Tests.UnitTests.Application.Commands
{ 
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        using Xunit;
        using Moq;
        using FluentAssertions;

        using IdentityServer.Application.Commands.DeleteRole;
        using IdentityServer.Application.Interfaces;
        using IdentityServer.Application.Results;
        using IdentityServer.Domain.Models; 

        public class DeleteRoleCommandHandlerTests
        {
            private readonly Mock<IRoleManager> _roleManagerMock;
            private readonly DeleteRoleCommandHandler _handler;

            public DeleteRoleCommandHandlerTests()
            {
                _roleManagerMock = new Mock<IRoleManager>();
                _handler = new DeleteRoleCommandHandler(_roleManagerMock.Object);
            }
         

            [Fact]
            public async Task Handle_ShouldDeleteMultipleRoles_WhenCalledInSequence()
            {
                // Arrange
                var command1 = new DeleteRoleCommand { RoleId = 1 };
                var command2 = new DeleteRoleCommand { RoleId = 2 };

                var role1 = new Role { Id = 1, Name = "Admin" };
                var role2 = new Role { Id = 2, Name = "User" };

                _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command1.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(role1));
                _roleManagerMock.Setup(x => x.DeleteRoleAsync(role1.Id))
                    .ReturnsAsync(IdentityResult<bool>.Success(true));

                _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command2.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(role2));
                _roleManagerMock.Setup(x => x.DeleteRoleAsync(role2.Id))
                    .ReturnsAsync(IdentityResult<bool>.Success(true));

                // Act
                var result1 = await _handler.Handle(command1, CancellationToken.None);
                var result2 = await _handler.Handle(command2, CancellationToken.None);

                // Assert
                result1.IsSuccess.Should().BeTrue();
                result1.Data.Should().BeTrue();

                result2.IsSuccess.Should().BeTrue();
                result2.Data.Should().BeTrue();
            }

            [Fact]
            public async Task Handle_ShouldThrowNullReferenceException_WhenRoleCommandIsNull()
            {
                // Arrange
                DeleteRoleCommand command = null;

                // Act
                Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

                // Assert
                await act.Should().ThrowAsync<NullReferenceException>();
            }



            [Fact]
            public async Task Handle_ShouldReturnFailure_WhenDeleteRoleThrowsAnotherException()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 1 };
                var role = new Role { Id = 1, Name = "Admin" };

                _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(role));

                _roleManagerMock.Setup(x => x.DeleteRoleAsync(role.Id))
                    .ThrowsAsync(new InvalidOperationException("Invalid operation"));

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Be("An unexpected error occurred while deleting the role.");
            }

            [Fact]
            public async Task Handle_ShouldReturnFailure_WhenRoleManagerReturnsInvalidResult()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 999 };
                var errorMessage = "Role not found.";

                _roleManagerMock.Setup(x => x.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Failure(errorMessage));

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Be(errorMessage);
            }

            [Fact]
            public async Task Handle_ShouldReturnFailure_WhenRoleIdIsZeroAfterValidation()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 0 };

                // Act
                var validator = new DeleteRoleCommandValidator();
                var validationResult = await validator.ValidateAsync(command);

                // Assert
                validationResult.IsValid.Should().BeFalse();
                validationResult.Errors.Should().HaveCount(2);

                validationResult.Errors[0].ErrorMessage.Should().Be("Role Id is required.");
                validationResult.Errors[1].ErrorMessage.Should().Be("Role Id must be a positive number.");
            }
         

            [Fact]
            public async Task Handle_RoleExists_DeletesRoleSuccessfully()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 1 };
             
                _roleManagerMock.Setup(m => m.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(new Role { Id = 1 }));
             
                _roleManagerMock.Setup(m => m.DeleteRoleAsync(1))
                    .ReturnsAsync(IdentityResult<bool>.Success(true));

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
                result.Data.Should().BeTrue();
            }

            [Fact]
            public async Task Handle_RoleNotFound_ReturnsFailure()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 1 };
             
                _roleManagerMock.Setup(m => m.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Failure("Role not found."));

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Be("Role not found.");
            }

            [Fact]
            public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 1 };
             
                var cts = new CancellationTokenSource();
                cts.Cancel();

                // mock the GetRoleByIdAsync method to simulate successful role retrieval
                _roleManagerMock.Setup(m => m.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(new Role { Id = 1 }));

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(command, cts.Token));
            }

            [Fact]
            public async Task Handle_DeleteRoleThrowsException_ReturnsFailure()
            {
                // Arrange
                var command = new DeleteRoleCommand { RoleId = 1 };
             
                _roleManagerMock.Setup(m => m.GetRoleByIdAsync(command.RoleId))
                    .ReturnsAsync(IdentityResult<Role>.Success(new Role { Id = 1 }));

                // moc the DeleteRoleAsync method to simulate an exception during deletion
                _roleManagerMock.Setup(m => m.DeleteRoleAsync(1))
                    .ThrowsAsync(new InvalidOperationException("Unexpected error during deletion"));

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Be("An unexpected error occurred while deleting the role.");

            }
        }
    } 
