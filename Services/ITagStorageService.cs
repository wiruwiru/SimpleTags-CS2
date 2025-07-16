using CounterStrikeSharp.API.Core;

namespace SimpleTags.Services
{
    public interface ITagStorageService
    {
        Task<bool> InitializeAsync();
        bool IsPlayerTagEnabled(CCSPlayerController player);
        Task<bool> IsPlayerTagEnabledAsync(CCSPlayerController player);
        void TogglePlayerTag(CCSPlayerController player);
        Task TogglePlayerTagAsync(CCSPlayerController player);
        void OnPlayerDisconnect(CCSPlayerController player);
        void ClearCache();
        string GetStorageType();
    }
}