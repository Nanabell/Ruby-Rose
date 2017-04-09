using Newtonsoft.Json;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        [JsonProperty("tokens")]
        public Tokens Token { get; set; } = new Tokens();

        [JsonProperty("databases")]
        public Databases Database { get; set; } = new Databases();

        [JsonProperty("is-sharded")]
        public bool IsSharded { get; set; } = false;

        [JsonProperty("shard-count")]
        public int TotalShards { get; set; } = 1;

        [JsonProperty("owner-ids")]
        public ulong[] OwnerIds { get; set; } = new ulong[0];

        [JsonProperty("fallback-name")]
        public string FallbackName { get; set; } = null;

        public static CoreConfig ReadConfig()
        {
            if (!File.Exists("config.json"))
            {
                File.WriteAllText("config.json", JsonConvert.SerializeObject(new CoreConfig(), Formatting.Indented));
                throw new FileNotFoundException("Config file Not Found - Generating new Template..");
            }

            var jsonstring = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<CoreConfig>(jsonstring);

            if (config.FallbackName != null)
                DatabaseExtentions.LoadFallbackName(config.FallbackName);

            return config;
        }
    }

    public class Databases
    {
        [JsonProperty("mongo")]
        public string Mongo { get; set; } = "Connectionstring to MongoDatabase";
    }

    public class Tokens
    {
        [JsonProperty("main")]
        public string Main { get; set; } = "Main Bot Token";

        [JsonProperty("dev")]
        public string Dev { get; set; } = "OPTIONAL Test Bot Token";
    }
}