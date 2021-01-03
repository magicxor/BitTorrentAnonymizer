using System;
using System.Net;
using BitTorrentAnonymizer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;

namespace BitTorrentAnonymizer.Factories
{
    public class WebProxyFactory
    {
        private readonly ILogger<WebProxyFactory> _logger;
        private readonly ProxyConfiguration _proxyConfiguration;

        public WebProxyFactory(ILogger<WebProxyFactory> logger,
            IOptions<ProxyConfiguration> proxyConfiguration)
        {
            _logger = logger;
            _proxyConfiguration = proxyConfiguration.Value;

            _logger.LogInformation($"Proxy: host {_proxyConfiguration.ProxyHost}, port {_proxyConfiguration.ProxyPort}, protocol {_proxyConfiguration.ProxyProtocol}, authentication {_proxyConfiguration.ProxyAuthentication}");
        }

        public IWebProxy CreateWebProxy()
        {
            switch (_proxyConfiguration.ProxyProtocol)
            {
                case ProxyProtocols.Http:
                    var proxyUriHttp = new UriBuilder("http", _proxyConfiguration.ProxyHost, _proxyConfiguration.ProxyPort).Uri;
                    return _proxyConfiguration.ProxyAuthentication
                        ? new WebProxy(proxyUriHttp, true, null, new NetworkCredential(_proxyConfiguration.ProxyUsername, _proxyConfiguration.ProxyPassword))
                        : new WebProxy(proxyUriHttp);
                case ProxyProtocols.Socks5:
                    return _proxyConfiguration.ProxyAuthentication
                        ? new HttpToSocks5Proxy(_proxyConfiguration.ProxyHost, _proxyConfiguration.ProxyPort, _proxyConfiguration.ProxyUsername, _proxyConfiguration.ProxyPassword)
                        : new HttpToSocks5Proxy(_proxyConfiguration.ProxyHost, _proxyConfiguration.ProxyPort);
                default:
                    return null;
            }
        }
    }
}
