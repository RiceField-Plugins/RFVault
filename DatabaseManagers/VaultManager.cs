using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dapper;
using LiteDB.Async;
using RFVault.API.Interfaces;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Core.Logging;
#if DEBUG
using Extensions = RFLocker.Utils.Extensions;
#endif

namespace RFVault.DatabaseManagers
{
    internal class VaultManager : IVaultManager
    {
        internal static bool Ready { get; set; }
        internal List<PlayerVault> Collection { get; set; } = new List<PlayerVault>();

        private static readonly string LiteDB_TableName = "vault";

        private static readonly string Json_FileName = "vault.json";
        private DataStore<List<PlayerVault>> Json_DataStore { get; set; }

        private static readonly string MySql_TableName = "rfvault";

        private static readonly string MySql_CreateTableQuery =
            "`Id` INT NOT NULL AUTO_INCREMENT, " +
            "`SteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`VaultName` VARCHAR(255) NOT NULL DEFAULT 'N/A', " +
            "`VaultContent` TEXT NOT NULL, " +
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
            "PRIMARY KEY (Id)";

        internal VaultManager()
        {
            UniTask.RunOnThreadPool(async () =>
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        await LiteDB_ReloadAsync();
                        break;
                    case EDatabase.JSON:
                        Json_DataStore = new DataStore<List<PlayerVault>>(Plugin.Inst.Directory, Json_FileName);
                        await JSON_ReloadAsync();
                        break;
                    case EDatabase.MYSQL:
                        // new CP1250();
                        await MySQL_ReloadAsync();
                        break;
                }

                Ready = true;
            }).Forget(exception => Logger.LogError("[RFVault] [ERROR] VaultManager Initializing: " + exception));
        }

        async UniTask<int> IVaultManager.AddAsync(ulong steamId, Vault vault)
        {
            return await AddAsync(steamId, vault);
        }

        PlayerVault IVaultManager.Get(ulong steamId, Vault vault)
        {
            return Get(steamId, vault);
        }

        async UniTask<bool> IVaultManager.UpdateAsync(ulong steamId, Vault vault)
        {
            return await UpdateAsync(steamId, vault);
        }

        async UniTask IVaultManager.MigrateAsync(EDatabase from, EDatabase to)
        {
            await MigrateAsync(from, to);
        }

#if DEBUG
        async UniTask IVaultManager.MigrateLockerAsync(EDatabase to)
        {
            await MigrateLockerAsync(to);
        }
