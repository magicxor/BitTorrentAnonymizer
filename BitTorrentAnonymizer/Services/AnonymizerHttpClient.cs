using System.Net.Http;

namespace BitTorrentAnonymizer.Services
{
    public class AnonymizerHttpClient
    {
        public readonly HttpClient Client;

        public AnonymizerHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
