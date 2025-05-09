namespace IdentityServer.Tests.IntegrationTests
{
    using global::IdentityServer.Application.Interfaces;
    using global::IdentityServer.Infrastructure.Identity;
    using global::IdentityServer.Tests.IntegrationTests.Fakes;
    using global::IdentityServer.Tests.IntegrationTests.Fakes.IdentityServer.Tests.IntegrationTests.Fakes;
    using global::IdentityServer.Tests.IntegrationTests.TestHelpers.IdentityServer.Tests.TestHelpers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    namespace IdentityServer.Tests.IntegrationTests
    {
        public class InMemoryWebApplicationFactory : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureServices(services =>
                { 
                    RemoveExistingServices(services); 
                    RegisterFakeServices(services); 
                });
            }

            private void RemoveExistingServices(IServiceCollection services)
            { 
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IRoleRepository>();
                services.RemoveAll<IUserManager>();
                services.RemoveAll<IRoleManager>();
                services.RemoveAll<ITokenService>(); 
            }

            private void RegisterFakeServices(IServiceCollection services)
            { 
                services.AddScoped<IUserRepository, FakeUserRepository>();
                services.AddScoped<IRoleRepository, FakeRoleRepository>();
                services.AddScoped<IUserManager, FakeUserManager>();
                services.AddScoped<IRoleManager, FakeRoleManager>();
                 
                services.AddScoped<ITokenService, FakeTokenService>();
                 
                services.AddScoped<IPasswordHasher, FakePasswordHasher>();
            }
             
            private void LogRegisteredServices(IServiceCollection services)
            {
                foreach (var service in services)
                {
                    Console.WriteLine($"{service.ServiceType.FullName} => {service.ImplementationType?.FullName}");
                }
            }

            public FakeUserManager GetFakeUserManager(IServiceProvider serviceProvider) =>
                serviceProvider.GetRequiredService<FakeUserManager>(); 
        }



    }

}
