namespace IdentityServer.Infrastructure.Identity
{
    using System.Threading.Tasks;  

    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Exceptions; 
    using IdentityServer.Domain.Models; 

    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository; 
        private readonly IPasswordHasher _passwordHasher;

        public UserManager( IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository; 
            _passwordHasher = passwordHasher;
        }  

        public async Task<IdentityResult<User>> FindByIdAsync(int userId)  
        { 
            try
            {  
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null) return IdentityResult<User>.Failure("User is not found."); 
                return IdentityResult<User>.Success(user);
            }
            catch(RepositoryException ex) { 
                return IdentityResult<User>.Failure($"An error occurred while finding the user.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<User>.Failure($"An unexpected error occurred finding the user.");
            } 
        }

        public async Task<IdentityResult<IEnumerable<User>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetUsersAsync();
                if (users == null) return IdentityResult<IEnumerable<User>>.Failure("No users");
                return IdentityResult<IEnumerable<User>>.Success(users);
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<IEnumerable<User>>.Failure($"An error occurred while finding the users.");
            }
            catch (Exception ex)
            {
                return IdentityResult<IEnumerable<User>>.Failure($"An unexpected error occurred finding the users.");
            }
        }
         
        public async Task<IdentityResult<User>> CreateAsync(User user)   
        { 
            try
            { 
                var existingUser = await _userRepository.GetUserByEmailAsync(user.Email); 
                if (existingUser != null) return IdentityResult<User>.Failure("Email already exists.");
                  
                var hashedPassword = _passwordHasher.HashPassword(user.Password); 
                var createdUser = await _userRepository.CreateUserAsync(user, hashedPassword); 
                if (createdUser == null) return IdentityResult<User>.Failure("Failed to create user: User creation failed due to database issues.");
                 
                return IdentityResult<User>.Success(createdUser);       
            }
            catch (RepositoryException ex)
            { 
                return IdentityResult<User>.Failure($"An error occurred while creating the user.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<User>.Failure($"Error while creating the user.");
            }
        }  

        public async Task<IdentityResult<bool>> DeleteAsync(int userId)  
        {
            try
            {
                var deleted = await _userRepository.DeleteUserAsync(userId); 
                if (!deleted)
                    return IdentityResult<bool>.Failure("Failed to delete user.");
                return IdentityResult<bool>.Success(true);
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<bool>.Failure($"Failed to delete the user.");
            }
            catch (Exception ex)
            {
                return IdentityResult<bool>.Failure($"Unexpected error occurred while deleting the user.");
            }
        }      

        public async Task<IdentityResult<User>> UpdateAsync(User user)    
        {  
            try
            {  
                var success = await _userRepository.UpdateUserAsync(user); 
                if (success != null)
                    return IdentityResult<User>.Success(user); 
                else
                    return IdentityResult<User>.Failure("Failed to update user.");
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<User>.Failure($"Error occurred while updating the user.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<User>.Failure($"An unexpected error occurred.");
            }
        }   

        public async Task<IdentityResult<User>> ValidateUserAsync(string email, string password)
        {  
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null) return IdentityResult<User>.Failure("User not found");  

                var isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, password);
                if (!isPasswordValid) return IdentityResult<User>.Failure("Wrong credentials"); 
                
                return IdentityResult<User>.Success(user);
            }
            catch (RepositoryException ex)
            {      
                return IdentityResult<User>.Failure($"Wrong credentials.");
            }
            catch (Exception ex)
            { 
                return IdentityResult<User>.Failure($"An unexpected error occurred.");
            }
        }  

        public async Task<IdentityResult<bool>> ResetPasswordAsync(int id, string newPassword)   
        { 
            try
            {  
                var hashedPassword = _passwordHasher.HashPassword(newPassword);
                var success = await _userRepository.ResetPasswordAsync(id, hashedPassword); 
                if (success) return IdentityResult<bool>.Success(true);     
                else return IdentityResult<bool>.Failure("Failed to reset password.");   
            }
            catch (RepositoryException ex)
            { 
                return IdentityResult<bool>.Failure($"Error occurred while resetting the password.");
            }
            catch (Exception ex)
            {  
                return IdentityResult<bool>.Failure($"An unexpected error occurred.");
            }  
        }

        public async Task<IdentityResult<bool>> FindByEmailAsync(string email)
        {
            try
            { 
                var exist = await _userRepository.GetUserByEmailAsync(email);
                if (exist != null) return IdentityResult<bool>.Success(true);
                else return IdentityResult<bool>.Success(false); ;
            }
            catch (RepositoryException ex)
            {
                return IdentityResult<bool>.Failure($"Error occurred while email check.");
            }
            catch (Exception ex)
            {
                return IdentityResult<bool>.Failure($"An unexpected error occurred.");
            }
        }
    }
}
