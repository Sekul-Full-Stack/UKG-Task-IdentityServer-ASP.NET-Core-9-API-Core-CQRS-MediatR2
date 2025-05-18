using IdentityServer.Application.Interfaces;
using IdentityServer.Domain.Models; 

    namespace IdentityServer.Tests.IntegrationTests.Fakes
    {
        public class FakeUserRepository : IUserRepository
        {
            public Task<bool> IsEmailUniqueAsync(string email) => Task.FromResult(true);

            public Task CreateUserAsync(string username, string email, string password, string phone) =>
                Task.CompletedTask;

            public Task<User> CreateUserAsync(User user, string password) =>
                Task.FromResult(user);

            public Task<IEnumerable<User>> GetUsersAsync() =>
                Task.FromResult<IEnumerable<User>>(new List<User>());

            public Task<User> UpdateUserAsync(User user) =>
                Task.FromResult(user);

            public Task<bool> DeleteUserAsync(int userId) =>
                Task.FromResult(true);

            public Task<User> GetUserByIdAsync(int userId) =>
                Task.FromResult(new User { Id = userId });

            public Task<User> GetUserByEmailAsync(string email) =>
                Task.FromResult(new User { Email = email });

            public Task<bool> CheckPasswordAsync(int userId, string passwordHash) =>
                Task.FromResult(true);

            public Task<bool> ResetPasswordAsync(int userId, string newPassword) =>
                Task.FromResult(true);
        }
    }
