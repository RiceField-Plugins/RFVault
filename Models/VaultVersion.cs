using System;

namespace RFVault.Models
{
    [Serializable]
    public class VaultVersion
    {
        public uint DatabaseVersion { get; set; }
        public VaultVersion()
        {
            
        }
    }
}