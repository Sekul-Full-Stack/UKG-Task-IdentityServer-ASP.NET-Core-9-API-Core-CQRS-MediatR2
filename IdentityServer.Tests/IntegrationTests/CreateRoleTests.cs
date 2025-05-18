namespace IdentityServer.Tests.IntegrationTests
{
    using System.Net;
    using Xunit;

    using IdentityServer.Application.Commands.CreateRole;
    using IdentityServer.Tests.IntegrationTests.Fakes;

    public class CreateRoleTests : IClassFixture<InMemoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InMemoryWebApplicationFactory _factory;

        public CreateRoleTests(InMemoryWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateRole_ShouldSucceed_WhenInputIsValid()
        {
            var command = new CreateRoleCommand("Manager", "Responsible for managing users.");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("true", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenNameIsEmpty()
        {
            var command = new CreateRoleCommand("", "Handles roles.");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Name is required", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenDescriptionIsEmpty()
        {
            var command = new CreateRoleCommand("Editor", "");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Description is required", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenNameTooShort()
        {
            var command = new CreateRoleCommand("A", "Manages something.");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Name must be at least 2 characters", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenNameTooLong()
        {
            var longName = new string('A', 51);
            var command = new CreateRoleCommand(longName, "Too long name test.");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Name must not exceed 50 characters", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenDescriptionTooShort()
        {
            var command = new CreateRoleCommand("Helper", "A"); // Too short
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Description must be at least 3 characters", content);
        }


        [Fact]
        public async Task CreateRole_ShouldFail_WhenDescriptionTooLong()
        {
            var longDescription = new string('B', 201);
            var command = new CreateRoleCommand("Supervisor", longDescription);
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Description must not exceed 200 characters", content);
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenNameAndDescriptionEmpty()
        {
            var command = new CreateRoleCommand("", "");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Name is required", content);
            Assert.Contains("Description is required", content);
        }

        [Fact]
        public async Task CreateRole_ShouldSucceed_WhenFieldsAreAtMaxLength()
        {
            var name = new string('X', 50);
            var description = new string('Y', 200);
            var command = new CreateRoleCommand(name, description);

            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateRole_ShouldFail_WhenRoleNameAlreadyExists()
        {
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<FakeRoleManager>();
            await roleManager.CreateRoleAsync("Duplicate", "Already exists");

            var command = new CreateRoleCommand("Duplicate", "Trying to duplicate");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role already exists.", content); // Adjust based on your implementation
        }

        [Fact]
        public async Task CreateRole_ShouldReturnJson_WhenSuccessful()
        {
            var command = new CreateRoleCommand("Support", "Handles support cases.");
            var response = await _client.PostAsJsonAsync("/api/users/create-role", command);

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task CreateRole_ShouldThrow_WhenCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var command = new CreateRoleCommand("TempRole", "Cancelled role creation");
            var content = JsonContent.Create(command);
            await Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(() =>
                _client.PostAsync("/api/users/create-role", content, cts.Token));

        }

    }
}