#endif

        private int NewId()
        {
            try
            {
                var last = Collection.Max(x => x.Id);
                return last + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private async UniTask<List<PlayerVault>> LiteDB_LoadAsync()
        {
            var result = new List<PlayerVault>();
            using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
            {
                var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                var all = await col.FindAllAsync();
                result.AddRange(all);
            }

            return result;
        }

        private async UniTask LiteDB_ReloadAsync()
        {
            Collection = await LiteDB_LoadAsync();
            if (Collection != null)
                return;
            Collection = new List<PlayerVault>();
        }

        private async UniTask JSON_ReloadAsync()
        {
            Collection = await Json_DataStore.LoadAsync();
            if (Collection != null)
                return;
            Collection = new List<PlayerVault>();
            await Json_DataStore.SaveAsync(Collection);
        }

        private async UniTask MySQL_CreateTableAsync(string tableName, string createTableQuery)
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
            {
                await connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS `{tableName}` ({createTableQuery});");
            }
        }

        private async UniTask<List<PlayerVault>> MySQL_LoadAsync()
        {
            var result = new List<PlayerVault>();
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
            {
                var loadQuery = $"SELECT * FROM `{MySql_TableName}`;";
                var databases = await connection.QueryAsync(loadQuery);
                result.AddRange(from database in databases.Cast<IDictionary<string, object>>()
                    let byteArray = database["VaultContent"].ToString().ToByteArray()
                    let vaultContent = byteArray.Deserialize<ItemsWrapper>()
                    select new PlayerVault
                    {
                        Id = Convert.ToInt32(database["Id"]),
                        SteamId = Convert.ToUInt64(database["SteamId"]),
                        VaultName = database["VaultName"].ToString(),
                        VaultContent = vaultContent,
                        LastUpdated = Convert.ToDateTime(database["LastUpdated"]),
                    });
            }

            return result;
        }

        private async UniTask MySQL_ReloadAsync()
        {
            await MySQL_CreateTableAsync(MySql_TableName, MySql_CreateTableQuery);
            Collection = await MySQL_LoadAsync();
            if (Collection != null)
                return;
            Collection = new List<PlayerVault>();
        }

        private async UniTask<int> AddAsync(ulong steamId, Vault vault)
        {
            var existingVault = Collection.Find(x => x.SteamId == steamId && x.VaultName == vault.Name);
            if (existingVault != null)
                return -1;
            var playerVault = new PlayerVault
            {
                Id = NewId(),
                SteamId = steamId,
                VaultName = vault.Name,
                VaultContent = new ItemsWrapper
                {
                    Height = vault.Height,
                    Width = vault.Width,
                    Page = 7,
                    Items = new List<ItemJarWrapper>(),
                },
                LastUpdated = DateTime.Now
            };
            Collection.Add(playerVault);

            switch (Plugin.Conf.Database)
            {
                case EDatabase.LITEDB:
                    using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                    {
                        var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                        await col.InsertAsync(playerVault);
                        await col.EnsureIndexAsync(x => x.SteamId);
                    }

                    break;
                case EDatabase.JSON:
                    await Json_DataStore.SaveAsync(Collection);
                    break;
                case EDatabase.MYSQL:
                    using (var connection =
                        new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                    {
                        var serialized = playerVault.VaultContent.Serialize();
                        var vaultContent = serialized.ToBase64();
                        var insertQuery =
                            $"INSERT INTO `{MySql_TableName}` (Id, SteamId, VaultName, VaultContent) " +
                            "VALUES(@Id, @SteamId, @VaultName, @VaultContent)";
                        var parameter = new DynamicParameters();
                        parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                        parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                        parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                        await connection.ExecuteAsync(insertQuery, parameter);
                    }

                    break;
            }

            return playerVault.Id;
        }

        private PlayerVault Get(ulong steamId, Vault vault)
        {
            return Collection.Find(x => x.SteamId == steamId && x.VaultName == vault.Name);
        }

        private async UniTask<bool> UpdateAsync(ulong steamId, Vault vault)
        {
            var index = Collection.FindIndex(x => x.SteamId == steamId && x.VaultName == vault.Name);
            if (index == -1)
                return false;
            var playerVault = Collection[index];
            switch (Plugin.Conf.Database)
            {
                case EDatabase.LITEDB:
                    using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                    {
                        var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                        return await col.UpdateAsync(playerVault);
                    }
                case EDatabase.JSON:
                    return await Json_DataStore.SaveAsync(Collection);
                case EDatabase.MYSQL:
                    int result;
                    using (var connection =
                        new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                    {
                        var serialized = playerVault.VaultContent.Serialize();
                        var vaultContent = serialized.ToBase64();
                        var parameter = new DynamicParameters();
                        parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                        parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                        var updateQuery =
                            $"UPDATE {MySql_TableName} SET VaultContent = @VaultContent WHERE SteamId = @SteamId AND VaultName = @VaultName;";
                        result = await connection.ExecuteAsync(updateQuery, parameter);
                    }

                    return result == 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTask MigrateAsync(EDatabase from, EDatabase to)
        {
            switch (from)
            {
                case EDatabase.LITEDB:
                    await LiteDB_ReloadAsync();
                    switch (to)
                    {
                        case EDatabase.JSON:
                            await Json_DataStore.SaveAsync(Collection);
                            break;
                        case EDatabase.MYSQL:
                            using (var connection =
                                new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                            {
                                var deleteQuery = $"DELETE FROM {MySql_TableName};";
                                await connection.ExecuteAsync(deleteQuery);

                                foreach (var playerVault in Collection)
                                {
                                    var serialized = playerVault.VaultContent.Serialize();
                                    var vaultContent = serialized.ToBase64();
                                    var parameter = new DynamicParameters();
                                    parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                    parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                        ParameterDirection.Input);
                                    parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                        ParameterDirection.Input);
                                    parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                        ParameterDirection.Input);
                                    var insertQuery =
                                        $"INSERT INTO `{MySql_TableName}` (Id, SteamId, VaultName, VaultContent) " +
                                        "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                    await connection.ExecuteAsync(insertQuery, parameter);
                                }
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(to), to, null);
                    }

                    break;
                case EDatabase.JSON:
                    await JSON_ReloadAsync();
                    switch (to)
                    {
                        case EDatabase.LITEDB:
                            using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                            {
                                var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                await col.DeleteAllAsync();
                                await col.InsertBulkAsync(Collection);
                            }

                            break;
                        case EDatabase.MYSQL:
                            using (var connection =
                                new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                            {
                                var deleteQuery = $"DELETE FROM {MySql_TableName};";
                                await connection.ExecuteAsync(deleteQuery);

                                foreach (var playerVault in Collection)
                                {
                                    var serialized = playerVault.VaultContent.Serialize();
                                    var vaultContent = serialized.ToBase64();
                                    var parameter = new DynamicParameters();
                                    parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                    parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                        ParameterDirection.Input);
                                    parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                        ParameterDirection.Input);
                                    parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                        ParameterDirection.Input);
                                    var insertQuery =
                                        $"INSERT INTO `{MySql_TableName}` (Id, SteamId, VaultName, VaultContent) " +
                                        "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                    await connection.ExecuteAsync(insertQuery, parameter);
                                }
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(to), to, null);
                    }

                    break;
                case EDatabase.MYSQL:
                    await MySQL_ReloadAsync();
                    switch (to)
                    {
                        case EDatabase.LITEDB:
                            using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                            {
                                var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                await col.DeleteAllAsync();
                                await col.InsertBulkAsync(Collection);
                            }

                            break;
                        case EDatabase.JSON:
                            await Json_DataStore.SaveAsync(Collection);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(to), to, null);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(from), from, null);
            }
        }
