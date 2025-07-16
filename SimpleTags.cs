using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using PlayerSettings;

using SimpleTags.Configs;
using SimpleTags.Services;
using SimpleTags.Managers;

namespace SimpleTags;

[MinimumApiVersion(318)]
public class SimpleTags : BasePlugin, IPluginConfig<SimpleTagsConfig>
{
    public override string ModuleName => "SimpleTags";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Simple tags plugins";

    public SimpleTagsConfig Config { get; set; } = new();
    private ITagStorageService? _storageService;
    private ISettingsApi? _settingsApi;
    private TagManager? _tagManager;
    private ChatManager? _chatManager;
    private readonly PluginCapability<ISettingsApi?> _settingsCapability = new("settings:nfcore");

    public void OnConfigParsed(SimpleTagsConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        foreach (var command in Config.Commands)
        {
            AddCommand(command, "Toggle tag display", OnTagCommand);
        }

        if (hotReload)
        {
            Server.NextFrame(() =>
            {
                InitializeServices();
                _chatManager?.RegisterEvents(this);
                _tagManager?.ReloadAllPlayerTags();
            });
        }
        else
        {

        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _settingsApi = _settingsCapability.Get();

        if (_settingsApi == null)
        {
            Server.PrintToConsole("[SimpleTags] PlayerSettings core not found!");
            Server.PrintToConsole("[SimpleTags] Tags will work but preferences won't persist between server restarts.");
        }
        else
        {
            Server.PrintToConsole("[SimpleTags] PlayerSettings found and will be used for persistence.");
        }

        InitializeServices();
        _chatManager?.RegisterEvents(this);

        if (hotReload)
        {
            _tagManager?.ReloadAllPlayerTags();
        }
    }

    private void InitializeServices()
    {
        _storageService = new PlayerSettingsTagStorage(_settingsApi);
        _tagManager = new TagManager(Config, _storageService);
        _chatManager = new ChatManager(_tagManager, _storageService);
        _ = InitializeStorageAsync();
    }

    private async Task InitializeStorageAsync()
    {
        try
        {
            if (_storageService != null)
            {
                var success = await _storageService.InitializeAsync();
                var storageType = _storageService.GetStorageType();

                Server.NextFrame(() =>
                {
                    if (success)
                    {
                        Server.PrintToConsole($"[SimpleTags] Storage initialized successfully: {storageType}");
                    }
                    else
                    {
                        Server.PrintToConsole($"[SimpleTags] Storage initialization failed, using: {storageType}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Server.NextFrame(() =>
            {
                Server.PrintToConsole($"[SimpleTags] Error initializing storage: {errorMessage}");
            });
        }
    }

    public override void Unload(bool hotReload)
    {
        _storageService?.ClearCache();
        _storageService = null;
        _tagManager = null;
        _chatManager = null;
    }

    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void OnTagCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid)
            return;

        if (!string.IsNullOrEmpty(Config.CommandPermissions) && !AdminManager.PlayerHasPermissions(player, Config.CommandPermissions))
        {
            commandInfo.ReplyToCommand($"{Localizer["prefix"]} {Localizer["no_permissions"]}");
            return;
        }

        if (_storageService == null || _tagManager == null)
        {
            commandInfo.ReplyToCommand($"{Localizer["prefix"]} Service not initialized");
            return;
        }

        _ = HandleToggleCommand(player, commandInfo);
    }

    private async Task HandleToggleCommand(CCSPlayerController player, CommandInfo commandInfo)
    {
        try
        {
            if (_storageService == null || _tagManager == null)
            {
                Server.NextFrame(() =>
                {
                    if (player.IsValid)
                    {
                        player.PrintToChat($"{Localizer["prefix"]} Service not initialized");
                    }
                });
                return;
            }

            var playerSlot = player.Slot;
            var playerName = player.PlayerName;

            await _storageService.TogglePlayerTagAsync(player);
            bool isEnabled = await _storageService.IsPlayerTagEnabledAsync(player);

            string message = isEnabled ? Localizer["tag_enabled"] : Localizer["tag_disabled"];
            string prefixMessage = Localizer["prefix"];

            Server.NextFrame(() =>
            {
                try
                {
                    var currentPlayer = Utilities.GetPlayerFromSlot(playerSlot);
                    if (currentPlayer != null && currentPlayer.IsValid && !currentPlayer.IsBot)
                    {
                        currentPlayer.PrintToChat($"{prefixMessage} {message}");
                        _tagManager?.SetPlayerClanTag(currentPlayer);
                    }
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"[SimpleTags] Error sending response to {playerName}: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            var playerName = player?.PlayerName ?? "Unknown";
            var playerSlot = player?.Slot ?? -1;
            Server.NextFrame(() =>
            {
                Server.PrintToConsole($"[SimpleTags] Error in HandleToggleCommand for {playerName}: {ex.Message}");
                if (playerSlot >= 0)
                {
                    var currentPlayer = Utilities.GetPlayerFromSlot(playerSlot);
                    if (currentPlayer != null && currentPlayer.IsValid && !currentPlayer.IsBot)
                    {
                        currentPlayer.PrintToChat($"{Localizer["prefix"]} An error occurred while processing your request");
                    }
                }
            });
        }
    }
}