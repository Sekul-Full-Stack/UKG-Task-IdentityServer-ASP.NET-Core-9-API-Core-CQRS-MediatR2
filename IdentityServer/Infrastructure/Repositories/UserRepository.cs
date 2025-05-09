namespace IdentityServer.Infrastructure.Repositories
{
    using System.Data; 
    using System.Threading.Tasks; 

    using Microsoft.Data.SqlClient;

    using Dapper;

    using IdentityServer.Application.Interfaces; 
    using IdentityServer.Domain.Models;
    using IdentityServer.Domain.Exceptions; 

    public class UserRepository: IUserRepository
    {
        private readonly IDbConnection _dbConnection; 
        public UserRepository(IConfiguration configuration) => 
            _dbConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
         
        public async Task<User> CreateUserAsync(User user, string hashedPassword)  
        {
            var query = @"INSERT INTO Users (Email, UserName, PhoneNumber, PasswordHash) 
                        OUTPUT INSERTED.Id, INSERTED.Email, INSERTED.UserName, INSERTED.PhoneNumber, INSERTED.DateCreated
                        VALUES(@Email, @UserName, @PhoneNumber, @PasswordHash); ";
             
            var parameters = new User
            {
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = hashedPassword
            };
            try
            { 
                var recorder = await _dbConnection.QuerySingleAsync<User>(query, parameters);
                if (recorder == null)
                    throw new RepositoryException("User creation failed.");

                return new User
                {
                    Id = recorder.Id,
                    Email = recorder.Email,
                    UserName = recorder.UserName,
                    PhoneNumber = recorder.PhoneNumber,
                    DateCreated = recorder.DateCreated
                };
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error creating user from the database.");
            } 
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            const string query = @"
                UPDATE Users 
                SET PhoneNumber = @PhoneNumber, Email = @Email 
                WHERE Id = @Id;

                SELECT Id, UserName, PhoneNumber, Email, DateCreated 
                FROM Users 
                WHERE Id = @Id;";
            var parameters = new
            {  
                user.PhoneNumber,
                user.Email,
                user.Id
            };
            try
            { 
                return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, parameters);
            }
            catch (Exception ex)
            { 
                throw new RepositoryException("Error occurred while updating user.");
            } 
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var sql = @"
                SELECT u.Id, u.UserName, u.Email, u.PhoneNumber, u.DateCreated, r.Name AS RoleName
                FROM Users u
                LEFT JOIN UserRoles ur ON u.Id = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.Id";

            try
            {
                var userDictionary = new Dictionary<int, User>();

                var result = await _dbConnection.QueryAsync<User, string, User>(
                    sql,
                    (user, roleName) =>
                    {
                        if (!userDictionary.TryGetValue(user.Id, out var currentUser))
                        {
                            currentUser = user;
                            currentUser.Roles = new List<string>();
                            userDictionary.Add(currentUser.Id, currentUser);
                        }

                        if (!string.IsNullOrEmpty(roleName))
                            currentUser.Roles.Add(roleName);

                        return currentUser;
                    },
                    splitOn: "RoleName"
                );

                return userDictionary.Values;
            }
            catch (Exception)
            {
                throw new RepositoryException("Error occurred while fetching users with roles.");
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var query = "DELETE FROM Users WHERE Id = @userId"; 
            var parameters = new { UserId = userId }; 
            try
            {
                return await _dbConnection.ExecuteAsync(query, parameters) > 0; 
            }
            catch (Exception ex)
            { 
                throw new RepositoryException("An error occurred while deleting the user.");
            }
        }  

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = "SELECT * FROM Users WHERE Email = @Email";
            var parameters = new { Email = email };
            try
            { 
                return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, parameters); 
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error fetching user data from the database."); 
            }
        }  
          
        public async Task<User?> GetUserByIdAsync(int userId)   
        { 
            var query = "SELECT * FROM Users WHERE Id = @UserId";
            var parameters = new { UserId = userId };
            try
            { 
                 return  await _dbConnection.QuerySingleOrDefaultAsync<User>(query, parameters); 
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error fetching user data from the database."); 
            } 
        }
         
        public async Task<bool> CheckPasswordAsync(int userId, string passwordHash)
        {  
            var query = "SELECT PasswordHash FROM Users WHERE Id = @Id";
            var parameters = new { Id = userId};
            try
            {
                var storedPasswordHash = await _dbConnection.QuerySingleOrDefaultAsync<string>(query, new { Id = userId });
                return storedPasswordHash == passwordHash;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("An error occurred while adding the claim.");
            }
        }    
         
        public async Task<string> GetRoleForUserAsync(int userId)
        {  
            const string query = "SELECT Role FROM Users WHERE Id = @UserId";
            var parameters = new { UserId = userId };
            try
            {
                return await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { UserId = userId }) ?? "";
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error occurred while removing user from role.");
            }
        } 
           
        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        { 
            try
            {  
                var updateQuery = "UPDATE Users SET PasswordHash = @NewPassword WHERE Id = @UserId"; 
                var parameters = new { UserId = userId, NewPassword = newPassword }; 
                return await _dbConnection.ExecuteAsync(updateQuery, parameters) > 0; 
            }
            catch (Exception ex)
            { 
                throw new RepositoryException("Error occurred while resetting the password");
            }
        }
    }
}
