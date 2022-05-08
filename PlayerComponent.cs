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
        internal bool IsBusy { get; set; }
        internal Vault SelectedVault { get; set; }
        internal PlayerVault PlayerVault { get; set; }
        internal Items PlayerVaultItems { get; set; }

        protected override void Load()
        {
            LoadInternal();
        }

        protected override void Unload()
        {
            UnloadInternal();
        }

        internal void LoadInternal()
        {
            var vault = VaultUtil.GetVaults(Player).FirstOrDefault();
            if (vault != null)
                SelectedVault = vault;

            Player.Player.inventory.onInventoryResized += OnInventoryResized;
        }

        internal void UnloadInternal()
        {
            Player.Player.inventory.onInventoryResized -= OnInventoryResized;
            
            if (PlayerVaultItems != null)
                OnInventoryResized(PlayerInventory.STORAGE, 0, 0);
        }

        private void OnInventoryResized(byte page, byte newwidth, byte newheight)
        {
            // Logger.LogWarning($"[DEBUG] OnInventoryResized {Player.CharacterName} page:{page} newwidth:{newwidth} newheight:{newheight}");
            if (page == PlayerInventory.STORAGE && newwidth == 0 && newheight == 0 && PlayerVault != null && PlayerVaultItems != null)
            {
                if (!IsBusy)
                {
                    var itemsWrapper = ItemsWrapper.Create(PlayerVaultItems);
                    PlayerVault.VaultContent = itemsWrapper;
                }
                
                IsBusy = true;
                DatabaseManager.Queue.Enqueue(async () =>
                {
                    await VaultManager.UpdateAsync(PlayerVault);
                    PlayerVaultItems.clear();
                    PlayerVaultItems.items.TrimExcess();
                    PlayerVault = null;
                    PlayerVaultItems = null;
                    IsBusy = false;
                });
            }
        }
    }
}