using System.IO;
using RFVault.API.Interfaces;

namespace RFVault.DatabaseManagers
{
    internal class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "vault.db";
        internal static readonly string LiteDB_FilePath = Path.Combine(Plugin.Inst.Directory, LiteDB_FileName);
        internal static readonly string LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";
        
        internal static string MySql_ConnectionString = Plugin.Conf.MySqlConnectionString;

        internal IVaultManager VaultManager;
        
        internal DatabaseManager()
        {
            VaultManager = new VaultManager();
        }
    }
}