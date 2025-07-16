using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PlayerSettings;

namespace SimpleTags.Services
{
    public class PlayerSettingsTagStorage : ITagStorageService
    {
        private readonly ISettingsApi? _settingsApi;
        private readonly HashSet<int> _fallbackDisabledPlayers;
        private const string TAG_SETTING_KEY = "simpletags_enabled";

        public PlayerSettingsTagStorage(ISettingsApi? settingsApi)
        {
            _settingsApi = settingsApi;
            _fallbackDisabledPlayers = new HashSet<int>();
        }

        public async Task<bool> InitializeAsync()
        {
            await Task.CompletedTask;
            if (_settingsApi != null)
            {
                Server.PrintToConsole("[SimpleTags] Using PlayerSettings for storage");
                return true;
            }
            else
            {
                Server.PrintToConsole("[SimpleTags] PlayerSettings not available, using memory fallback");
                return false;
            }
        }

        public bool IsPlayerTagEnabled(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.AuthorizedSteamID == null)
                return true;

            if (_settingsApi != null)
            {
                try
                {
                    var tagEnabled = _settingsApi.GetPlayerSettingsValue(player, TAG_SETTING_KEY, "true");
                    return string.Equals(tagEnabled, "true", StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"[SimpleTags] Error getting player setting for {player.PlayerName}: {ex.Message}");
                    return !_fallbackDisabledPlayers.Contains(player.Slot);
                }
            }

            return !_fallbackDisabledPlayers.Contains(player.Slot);
        }

        public async Task<bool> IsPlayerTagEnabledAsync(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.AuthorizedSteamID == null)
                return true;

            if (_settingsApi != null)
            {
                try
                {
                    var playerSlot = player.Slot;
                    var playerName = player.PlayerName;
                    var getValueTask = new TaskCompletionSource<bool>();

                    Server.NextFrame(() =>
                    {
                        try
                        {
                            var currentPlayer = Utilities.GetPlayerFromSlot(playerSlot);
                            if (currentPlayer != null && currentPlayer.IsValid && !currentPlayer.IsBot)
                            {
                                var tagEnabled = _settingsApi.GetPlayerSettingsValue(currentPlayer, TAG_SETTING_KEY, "true");
                                var result = string.Equals(tagEnabled, "true", StringComparison.OrdinalIgnoreCase);
                                getValueTask.SetResult(result);
                            }
                            else
                            {
                                getValueTask.SetResult(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            getValueTask.SetException(ex);
                        }
                    });

                    return await getValueTask.Task;
                }
                catch (Exception ex)
                {
                    var playerName = player.PlayerName;
                    var errorMessage = ex.Message;

                    Server.NextFrame(() =>
                    {
                        Server.PrintToConsole($"[SimpleTags] Error loading preferences for {playerName}: {errorMessage}");
                    });
                    return !_fallbackDisabledPlayers.Contains(player.Slot);
                }
            }

            return !_fallbackDisabledPlayers.Contains(player.Slot);
        }

        public void TogglePlayerTag(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.AuthorizedSteamID == null)
                return;

            if (_settingsApi != null)
            {
                try
                {
                    var currentValue = _settingsApi.GetPlayerSettingsValue(player, TAG_SETTING_KEY, "true");
                    var newValue = string.Equals(currentValue, "true", StringComparison.OrdinalIgnoreCase) ? "false" : "true";

                    _settingsApi.SetPlayerSettingsValue(player, TAG_SETTING_KEY, newValue);

                    Server.PrintToConsole($"[SimpleTags] Player {player.PlayerName} tag toggled from '{currentValue}' to '{newValue}'");
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"[SimpleTags] Error toggling player setting for {player.PlayerName}: {ex.Message}");
                    if (_fallbackDisabledPlayers.Contains(player.Slot))
                    {
                        _fallbackDisabledPlayers.Remove(player.Slot);
                    }
                    else
                    {
                        _fallbackDisabledPlayers.Add(player.Slot);
                    }
                }
            }
            else
            {
                if (_fallbackDisabledPlayers.Contains(player.Slot))
                {
                    _fallbackDisabledPlayers.Remove(player.Slot);
                }
                else
                {
                    _fallbackDisabledPlayers.Add(player.Slot);
                }
            }
        }

        public async Task TogglePlayerTagAsync(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.AuthorizedSteamID == null)
                return;

            var playerSlot = player.Slot;
            var playerName = player.PlayerName;
            var steamId = player.AuthorizedSteamID.SteamId64;

            if (_settingsApi != null)
            {
                try
                {
                    string currentValue = "";
                    var getCurrentTask = new TaskCompletionSource<string>();

                    Server.NextFrame(() =>
                    {
                        try
                        {
                            var currentPlayer = Utilities.GetPlayerFromSlot(playerSlot);
                            if (currentPlayer != null && currentPlayer.IsValid && !currentPlayer.IsBot)
                            {
                                var value = _settingsApi.GetPlayerSettingsValue(currentPlayer, TAG_SETTING_KEY, "true");
                                getCurrentTask.SetResult(value);
                            }
                            else
                            {
                                getCurrentTask.SetResult("true");
                            }
                        }
                        catch (Exception ex)
                        {
                            getCurrentTask.SetException(ex);
                        }
                    });

                    currentValue = await getCurrentTask.Task;
                    var newValue = string.Equals(currentValue, "true", StringComparison.OrdinalIgnoreCase) ? "false" : "true";

                    var setValueTask = new TaskCompletionSource<bool>();
                    Server.NextFrame(() =>
                    {
                        try
                        {
                            var currentPlayer = Utilities.GetPlayerFromSlot(playerSlot);
                            if (currentPlayer != null && currentPlayer.IsValid && !currentPlayer.IsBot)
                            {
                                _settingsApi.SetPlayerSettingsValue(currentPlayer, TAG_SETTING_KEY, newValue);
                                Server.PrintToConsole($"[SimpleTags] Player {playerName} tag toggled from '{currentValue}' to '{newValue}'");
                            }
                            setValueTask.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            setValueTask.SetException(ex);
                        }
                    });

                    await setValueTask.Task;
                }
                catch (Exception ex)
                {
                    Server.NextFrame(() =>
                    {
                        Server.PrintToConsole($"[SimpleTags] Error toggling player setting for {playerName}: {ex.Message}");
                    });

                    if (_fallbackDisabledPlayers.Contains(playerSlot))
                    {
                        _fallbackDisabledPlayers.Remove(playerSlot);
                    }
                    else
                    {
                        _fallbackDisabledPlayers.Add(playerSlot);
                    }
                }
            }
            else
            {
                if (_fallbackDisabledPlayers.Contains(playerSlot))
                {
                    _fallbackDisabledPlayers.Remove(playerSlot);
                }
                else
                {
                    _fallbackDisabledPlayers.Add(playerSlot);
                }
            }
        }

        public void OnPlayerDisconnect(CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
                return;

            _fallbackDisabledPlayers.Remove(player.Slot);
        }

        public void ClearCache()
        {
            _fallbackDisabledPlayers.Clear();
        }

        public string GetStorageType()
        {
            return _settingsApi != null ? "PlayerSettings" : "Memory (Fallback)";
        }
    }
}