using Microsoft.IdentityModel.Tokens;

using PeopleManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IRoleManagerClient, RoleManagerClient>(client =>
{
    client.BaseAddress = new Uri(" http://localhost:5246");
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = " http://localhost:5246";
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5246"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
 