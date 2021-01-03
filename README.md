# BitTorrentAnonymizer

Anonymizer / CGI proxy / web proxy

![docker image deploy](https://github.com/magicxor/BitTorrentAnonymizer/workflows/docker%20image%20deploy/badge.svg?branch=master)

![Imgur](https://i.imgur.com/t7YiiJe.png)

## Usage

* Pull [bittorrent-anonymizer](https://hub.docker.com/repository/docker/magicxor/bittorrent-anonymizer)

* Set the environment variables

```Shell
ASPNETCORE_ENVIRONMENT="Development"
BitTorrentAnonymizer_AllowedHosts="*"
BitTorrentAnonymizer_Logging__LogLevel__Default="Information"
BitTorrentAnonymizer_Logging__LogLevel__Microsoft="Warning"
BitTorrentAnonymizer_Logging__LogLevel__Microsoft.Hosting.Lifetime="Information"
BitTorrentAnonymizer_ProxyConfiguration__ProxyProtocol="Http"
BitTorrentAnonymizer_ProxyConfiguration__ProxyHost="some.proxy.com"
BitTorrentAnonymizer_ProxyConfiguration__ProxyPort="8080"
BitTorrentAnonymizer_ProxyConfiguration__ProxyAuthentication="true"
BitTorrentAnonymizer_ProxyConfiguration__ProxyUsername="user_123"
BitTorrentAnonymizer_ProxyConfiguration__ProxyPassword="p@ssw0rd"
```

* Start `bittorrent-anonymizer` and try to open `http://localhost/?targetUri=http://your.url.com` or `http://localhost/?targetUri=http%3A%2F%2Fyour.url.com`
