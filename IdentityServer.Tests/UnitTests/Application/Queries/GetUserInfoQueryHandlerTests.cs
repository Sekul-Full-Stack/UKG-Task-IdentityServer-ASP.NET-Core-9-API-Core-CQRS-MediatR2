namespace IdentityServer.Tests.UnitTests.Application.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;
    using Moq;
    using Xunit;

    using IdentityServer.Application.Queries.GetUserInfo;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class GetUserInfoQueryHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly GetUserInfoQueryHandler _handler;

        public GetUserInfoQueryHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _handler = new GetUserInfoQueryHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserInfo_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "testuser@example.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Success(user));

            var query = new GetUserInfoQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.UserName.Should().Be("testuser");
            result.Data.Email.Should().Be("testuser@example.com");
            result.Data.PhoneNumber.Should().Be("1234567890");
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Failure("User not found"));

            var query = new GetUserInfoQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserIdIsInvalid()
        {
            // Arrange
            var invalidQuery = new GetUserInfoQuery { UserId = -1 };

            // Act
            var validationResult = new GetUserInfoValidator().Validate(invalidQuery);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage == "User Id must be a positive number.");
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserIdIsZero()
        {
            // Arrange
            var invalidQuery = new GetUserInfoQuery { UserId = 0 };

            // Act
            var validationResult = new GetUserInfoValidator().Validate(invalidQuery);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage == "User Id must be a positive number.");
        }


        [Fact]
        public async Task Handle_ShouldRespectCancellationToken_WhenCancelled()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true); 
            var query = new GetUserInfoQuery { UserId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(query, cancellationToken));
        }


        [Fact]
        public async Task Handle_ShouldReturnValidUserInfo_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "testuser@example.com",
                PhoneNumber = "1234567890",
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Success(user));

            var query = new GetUserInfoQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.UserName.Should().Be("testuser");
            result.Data.Email.Should().Be("testuser@example.com");
            result.Data.PhoneNumber.Should().Be("1234567890");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserIsNull()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Success(null));   

            var query = new GetUserInfoQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert 
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("No data.");
        }


        [Fact]
        public async Task Handle_ShouldReturnUserInfo_WhenUserHasEmptyFields()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty,
                DateCreated = DateTime.UtcNow
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(IdentityResult<User>.Success(user));

            var query = new GetUserInfoQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.UserName.Should().BeEmpty();
            result.Data.Email.Should().BeEmpty();
            result.Data.PhoneNumber.Should().BeEmpty();
        }
    }
}
