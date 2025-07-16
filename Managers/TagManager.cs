using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using SimpleTags.Services;
using SimpleTags.Configs;

namespace SimpleTags.Managers
{
    public class TagManager
    {
        private readonly SimpleTagsConfig _config;
        private readonly ITagStorageService _storageService;

        public TagManager(SimpleTagsConfig config, ITagStorageService storageService)
        {
            _config = config;
            _storageService = storageService;
        }

        public void SetPlayerClanTag(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null) return;

            string tag = "";
            if (_storageService.IsPlayerTagEnabled(player))
            {
                tag = GetPlayerScoreboardTag(player);
            }

            if (player.Clan != tag)
            {
                player.Clan = tag;
                Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
            }
        }

        private string GetPlayerScoreboardTag(CCSPlayerController player)
        {
            string steamid = player.SteamID!.ToString();
            if (_config.Tags.TryGetValue(steamid, out var playerTag))
            {
                return playerTag.Scoreboard ?? "";
            }

            foreach (var tagKey in _config.Tags.Keys)
            {
                if (tagKey.StartsWith("#"))
                {
                    string group = tagKey;
                    try
                    {
                        bool inGroup = AdminManager.PlayerInGroup(player, group);
                        if (inGroup && _config.Tags.TryGetValue(group, out var groupTag))
                        {
                            return groupTag.Scoreboard ?? "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Server.PrintToConsole($"[SimpleTags] Error checking group {group} for player {player.PlayerName}: {ex.Message}");
                    }
                }

                if (tagKey.StartsWith("@"))
                {
                    string permission = tagKey;
                    try
                    {
                        bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);
                        if (hasPermission && _config.Tags.TryGetValue(permission, out var permissionTag))
                        {
                            return permissionTag.Scoreboard ?? "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Server.PrintToConsole($"[SimpleTags] Error checking permission {permission} for player {player.PlayerName}: {ex.Message}");
                    }
                }
            }

            if (_config.Tags.TryGetValue("everyone", out var everyoneTag))
            {
                return everyoneTag.Scoreboard ?? "";
            }

            return "";
        }

        public void ReloadAllPlayerTags()
        {
            var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
            foreach (var player in players)
            {
                SetPlayerClanTag(player);
            }
        }

        public TagInfo? GetPlayerTagInfo(CCSPlayerController player)
        {
            if (player?.AuthorizedSteamID == null)
                return null;

            string steamid = player.AuthorizedSteamID.SteamId64.ToString();
            if (_config.Tags.TryGetValue(steamid, out var playerTag))
            {
                return new TagInfo
                {
                    Prefix = playerTag.Prefix,
                    NickColor = playerTag.NickColor,
                    MessageColor = playerTag.MessageColor
                };
            }

            foreach (var tagKey in _config.Tags.Keys)
            {
                if (tagKey.StartsWith("#"))
                {
                    string group = tagKey;
                    try
                    {
                        bool inGroup = AdminManager.PlayerInGroup(player, group);
                        if (inGroup && _config.Tags.TryGetValue(group, out var groupTag))
                        {
                            return new TagInfo
                            {
                                Prefix = groupTag.Prefix,
                                NickColor = groupTag.NickColor,
                                MessageColor = groupTag.MessageColor
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Server.PrintToConsole($"[SimpleTags] Error checking group {group} for player {player.PlayerName}: {ex.Message}");
                    }
                }

                if (tagKey.StartsWith("@"))
                {
                    string permission = tagKey;
                    try
                    {
                        bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);
                        if (hasPermission && _config.Tags.TryGetValue(permission, out var permissionTag))
                        {
                            return new TagInfo
                            {
                                Prefix = permissionTag.Prefix,
                                NickColor = permissionTag.NickColor,
                                MessageColor = permissionTag.MessageColor
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Server.PrintToConsole($"[SimpleTags] Error checking permission {permission} for player {player.PlayerName}: {ex.Message}");
                    }
                }
            }

            if (_config.Tags.TryGetValue("everyone", out var everyoneTag))
            {
                return new TagInfo
                {
                    Prefix = everyoneTag.Prefix,
                    NickColor = everyoneTag.NickColor,
                    MessageColor = everyoneTag.MessageColor,
                    IsEveryoneTag = true,
                    TeamChatEnabled = everyoneTag.TeamChat
                };
            }

            return null;
        }
    }

    public class TagInfo
    {
        public string Prefix { get; set; } = "";
        public string NickColor { get; set; } = "";
        public string MessageColor { get; set; } = "";
        public bool IsEveryoneTag { get; set; } = false;
        public bool TeamChatEnabled { get; set; } = false;
    }
}