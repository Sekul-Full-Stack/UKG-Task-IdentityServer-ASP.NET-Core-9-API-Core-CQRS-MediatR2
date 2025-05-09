namespace IdentityServer.Tests.IntegrationTests.Fakes
{
    using global::IdentityServer.Application.Interfaces;
    using global::IdentityServer.Application.Results;
    using global::IdentityServer.Domain.Models; 
    using System.Collections.Concurrent;

    public class FakeUserManager : IUserManager
    {
        private readonly ConcurrentDictionary<int, User> _users = new();
        private int _idCounter = 1;
        private readonly object _lock = new();  

        public Task<IdentityResult<User>> CreateAsync(User user)
        {
            lock (_lock)
            {
                bool emailExists = _users.Values.Any(u =>
                    string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase));

                if (emailExists) 
                    return Task.FromResult(IdentityResult<User>.Failure("Email already in use")); 

                user.Id = _idCounter++;
                user.DateCreated = DateTime.UtcNow;
                _users[user.Id] = user; 
                return Task.FromResult(IdentityResult<User>.Success(user));
            }
        }

        public Task<IdentityResult<bool>> DeleteAsync(int userId)
        {
            var result = _users.TryRemove(userId, out _);
            return Task.FromResult(IdentityResult<bool>.Success(result));
        }

        public Task<IdentityResult<IEnumerable<User>>> GetAllUsersAsync()
        {
            return Task.FromResult(IdentityResult<IEnumerable<User>>.Success(_users.Values));
        }

        public Task<IdentityResult<User>> FindByIdAsync(int userId)
        {
            if (_users.TryGetValue(userId, out var user))
                return Task.FromResult(IdentityResult<User>.Success(user));

            return Task.FromResult(IdentityResult<User>.Failure("User not found"));
        }

        public Task<IdentityResult<User>> UpdateAsync(User user)
        {
            if (!_users.ContainsKey(user.Id))
                return Task.FromResult(IdentityResult<User>.Failure("User not found"));

            _users[user.Id] = user;
            return Task.FromResult(IdentityResult<User>.Success(user));
        }

        public Task<IdentityResult<bool>> ResetPasswordAsync(int id, string newPassword)
        {
            if (_users.TryGetValue(id, out var user))
            {
                user.Password = newPassword;
                return Task.FromResult(IdentityResult<bool>.Success(true));
            }

            return Task.FromResult(IdentityResult<bool>.Failure("User not found"));
        }

        public Task<IdentityResult<User>> ValidateUserAsync(string username, string password)
        {
            var user = _users.Values.FirstOrDefault(u =>
                string.Equals(u.Email, username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user != null)
                return Task.FromResult(IdentityResult<User>.Success(user));

            return Task.FromResult(IdentityResult<User>.Failure("Invalid credentials"));
        }

        public Task<IdentityResult<bool>> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
                return Task.FromResult(IdentityResult<bool>.Failure("Email is null or empty")); 
             
            var exists = _users.Values.Any(u =>
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
             
            return Task.FromResult(IdentityResult<bool>.Success(exists));
        }

    }


}
