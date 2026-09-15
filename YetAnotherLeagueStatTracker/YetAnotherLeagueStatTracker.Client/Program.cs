using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using YetAnotherLeagueStatTracker.Services;

namespace YetAnotherLeagueStatTracker.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        
        builder.Services.AddSingleton<DataDragon>();
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();
        
        await builder.Build().RunAsync();
    }
}