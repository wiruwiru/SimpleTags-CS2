using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace SimpleTags.Configs
{
    public class SimpleTagsConfig : BasePluginConfig
    {
        [JsonPropertyName("Commands")]
        public List<string> Commands { get; set; } = new List<string> { "css_tag", "css_tags" };

        [JsonPropertyName("CommandPermissions")]
        public string CommandPermissions { get; set; } = "";

        [JsonPropertyName("Tags")]
        public Dictionary<string, TagConfiguration> Tags { get; set; } = new Dictionary<string, TagConfiguration>
        {
            ["#css/admin"] = new TagConfiguration
            {
                Prefix = "{RED}[ADMIN]",
                NickColor = "{RED}",
                MessageColor = "{GOLD}",
                Scoreboard = "[ADMIN]"
            },
            ["@css/chat"] = new TagConfiguration
            {
                Prefix = "{GREEN}[CHAT]",
                NickColor = "{RED}",
                MessageColor = "{GOLD}",
                Scoreboard = "[CHAT]"
            },
            ["76561199074660131"] = new TagConfiguration
            {
                Prefix = "{RED}[ADMIN]",
                NickColor = "{RED}",
                MessageColor = "{GOLD}",
                Scoreboard = "[ADMIN]"
            },
            ["everyone"] = new TagConfiguration
            {
                TeamChat = false,
                Prefix = "{Grey}[Player]",
                NickColor = "",
                MessageColor = "",
                Scoreboard = "[Player]"
            }
        };

        [JsonPropertyName("Settings")]
        public TagSettings Settings { get; set; } = new();
    }

    public class TagConfiguration
    {
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = "";

        [JsonPropertyName("nick_color")]
        public string NickColor { get; set; } = "";

        [JsonPropertyName("message_color")]
        public string MessageColor { get; set; } = "";

        [JsonPropertyName("scoreboard")]
        public string Scoreboard { get; set; } = "";

        [JsonPropertyName("team_chat")]
        public bool TeamChat { get; set; } = true;
    }

    public class TagSettings
    {
        [JsonPropertyName("DefaultEnabled")]
        public bool DefaultEnabled { get; set; } = true;

        [JsonPropertyName("TagMethod")]
        public string TagMethod { get; set; } = "ClanTag"; // "ClanTag" or "Rename"
    }

    public enum TagMethodType
    {
        ClanTag,
        Rename
    }
}