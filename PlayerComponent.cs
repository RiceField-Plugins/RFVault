using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using RFVault.DatabaseManagers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFVault
{
    public class PlayerComponent : UnturnedPlayerComponent
    {
        internal Vault SelectedVault { get; set; }
        internal PlayerVault PlayerVault { get; set; }
        internal Items PlayerVaultItems { get; set; }

        protected override void Load()
        {
            var vault = VaultUtil.GetVaults(Player).FirstOrDefault();
            if (vault != null)
                SelectedVault = vault;

            Player.Player.inventory.onInventoryResized += OnInventoryResized;
        }

        protected override void Unload()
        {
            Player.Player.inventory.onInventoryResized -= OnInventoryResized;
        }

        private void OnInventoryResized(byte page, byte newwidth, byte newheight)
        {
            // Logger.LogWarning($"[DEBUG] OnInventoryResized {Player.CharacterName} page:{page} newwidth:{newwidth} newheight:{newheight}");
            if (page == PlayerInventory.STORAGE && newwidth == 0 && newheight == 0 && PlayerVault != null && PlayerVaultItems != null)
            {
                var itemsWrapper = ItemsWrapper.Create(PlayerVaultItems);
                PlayerVault.VaultContent = itemsWrapper;
                // Logger.LogWarning($"[DEBUG] OnInventoryResized {Player.CharacterName} PlayerVaultItems:{PlayerVaultItems.items.Count}");
                Task.Run(async () =>
                {
                    await VaultManager.UpdateAsync(PlayerVault);
                    PlayerVault = null;
                    PlayerVaultItems = null;
                }).Forget(
                    e =>
                    {
                        Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] OnStorageUpdated: {e.Message}");
                        Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                    });
                return;
            }
        }
    }
}