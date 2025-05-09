namespace IdentityServer.Application.Commands.SignIn
{
    using MediatR;  
    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;  
    public class SignInCommandHandler : IRequestHandler<SignInCommand, IdentityResult<SignInResponse>> 
    {
        private readonly IUserManager _userManager;
        private readonly IRoleManager _roleManager;
        private readonly ITokenService _tokenService;

        public SignInCommandHandler(
            IUserManager userManager, 
            IRoleManager roleManager, 
            ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<IdentityResult<SignInResponse>> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var user = await _userManager.ValidateUserAsync(request.Email, request.Password);
                if (!user.IsSuccess)
                    return IdentityResult<SignInResponse>.Failure("Invalid credentials");

                var roles = await _roleManager.GetRolesAsync(user.Data.Id);
                if (!roles.IsSuccess)
                    return IdentityResult<SignInResponse>.Failure(roles.Error);

                var roleList = roles?.Data ?? Enumerable.Empty<string>();

                try
                {
                    var token = _tokenService.GenerateToken(user.Data.Id.ToString(), user.Data, roleList);
                    if (!token.IsSuccess)
                        return IdentityResult<SignInResponse>.Failure(token.Error ?? "Token generation failed");

                    return IdentityResult<SignInResponse>.Success(
                        new SignInResponse(token.Data, new AuthenticatedUser(
                            user.Data.Id, user.Data.UserName, user.Data.Email,
                            user.Data.PhoneNumber, user.Data.DateCreated, roleList)));
                }
                catch (Exception ex)
                {
                    return IdentityResult<SignInResponse>.Failure("Token generation failed");
                }
            }
            catch (Exception ex)
            {
                return IdentityResult<SignInResponse>.Failure("Unexpected error");
            }
        }
    }
}
