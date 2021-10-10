using System;

namespace RFVault.Models
{
    [Serializable]
    public class PlayerVault
    {
        public int Id { get; set; }
        public ulong SteamId { get; set; }
        public string VaultName { get; set; }
        public ItemsWrapper VaultContent { get; set; } = new ItemsWrapper();
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public PlayerVault()
        {
        }

        public Vault GetVault()
        {
            return Vault.Parse(VaultName);
        }
    }
}