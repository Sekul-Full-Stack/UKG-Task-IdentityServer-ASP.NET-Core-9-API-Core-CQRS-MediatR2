using System.Data;
using System.Reflection;

using Microsoft.Data.SqlClient;

using MediatR;
using FluentValidation;

using IdentityServer.Application.Interfaces;
using IdentityServer.Infrastructure.Identity;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Application.Commands.CreateUser;
using IdentityServer.Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IRoleRepository, RoleRepository>(); 

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleManager, RoleManager>();

builder.Services.AddControllers();

var app = builder.Build(); 

app.MapControllers();

app.Run();
 