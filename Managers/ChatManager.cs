using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using SimpleTags.Services;
using System.Reflection;

namespace SimpleTags.Managers
{
    public class ChatManager
    {
        private readonly TagManager _tagManager;
        private readonly ITagStorageService _storageService;

        public ChatManager(TagManager tagManager, ITagStorageService storageService)
        {
            _tagManager = tagManager;
            _storageService = storageService;
        }

        public void RegisterEvents(BasePlugin plugin)
        {
            plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
            plugin.RegisterListener<Listeners.OnClientAuthorized>((playerSlot, steamId) => OnClientAuthorized(playerSlot, steamId));
            plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
            plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            plugin.AddCommandListener("say", OnPlayerChat);
            plugin.AddCommandListener("say_team", OnPlayerChatTeam);
        }

        private void OnMapStart(string mapName)
        {
        }

        private void OnClientAuthorized(int playerSlot, SteamID steamId)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return;

            Server.NextFrame(() =>
            {
                var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () => _tagManager.SetPlayerClanTag(player));
            });
        }

        private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

            Server.NextFrame(() =>
            {
                var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () => _tagManager.SetPlayerClanTag(player));
            });

            return HookResult.Continue;
        }

        private void OnClientDisconnect(int playerSlot)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return;

            _storageService.OnPlayerDisconnect(player);
            _tagManager.OnPlayerDisconnect(player);
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

            Server.NextFrame(() =>
            {
                var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(1.5f, () => _tagManager.SetPlayerClanTag(player));
            });

            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

            Server.NextFrame(() =>
            {
                var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(1.5f, () => _tagManager.SetPlayerClanTag(player));
            });

            return HookResult.Continue;
        }

        private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
        {
            if (player == null || !player.IsValid || info.GetArg(1).Length == 0 || player.AuthorizedSteamID == null)
                return HookResult.Continue;

            if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv")
                return HookResult.Continue;

            if (!_storageService.IsPlayerTagEnabled(player))
            {
                return HookResult.Continue;
            }

            var tagInfo = _tagManager.GetPlayerTagInfo(player);
            if (tagInfo == null)
                return HookResult.Continue;

            string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
            string message = $" {deadIcon}{tagInfo.Prefix}{tagInfo.NickColor}{player.PlayerName}{ChatColors.Default}: {tagInfo.MessageColor}{info.GetArg(1)}";

            if (tagInfo.IsEveryoneTag && !tagInfo.TeamChatEnabled)
                return HookResult.Continue;

            Server.PrintToChatAll(ReplaceTags(message, player.TeamNum));
            return HookResult.Handled;
        }

        private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
        {
            if (player == null || !player.IsValid || info.GetArg(1).Length == 0 || player.AuthorizedSteamID == null)
                return HookResult.Continue;

            if (info.GetArg(1).StartsWith("@") && AdminManager.PlayerHasPermissions(player, "@css/chat"))
            {
                foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && AdminManager.PlayerHasPermissions(p, "@css/chat")))
                {
                    p.PrintToChat($" {ChatColors.Lime}(ADMIN) {ChatColors.Default}{player.PlayerName}: {info.GetArg(1).Remove(0, 1)}");
                }
                return HookResult.Handled;
            }

            if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv")
                return HookResult.Continue;

            if (!_storageService.IsPlayerTagEnabled(player))
            {
                return HookResult.Continue;
            }

            var tagInfo = _tagManager.GetPlayerTagInfo(player);
            if (tagInfo == null)
                return HookResult.Continue;

            string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
            foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
            {
                string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{tagInfo.Prefix}{tagInfo.NickColor}{player.PlayerName}{ChatColors.Default}: {tagInfo.MessageColor}{info.GetArg(1)}";
                p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
            }

            return HookResult.Handled;
        }

        private static string TeamName(int teamNum)
        {
            return teamNum switch
            {
                0 => "(NONE)",
                1 => "(SPEC)",
                2 => $"{ChatColors.Yellow}(T)",
                3 => $"{ChatColors.Blue}(CT)",
                _ => ""
            };
        }

        private static string TeamColor(int teamNum)
        {
            return teamNum switch
            {
                2 => $"{ChatColors.Gold}",
                3 => $"{ChatColors.Blue}",
                _ => ""
            };
        }

        private static string ReplaceTags(string message, int teamNum = 0)
        {
            if (message.Contains('{'))
            {
                string modifiedValue = message;
                foreach (FieldInfo field in typeof(ChatColors).GetFields())
                {
                    string pattern = $"{{{field.Name}}}";
                    if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                }
                return modifiedValue.Replace("{TEAMCOLOR}", TeamColor(teamNum));
            }

            return message;
        }
    }
}