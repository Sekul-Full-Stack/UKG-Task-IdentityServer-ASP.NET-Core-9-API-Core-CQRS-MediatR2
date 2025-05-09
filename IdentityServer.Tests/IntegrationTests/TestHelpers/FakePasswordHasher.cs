using IdentityServer.Application.Interfaces;

namespace IdentityServer.Tests.IntegrationTests.TestHelpers
{
    namespace IdentityServer.Tests.TestHelpers
    {
        public class FakePasswordHasher : IPasswordHasher
        {
            public string Hash(string password) => "hashed-" + password;

            public string HashPassword(string password)
            { 
                return "hashed-" + password;
            }

            public bool Verify(string password, string hashedPassword) => true;

            public bool VerifyPassword(string hashedPassword, string providedPassword)
            { 
                return hashedPassword == "hashed-" + providedPassword;
            }
        }
    }

}
