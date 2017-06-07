using System;
using Newtonsoft.Json;
using System.IO;
using NLog;

namespace RubyRose
{
    public class CoreConfig
    {
        private static readonly Logger Logger = LogManager.GetLogger("Config");
        
        [JsonProperty("token")]
        public string Token { get; set; } = "INSERT TOKEN HERE. DO NOT MAKE THIS PUBLIC";

        [JsonProperty("command_on_mention")]
        public bool TriggerOnMention { get; set; } = true;

        [JsonProperty("prefix")]
        public string Prefix { get; set; } = "!";

        [JsonProperty("game")]
        public string Game { get; set; } = "TestBot";

        public class ConfigDatabase
        {
            [JsonProperty("host")]
            public string Host { get; set; } = "localhost";

            [JsonProperty("port")]
            public int Port { get; set; } = 27017;

            [JsonProperty("db")]
            public string Db { get; set; } = "admin";

            [JsonProperty("user")]
            public string Username { get; set; } = "user";

            [JsonProperty("password")]
            public string Password { get; set; } = "password";

            [JsonIgnore]
            public string ConnectionString => $"mongodb://{Username}:{Password}@{Host}:{Port}/{Db}";
        }
        
        [JsonProperty("MongoDb")]
        public ConfigDatabase Database { get; set; } = new ConfigDatabase();

        [JsonProperty("version-nr")]
        public double Version { get; set; } = 1.0;

        
        public static CoreConfig Load()
        {
            Logger.Info("Loading Configuration from config.json");
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                return JsonConvert.DeserializeObject<CoreConfig>(json);
            }
            var config = new CoreConfig();
            config.Save();
            throw new InvalidOperationException("configuration file created; insert token and restart.");
        }
        
        public void Save()
        {
            Logger.Info("Saving Configuration to config.json");
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText("config.json", json);
        }
    }
}