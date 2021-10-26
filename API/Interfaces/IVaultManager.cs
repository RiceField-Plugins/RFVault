using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.Models;

namespace RFVault.API.Interfaces
{
    public interface IVaultManager
    {
        Task<int> AddAsync(ulong steamId, Vault vault);
        PlayerVault Get(ulong steamId, Vault vault);
        Task<bool> UpdateAsync(ulong steamId, Vault vault);
        Task MigrateAsync(EDatabase from, EDatabase to);
#if DEBUG
        Task MigrateLockerAsync(EDatabase to);
#endif
    }
}