using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityServer.Application.Commands.CreateRole;
using IdentityServer.Application.Commands.CreateUser;
using IdentityServer.Application.Commands.DeleteRole;
using IdentityServer.Application.Commands.DeleteUser;
using IdentityServer.Application.Commands.ResetUserPassword;
using IdentityServer.Application.Commands.SignIn;
using IdentityServer.Application.Commands.UpdateUser;
using IdentityServer.Application.Interfaces;
using IdentityServer.Application.Queries.GetUserInfo;
using IdentityServer.Infrastructure.Identity;
using MediatR;
 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateUserCommand>();
});  

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();  
builder.Services.AddFluentValidationClientsideAdapters();
 
builder.Services.AddValidatorsFromAssemblyContaining<CreateRoleCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteRoleCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetUserPasswordCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SignInCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateRoleCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetUserInfoValidator>();  

builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IRoleManager, RoleManager>(); 
  
builder.Services.AddControllers();
var app = builder.Build();
 
app.UseRouting();
app.UseAuthorization();
app.MapControllers(); 
 
app.Run();

public partial class Program { }

