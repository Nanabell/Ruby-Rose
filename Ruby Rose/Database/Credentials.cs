namespace RubyRose.Database
{
    public class Credentials
    {
        public string Prefix { get; set; }
        public string NowPlaying { get; set; }
        public string Token { get; set; }
        public Database Database { get; set; }
    }

    public class Database
    {
        public string Mongo { get; set; }
        public string Efcore { get; set; }
    }
}