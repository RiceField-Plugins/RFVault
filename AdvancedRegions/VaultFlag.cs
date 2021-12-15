using ImperialPlugins.AdvancedRegions.RegionFlags;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFVault.AdvancedRegions
{
    [FlagInfo("Allows open vault.", SupportsGroupValues = true)]
    public sealed class VaultFlag : RegionFlag<VaultFlagOptions>
    {
        public void OnPlayerEnter(Player player)
        {
            var value = GetValue(player);
            if (value == null || !value.Enabled)
                return;
            // if (value.Vaults == null || value.Vaults.Count == 0)
            //     return;
            var pComponent = UnturnedPlayer.FromPlayer(player).GetComponent<PlayerComponent>();
            pComponent.AdvancedRegionsAllowOpenVault = true;
            // foreach (var vault in value.Vaults)
            //     pComponent.AdvancedRegionsAllowedVaults.Add(vault);
        }
    
        public void OnPlayerLeave(Player player)
        {
            var value = GetValue(player);
            if (value == null || !value.Enabled)
                return;
            // if (value.Vaults == null || value.Vaults.Count == 0)
            //     return;
            var pComponent = UnturnedPlayer.FromPlayer(player).GetComponent<PlayerComponent>();
            pComponent.AdvancedRegionsAllowOpenVault = false;
            // foreach (var vault in value.Vaults)
            //     pComponent.AdvancedRegionsAllowedVaults.Remove(vault);
        }
    }
}