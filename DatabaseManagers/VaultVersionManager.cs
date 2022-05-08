using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Storages;
using RFVault.Enums;
using RFVault.Models;
using Rocket.Core.Logging;

namespace RFVault.DatabaseManagers
{
    internal static class VaultVersionManager
    {
        internal static bool Ready { get; set; }
        private static VaultVersion Json_Collection { get; set; } = new();
        private static VaultVersion MigrateCollection { get; set; } = new();


        private const string Json_FileName = "vault_version.json";
        private static JsonDataStore<VaultVersion> Json_DataStore { get; set; }

        private static string MySql_TableName => $"{DatabaseManager.MySql_TableName}_version";

        private const string MySql_CreateTableQuery =
            "`DatabaseVersion` INT UNSIGNED NOT NULL DEFAULT 0";

        internal static void Initialize()
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<VaultVersion>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload();
                        break;
                    case EDatabase.MYSQL:
                        MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                        break;
                }

                Ready = true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager Initializing: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void JSON_Reload(bool migrate = false)
        {
            try
            {
                if (migrate)
                {
                    MigrateCollection = Json_DataStore.Load() ?? new VaultVersion();
                    return;
                }

                Json_Collection = Json_DataStore.Load() ?? new VaultVersion();
                Json_DataStore.Save(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager JSON_Reload: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void MySQL_CreateTable(string tableName, string createTableQuery)
        {
            try
            {
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    Dapper.SqlMapper.Execute(connection,
                        $"CREATE TABLE IF NOT EXISTS `{tableName}` ({createTableQuery});");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager MySQL_CreateTable: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static async Task<VaultVersion> MySQL_LoadAllAsync()
        {
            try
            {
                var result = new VaultVersion();
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    var loadQuery = $"SELECT * FROM `{MySql_TableName}`;";
                    var databases = await Dapper.SqlMapper.QueryAsync(connection, loadQuery);
                    var all = databases.Cast<IDictionary<string, object>>();
                    result = new VaultVersion
                    {
                        DatabaseVersion = Convert.ToUInt32(all.First()["Id"])
                    };
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager MySQL_LoadAllAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new VaultVersion();
            }
        }

        internal static async Task SetAsync(VaultVersion vaultVersion)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        Json_Collection = vaultVersion;
                        await Json_DataStore.SaveAsync(Json_Collection);
                        break;
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var existing = await Dapper.SqlMapper.ExecuteScalarAsync<bool>(connection,
                                $"SELECT COUNT(DISTINCT 1) FROM `{MySql_TableName}`;");
                            if (existing)
                            {
                                var parameter = new Dapper.DynamicParameters();
                                parameter.Add("@DatabaseVersion", vaultVersion.DatabaseVersion, DbType.UInt32,
                                    ParameterDirection.Input);
                                var updateQuery =
                                    $"UPDATE `{MySql_TableName}` SET `DatabaseVersion` = @DatabaseVersion;";
                                await Dapper.SqlMapper.ExecuteAsync(connection, updateQuery, parameter);
                            }
                            else
                            {
                                var insertQuery =
                                    $"INSERT INTO `{MySql_TableName}` (`DatabaseVersion`) " +
                                    "VALUES(@DatabaseVersion); ";
                                var parameter = new Dapper.DynamicParameters();
                                parameter.Add("@DatabaseVersion", vaultVersion.DatabaseVersion, DbType.UInt32,
                                    ParameterDirection.Input);
                                await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                            }
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager AddAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static VaultVersion Get()
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        return Json_Collection;
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var query = $"SELECT * FROM `{MySql_TableName}`;";
                            var databases = Dapper.SqlMapper.Query(connection, query)
                                .Cast<IDictionary<string, object>>();
                            var database = databases.FirstOrDefault();
                            return new VaultVersion
                            {
                                DatabaseVersion = Convert.ToUInt32(database?["DatabaseVersion"]),
                            };
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultVersionManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        internal static async Task MigrateAsync(EDatabase from, EDatabase to)
        {
            try
            {
                switch (from)
                {
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<VaultVersion>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload(true);
                        switch (to)
                        {
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                       new MySql.Data.MySqlClient.MySqlConnection(
                                           DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, deleteQuery);

                                    var parameter = new Dapper.DynamicParameters();
                                    parameter.Add("@DatabaseVersion", MigrateCollection.DatabaseVersion, DbType.UInt32,
                                        ParameterDirection.Input);
                                    var insertQuery =
                                        $"INSERT INTO `{MySql_TableName}` (`DatabaseVersion`) " +
                                        "VALUES(@DatabaseVersion);";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.MYSQL:
                        MigrateCollection = await MySQL_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.JSON:
                                Json_DataStore = new JsonDataStore<VaultVersion>(Plugin.Inst.Directory, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(from), from, null);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager MigrateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }
    }
}