#if DEBUG
        private List<PlayerVault> MySQLLocker_LoadAsync()
        {
            var result = new List<PlayerVault>();
            foreach (var playerSerializableLocker in RFLocker.Plugin.Collection)
            {
                var locker = Extensions.ToSerializableVirtualLocker(playerSerializableLocker.Info);
                var itemJarWrappers = new List<ItemJarWrapper>();
                foreach (var lockerItem in locker.Items)
                    itemJarWrappers.Add(ItemJarWrapper.Create(lockerItem.ToItemJar()));
                var lockerModel = RFLocker.Models.LockerModel.Parse(playerSerializableLocker.LockerName);
                var itemsWrapper = new ItemsWrapper(7, lockerModel.Height, lockerModel.Width, itemJarWrappers);
                var playerVault = new PlayerVault
                {
                    Id = (int) playerSerializableLocker.EntryID,
                    SteamId = playerSerializableLocker.SteamID,
                    VaultName = playerSerializableLocker.LockerName,
                    LastUpdated = DateTime.Now,
                    VaultContent = itemsWrapper
                };
                result.Add(playerVault);
            }

            return result;
        }

        private async UniTask MySQLLocker_ReloadAsync()
        {
            await MySQL_CreateTableAsync(MySql_TableName, MySql_CreateTableQuery);
            RFLocker.Plugin.DbManager.ReadLocker();
            Collection = MySQLLocker_LoadAsync();
            if (Collection != null)
                return;
            Collection = new List<PlayerVault>();
        }

        private async UniTask MigrateLockerAsync(EDatabase to)
        {
            await MySQLLocker_ReloadAsync();
            switch (to)
            {
                case EDatabase.LITEDB:
                    using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                    {
                        var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                        await col.DeleteAllAsync();
                        await col.InsertBulkAsync(Collection);
                    }

                    break;
                case EDatabase.JSON:
                    await Json_DataStore.SaveAsync(Collection);
                    break;
                case EDatabase.MYSQL:
                    using (var connection =
                        new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                    {
                        var deleteQuery = $"DELETE FROM {MySql_TableName};";
                        await connection.ExecuteAsync(deleteQuery);

                        foreach (var playerVault in Collection)
                        {
                            var serialized = playerVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();
                            var parameter = new DynamicParameters();
                            parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                            parameter.Add("@SteamId", playerVault.SteamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                ParameterDirection.Input);
                            var insertQuery =
                                $"INSERT INTO `{MySql_TableName}` (Id, SteamId, VaultName, VaultContent) " +
                                "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                            await connection.ExecuteAsync(insertQuery, parameter);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(to), to, null);
            }
        }
#endif
    }
}