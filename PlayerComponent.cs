using System.Collections.Generic;
using System.Linq;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Unturned.Player;

namespace RFVault
{
    public class PlayerComponent : UnturnedPlayerComponent
    {
        internal bool AdvancedRegionsAllowOpenVault { get; set; }
        internal Vault SelectedVault { get; set; }
        internal PlayerVault CachedVault { get; set; }
        internal bool IsSubmitting { get; set; }
        internal bool IsProcessingVault { get; set; }

        protected override void Load()
        {
            var vault = VaultUtil.GetVaults(Player).FirstOrDefault();
            if (vault != null)
                SelectedVault = vault;
            AdvancedRegionsAllowOpenVault = false;
            IsSubmitting = false;
            IsProcessingVault = false;
        }

        protected override void Unload()
        {
        }
    }
}