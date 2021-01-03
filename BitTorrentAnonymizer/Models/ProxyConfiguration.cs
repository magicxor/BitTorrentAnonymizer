namespace BitTorrentAnonymizer.Models
{
    public class ProxyConfiguration
    {
        public ProxyProtocols ProxyProtocol { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public bool ProxyAuthentication { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
    }
}
