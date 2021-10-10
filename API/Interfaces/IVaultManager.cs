using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Models;

namespace RFVault.API.Interfaces
{
    public interface IVaultManager
    {
        UniTask<int> AddAsync(ulong steamId, Vault vault);
        PlayerVault Get(ulong steamId, Vault vault);
        UniTask<bool> UpdateAsync(ulong steamId, Vault vault);
        UniTask MigrateAsync(EDatabase from, EDatabase to);
    }
}