using Microsoft.Extensions.DependencyInjection;


namespace Chess.UI.Test
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public void EveryRegisteredService_CanBeConstructedFromTheContainer()
        {
            IServiceCollection services = App.ConfigureServices();

            // Checks every constructor for missing registrations and circular dependencies without creating anything
            var options = new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true };

            using ServiceProvider provider = services.BuildServiceProvider(options);
        }


        [Fact]
        public void EveryService_IsRegisteredAsASingleton()
        {
            // Models and view models keep state and events that the whole app shares
            Assert.All(App.ConfigureServices(), service => Assert.Equal(ServiceLifetime.Singleton, service.Lifetime));
        }
    }
}
