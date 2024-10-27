using Microsoft.Extensions.DependencyInjection;

namespace ControllerTests;


public static class HttpClientFactoryHelper
{
    public const string TestClientName = "TestClient";
    
    public static IHttpClientFactory CreateHttpClientFactory(string baseUrl)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(TestClientName, client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IHttpClientFactory>();
    }
}