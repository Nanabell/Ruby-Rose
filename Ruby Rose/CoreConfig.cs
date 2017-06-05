using Newtonsoft.Json;
using System.IO;

namespace RubyRose
{
    public class CoreConfig
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; } = "!";

        [JsonProperty("game")]
        public string Game { get; set; } = "TestBot";

        [JsonProperty("is-main-bot")]
        public bool IsMainBot { get; set; } = true;

        [JsonProperty("token")]
        public string Token { get; set; } = "INSERT TOKEN HERE. DO !N!!OT!! MAKE THIS PUBLIC";

        [JsonProperty("mongodb-connectionString")]
        public string MongoConnectionString { get; set; } = "mongodb://Username:Password@localhost";

        [JsonProperty("version-nr")]
        public double Version { get; set; } = 1.0;

        public static CoreConfig ReadConfig()
        {
            if (!File.Exists("config.json"))
            {
                File.WriteAllText("config.json", JsonConvert.SerializeObject(new CoreConfig(), Formatting.Indented));
                throw new FileNotFoundException("Config file Not Found - Generating new Template..");
            }

            var jsonstring = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<CoreConfig>(jsonstring);

            return config;
        }
    }
}