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
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var command = new CreateUserCommand("testuser", "password", "testuser@gmail.com", "1234567890");
             
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(command, cts.Token));
        } 
         
    }
}
