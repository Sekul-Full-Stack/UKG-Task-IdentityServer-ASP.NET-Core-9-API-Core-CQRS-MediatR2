namespace IdentityServer.Tests.UnitTests.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;
    using Moq;
    using Xunit;

    using IdentityServer.Application.Queries.GetAllUsers;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class GetAllUsersQueryHandlerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly GetAllUsersQueryHandler _handler;

        public GetAllUsersQueryHandlerTests()
        {
            _userManagerMock = new Mock<IUserManager>();
            _handler = new GetAllUsersQueryHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@example.com", PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() },
                new User { Id = 2, UserName = "user2", Email = "user2@example.com", PhoneNumber = "2222222222", DateCreated = DateTime.UtcNow, Roles = new[] { "User" }.ToList() }
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Select(u => u.UserName).Should().Contain(new[] { "user1", "user2" });
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(Enumerable.Empty<User>()));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldFilterOutNullUsers()
        {
            // Arrange
            var users = new List<User?>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@example.com", PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() },
                null
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users!));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().UserName.Should().Be("user1");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserManagerFails()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Failure("Database error"));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Database error");
        }

        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancelled()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(new GetAllUsersQuery(), cancellationToken));
        }

        [Fact]
        public async Task Handle_ShouldReturnAllUsers_WhenLargeNumberOfUsersExist()
        {
            // Arrange
            var users = Enumerable.Range(1, 1000).Select(i =>
                new User
                {
                    Id = i,
                    UserName = $"user{i}",
                    Email = $"user{i}@example.com",
                    PhoneNumber = $"123456789{i}",
                    DateCreated = DateTime.UtcNow.AddDays(-i),
                    Roles = new[] { "User" }.ToList()
                }).ToList();

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1000);
        }

        [Fact]
        public async Task Handle_ShouldHandleMalformedUserData()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@example.com", PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = null }, // Null roles
                new User { Id = 2, UserName = "user2", Email = "user2@example.com", PhoneNumber = "2222222222", DateCreated = DateTime.UtcNow, Roles = new[] { "User" }.ToList() } // Empty roles
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.First().Roles.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldHandleUsersWithMissingFields()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Email = null, PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() }, // Missing email
                new User { Id = 2, UserName = "user2", Email = "user2@example.com", PhoneNumber = null, DateCreated = DateTime.UtcNow, Roles = new[] { "User" }.ToList() } // Missing phone number
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.First().Email.Should().BeNull();
            result.Data.Last().PhoneNumber.Should().BeNull();
        }


        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenUserManagerReturnsNullUsers()
        {
            // Arrange
            var users = new List<User?> { null, null, null };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users!));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnValidUsers_WhenSomeUsersAreInvalid()
        {
            // Arrange
            var users = new List<User?>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@example.com", PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() },
                null, // invalid user
                new User { Id = 2, UserName = "user2", Email = "user2@example.com", PhoneNumber = "2222222222", DateCreated = DateTime.UtcNow, Roles = new[] { "User" }.ToList() }
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users!));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.First().UserName.Should().Be("user1");
            result.Data.Last().UserName.Should().Be("user2");
        }


        [Fact]
        public async Task Handle_ShouldRespectCancellationToken_WhenCancelledDuringExecution()
        {
            // Arrange
            var cancellationToken = new CancellationToken(true);
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@example.com", PhoneNumber = "1111111111", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() }
            };

            _userManagerMock.Setup(um => um.GetAllUsersAsync())
                            .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(new GetAllUsersQuery(), cancellationToken));
        }
         
        [Fact]
        public async Task Handle_ReturnsAllUsersSuccessfully()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    UserName = "user1",
                    Email = "user1@gmail.com",
                    PhoneNumber = "1234567890",
                    DateCreated = DateTime.UtcNow,
                    Roles = new[]  { "ADMIN" }.ToList()
                },
                new User
                {
                    Id = 2,
                    UserName = "user2",
                    Email = "user2@gmail.com",
                    PhoneNumber = "0987654321",
                    DateCreated = DateTime.UtcNow,
                    Roles = new[] { "EMPLOYEE" }.ToList()
                }
            };

            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.First().UserName.Should().Be("user1");
        }

        [Fact]
        public async Task Handle_UserRetrievalFails_ReturnsFailure()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Failure("Database unavailable"));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Error.Should().Be("Database unavailable");
        }

        [Fact]
        public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(new GetAllUsersQuery(), cts.Token));
        }


        [Fact]
        public async Task Handle_NoUsersFound_ReturnsEmptyList()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(new List<User>()));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_UsersListContainsNull_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User?> { null };
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users!));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();  
        }

        [Fact]
        public async Task Handle_NoUsers_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User>();   
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();   
            result.Data.Should().BeEmpty();       
        }

        [Fact]
        public async Task Handle_GetAllUsersAsyncFails_ReturnsFailure()
        {
            // Arrange
            var errorMessage = "An error occurred while retrieving users.";
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Failure(errorMessage));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();   
            result.Error.Should().Be(errorMessage);   
        }

        [Fact]
        public async Task Handle_MultipleUsers_ReturnsListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Email = "user1@gmail.com", PhoneNumber = "1234567890", DateCreated = DateTime.UtcNow, Roles = new[] { "Admin" }.ToList() },
                new User { Id = 2, UserName = "user2", Email = "user2@gmail.com", PhoneNumber = "0987654321", DateCreated = DateTime.UtcNow, Roles = new[] { "User" }.ToList() }
            };

            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();  
            result.Data.Should().HaveCount(2);   
            result.Data.First().UserName.Should().Be("user1");   
            result.Data.Last().UserName.Should().Be("user2");   
        }

        [Fact]
        public async Task Handle_LargeNumberOfUsers_ReturnsListOfUsers()
        {
            // Arrange
            var users = new List<User>();
            for (int i = 0; i < 1000; i++)
            {
                users.Add(new User
                {
                    Id = i,
                    UserName = $"user{i}",
                    Email = $"user{i}@gmail.com",
                    PhoneNumber = $"123456789{i}",
                    DateCreated = DateTime.UtcNow,
                    Roles = new[] { "User" }.ToList()
                });
            }

            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(users));

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();   
            result.Data.Should().HaveCount(1000);  
        }

        [Fact]
        public async Task Handle_GetAllUsersAsyncReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(IdentityResult<IEnumerable<User>>.Success(Enumerable.Empty<User>()));  

            // Act
            var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();   
            result.Data.Should().BeEmpty();     
        }
    }
}
