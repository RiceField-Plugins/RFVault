using System;
using System.IO;
using RFVault.Enums;
using Rocket.Core.Logging;

namespace RFVault.DatabaseManagers
{
    internal class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "vault.db";
        private static readonly string LiteDB_FilePath = Path.Combine(Plugin.Inst.Directory, LiteDB_FileName);
        internal static readonly string LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";

        internal static string MySql_TableName;
        internal static string MySql_ConnectionString;

        internal readonly VaultManager VaultManager;

        internal DatabaseManager()
        {
            try
            {
                if (Plugin.Conf.Database == EDatabase.MYSQL)
                {
                    var index = Plugin.Conf.MySqlConnectionString.LastIndexOf("TABLENAME", StringComparison.Ordinal);
                    if (index == -1)
                    {
                        MySql_TableName = "rfvault";
                        MySql_ConnectionString = Plugin.Conf.MySqlConnectionString;
                    }
                    else
                    {
                        var substr = Plugin.Conf.MySqlConnectionString.Substring(
                            Plugin.Conf.MySqlConnectionString.LastIndexOf('='));
                        MySql_TableName = substr.Substring(1, substr.Length - 2);
                        MySql_ConnectionString = Plugin.Conf.MySqlConnectionString.Remove(index);
                    }
                }
                
                VaultManager = new VaultManager();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] DatabaseManager Initializing: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }
    }
}