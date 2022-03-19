using System;
using System.IO;
using RFVault.Enums;
using Rocket.Core.Logging;

namespace RFVault.DatabaseManagers
{
    internal static class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "vault.db";
        private static string LiteDB_FilePath;
        internal static string LiteDB_ConnectionString;

        internal static string MySql_TableName;
        internal static string MySql_ConnectionString;

        internal static void Init()
        {
            LiteDB_FilePath = Path.Combine(Plugin.Inst.Directory, LiteDB_FileName);
            LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";
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
        }
    }
}