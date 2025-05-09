namespace IdentityServer.Application.Commands.CreateUser
{
    using System.Threading;

    using MediatR;

    using IdentityServer.Domain.Models;
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results; 

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, IdentityResult<CreateUserResponse>>
    {
        private readonly IUserManager _userManager; 
        private readonly IRoleManager _roleManager;
        public CreateUserCommandHandler(IUserManager userManager, IRoleManager roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

        }
        public async Task<IdentityResult<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = new User
            {
                UserName = request.UserName,
                Password = request.Password,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            }; 
            
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                //if (existingEmail.Data) return IdentityResult<CreateUserResponse>.Failure("Email already in use"); 

                var createdUser = await _userManager.CreateAsync(user);
                if (createdUser.IsSuccess) await _roleManager.AddToRoleAsync(createdUser.Data.Id, 3);
                return createdUser.Map(user => new CreateUserResponse(user.Id, user.UserName, user.Email, user.PhoneNumber, user.DateCreated));
             
            }
            catch (Exception ex)
            {
                return IdentityResult<CreateUserResponse>.Failure($"An unexpected error occurred while creating the user.");
            }
        }
    } 
}
