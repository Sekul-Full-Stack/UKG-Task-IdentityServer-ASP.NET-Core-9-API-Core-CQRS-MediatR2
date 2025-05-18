using IdentityServer.Application.Interfaces;
using IdentityServer.Tests.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IdentityServer.Tests.IntegrationTests
{
    public class InMemoryWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");  // Ensure it's using the correct environment for testing

            builder.ConfigureServices(services =>
            {
                // Remove existing services and register the fake ones
                RemoveExistingServices(services);
                RegisterFakeServices(services);
                LogRegisteredServices(services);
            });
        }

        private void LogRegisteredServices(IServiceCollection services)
        {
            foreach (var service in services)
            {
                Console.WriteLine($"Service Registered: {service.ServiceType.FullName} => {service.ImplementationType?.FullName}");
            }
        }

        private void RemoveExistingServices(IServiceCollection services)
        {
            // Remove the existing implementations
            services.RemoveAll<IUserManager>();
            services.RemoveAll<IRoleManager>();
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IRoleRepository>();
            services.RemoveAll<ITokenService>();
        }

        private void RegisterFakeServices(IServiceCollection services)
        { 
            services.AddSingleton<FakeUserManager>();
            services.AddSingleton<IUserManager>(sp => sp.GetRequiredService<FakeUserManager>());

            services.AddSingleton<FakeRoleManager>();
            services.AddSingleton<IRoleManager>(sp => sp.GetRequiredService<FakeRoleManager>());

            services.AddSingleton<IUserRepository, FakeUserRepository>();
            services.AddSingleton<IRoleRepository, FakeRoleRepository>();

            services.AddSingleton<FakeTokenService>();
            services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<FakeTokenService>()); 
        }
         
        public FakeUserManager GetFakeUserManager(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<FakeUserManager>();
         
        public FakeRoleManager GetFakeRoleManager(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<FakeRoleManager>(); 
    }
}
