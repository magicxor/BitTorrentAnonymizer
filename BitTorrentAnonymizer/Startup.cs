using System.Net.Http;
using BitTorrentAnonymizer.Factories;
using BitTorrentAnonymizer.Middleware;
using BitTorrentAnonymizer.Models;
using BitTorrentAnonymizer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitTorrentAnonymizer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ProxyConfiguration>(_configuration.GetSection(nameof(ProxyConfiguration)));

            services.AddSingleton<WebProxyFactory>();

            services.AddHttpClient<AnonymizerHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var webProxyFactory = provider.GetRequiredService<WebProxyFactory>();
                    var webProxy = webProxyFactory.CreateWebProxy();
                    return new HttpClientHandler
                    {
                        Proxy = webProxy,
                        UseProxy = webProxy != null,
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ProxyMiddleware>();
        }
    }
}
