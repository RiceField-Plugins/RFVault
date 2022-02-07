using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Storages;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using Steamworks;
#if DEBUG
using Extensions = RFLocker.Utils.Extensions;
#endif

namespace RFVault.DatabaseManagers
{
    internal class VaultManager
    {
        internal static bool Ready { get; set; }
        private List<PlayerVault> Json_Collection { get; set; } = new List<PlayerVault>();
        private List<PlayerVault> MigrateCollection { get; set; } = new List<PlayerVault>();

        private const string LiteDB_TableName = "vault";

        private const string Json_FileName = "vault.json";
        private DataStore<List<PlayerVault>> Json_DataStore { get; set; }

        private string MySql_TableName => $"{DatabaseManager.MySql_TableName}";

        private const string MySql_CreateTableQuery =
            "`Id` INT NOT NULL AUTO_INCREMENT, " +
            "`SteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`VaultName` VARCHAR(255) NOT NULL DEFAULT 'N/A', " +
            "`VaultContent` TEXT NOT NULL, " +
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
            "PRIMARY KEY (Id)";

        internal VaultManager()
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        Json_DataStore = new DataStore<List<PlayerVault>>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload();
                        break;
                    case EDatabase.LITEDB:
                        LiteDB_Init();
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

        private void LiteDB_Init()
        {
            using (var db = new LiteDB.LiteDatabase(DatabaseManager.LiteDB_ConnectionString))
            {
                if (db.UserVersion == 0)
                {
                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                    col.EnsureIndex(x => x.Id);
                    col.EnsureIndex(x => x.SteamId);
                    col.EnsureIndex(x => x.VaultName);
                    col.EnsureIndex(x => x.LastUpdated);
                    
                    db.UserVersion = 1;
                }
            }
        }

        private int Json_NewId()
        {
            try
            {
                var last = Json_Collection.Max(x => x.Id);
                return last + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void JSON_Reload(bool migrate = false)
        {
            try
            {
                if (migrate)
                {
                    MigrateCollection = Json_DataStore.Load() ?? new List<PlayerVault>();
                    return;
                }

                Json_Collection = Json_DataStore.Load() ?? new List<PlayerVault>();
                Json_DataStore.Save(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager JSON_Reload: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private void MySQL_CreateTable(string tableName, string createTableQuery)
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

        private async Task<List<PlayerVault>> LiteDB_LoadAllAsync()
        {
            try
            {
                var result = new List<PlayerVault>();
                using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                {
                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                    var all = await col.FindAllAsync();
                    result.AddRange(all);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager LiteDB_LoadAllAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new List<PlayerVault>();
            }
        }

        private async Task<List<PlayerVault>> MySQL_LoadAllAsync()
        {
            try
            {
                var result = new List<PlayerVault>();
                using (var connection =
                    new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    var loadQuery = $"SELECT * FROM `{MySql_TableName}`;";
                    var databases = await Dapper.SqlMapper.QueryAsync(connection, loadQuery);
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
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager MySQL_LoadAllAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new List<PlayerVault>();
            }
        }

        internal async Task AddAsync(ulong steamId, Vault vault)
        {
            var playerVault = new PlayerVault
            {
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
            try
            {
                PlayerVault existingVault;
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            existingVault =
                                await col.FindOneAsync(x => x.SteamId == steamId && x.VaultName == vault.Name);
                            if (existingVault != null) 
                                return;
                            
                            await col.InsertAsync(playerVault);
                            await col.EnsureIndexAsync(x => x.SteamId);
                        }

                        break;
                    case EDatabase.JSON:
                        existingVault = Json_Collection.Find(x => x.SteamId == steamId && x.VaultName == vault.Name);
                        if (existingVault != null) 
                            return;
                        
                        playerVault.Id = Json_NewId();
                        Json_Collection.Add(playerVault);
                        await Json_DataStore.SaveAsync(Json_Collection);
                        break;
                    case EDatabase.MYSQL:
                        using (var connection =
                            new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var existing = await Dapper.SqlMapper.ExecuteScalarAsync<bool>(connection,
                                $"SELECT COUNT(DISTINCT 1) FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;",
                                new {SteamId = steamId, VaultName = vault.Name});
                            if (existing)
                                return;

                            var serialized = playerVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();
                            var insertQuery =
                                $"INSERT INTO `{MySql_TableName}` (`SteamId`, `VaultName`, `VaultContent`) " +
                                "VALUES(@SteamId, @VaultName, @VaultContent); SELECT last_insert_id();";
                            var parameter = new Dapper.DynamicParameters();
                            parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                            await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, insertQuery, parameter);
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

        internal PlayerVault Get(ulong steamId, Vault vault)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        return Json_Collection.Find(x => x.SteamId == steamId && x.VaultName == vault.Name);
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.LiteDatabase(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            return col.FindOne(x => x.SteamId == steamId && vault.Name == x.VaultName);
                        }
                    case EDatabase.MYSQL:
                        using (var connection =
                            new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var parameter = new Dapper.DynamicParameters();
                            parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", vault.Name, DbType.String, ParameterDirection.Input);
                            var query =
                                $"SELECT * FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;";
                            var databases = Dapper.SqlMapper.Query(connection, query, parameter)
                                .Cast<IDictionary<string, object>>();
                            var database = databases.First();
                            var byteArray = database["VaultContent"].ToString().ToByteArray();
                            var vaultContent = byteArray.Deserialize<ItemsWrapper>();
                            return new PlayerVault
                            {
                                Id = Convert.ToInt32(database["Id"]),
                                SteamId = Convert.ToUInt64(database["SteamId"]),
                                VaultName = database["VaultName"].ToString(),
                                VaultContent = vaultContent,
                                LastUpdated = Convert.ToDateTime(database["LastUpdated"]),
                            };
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        internal async Task<bool> UpdateAsync(ulong steamId, Vault vault)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        var pComponent = UnturnedPlayer.FromCSteamID(new CSteamID(steamId))
                            .GetComponent<PlayerComponent>();
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            return await col.UpdateAsync(pComponent.CachedVault);
                        }
                    case EDatabase.JSON:
                        var index = Json_Collection.FindIndex(x => x.SteamId == steamId && x.VaultName == vault.Name);
                        if (index == -1)
                            return false;
                        
                        return await Json_DataStore.SaveAsync(Json_Collection);
                    case EDatabase.MYSQL:
                        pComponent = UnturnedPlayer.FromCSteamID(new CSteamID(steamId)).GetComponent<PlayerComponent>();
                        int result;
                        using (var connection =
                            new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var serialized = pComponent.CachedVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();
                            var parameter = new Dapper.DynamicParameters();
                            parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", pComponent.CachedVault.VaultName, DbType.String,
                                ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                            var updateQuery =
                                $"UPDATE {MySql_TableName} SET `VaultContent` = @VaultContent WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;";
                            result = await Dapper.SqlMapper.ExecuteAsync(connection, updateQuery, parameter);
                        }

                        return result == 1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager UpdateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return false;
            }
        }
        
        internal async Task<bool> ClearAsync(ulong steamId, Vault vault)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            var playerVault = await col.FindOneAsync(x => x.SteamId == steamId && x.VaultName == vault.Name);
                            if (playerVault == null)
                                return false;

                            playerVault.VaultContent = new ItemsWrapper();
                            return await col.UpdateAsync(playerVault);
                        }
                    case EDatabase.JSON:
                        var index = Json_Collection.FindIndex(x => x.SteamId == steamId && x.VaultName == vault.Name);
                        if (index == -1)
                            return false;

                        Json_Collection[index].VaultContent = new ItemsWrapper();
                        return await Json_DataStore.SaveAsync(Json_Collection);
                    case EDatabase.MYSQL:
                        int result;
                        using (var connection =
                            new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var serialized = new ItemsWrapper().Serialize();
                            var vaultContent = serialized.ToBase64();
                            var parameter = new Dapper.DynamicParameters();
                            parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", vault.Name, DbType.String,
                                ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                            var updateQuery =
                                $"UPDATE {MySql_TableName} SET `VaultContent` = @VaultContent WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;";
                            result = await Dapper.SqlMapper.ExecuteAsync(connection, updateQuery, parameter);
                        }

                        return result == 1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager UpdateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return false;
            }
        }

        internal async Task MigrateAsync(EDatabase from, EDatabase to)
        {
            try
            {
                switch (from)
                {
                    case EDatabase.LITEDB:
                        MigrateCollection = await LiteDB_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.JSON:
                                Json_DataStore = new DataStore<List<PlayerVault>>(Plugin.Inst.Directory, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                    new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, deleteQuery);

                                    foreach (var playerVault in MigrateCollection)
                                    {
                                        var serialized = playerVault.VaultContent.Serialize();
                                        var vaultContent = serialized.ToBase64();
                                        var parameter = new Dapper.DynamicParameters();
                                        parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VaultName`, `VaultContent`) " +
                                            "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                        await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.JSON:
                        Json_DataStore = new DataStore<List<PlayerVault>>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload(true);
                        switch (to)
                        {
                            case EDatabase.LITEDB:
                                using (var db =
                                    new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                                {
                                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                    await col.DeleteAllAsync();
                                    await col.InsertBulkAsync(MigrateCollection);
                                }

                                break;
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                    new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, deleteQuery);

                                    foreach (var playerVault in MigrateCollection)
                                    {
                                        var serialized = playerVault.VaultContent.Serialize();
                                        var vaultContent = serialized.ToBase64();
                                        var parameter = new Dapper.DynamicParameters();
                                        parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VaultName`, `VaultContent`) " +
                                            "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                        await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                                    }
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
                            case EDatabase.LITEDB:
                                using (var db =
                                    new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                                {
                                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                    await col.DeleteAllAsync();
                                    await col.InsertBulkAsync(MigrateCollection);
                                }

                                break;
                            case EDatabase.JSON:
                                Json_DataStore = new DataStore<List<PlayerVault>>(Plugin.Inst.Directory, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(from), from, null);
                }

                MigrateCollection.Clear();
                MigrateCollection.TrimExcess();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultManager MigrateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
            MigrateCollection.Clear();
            MigrateCollection.TrimExcess();
        }
#if DEBUG
        private List<PlayerVault> MySQLLocker_LoadAsync()
        {
            var result = new List<PlayerVault>();
            foreach (var playerSerializableLocker in RFLocker.Plugin.Json_Collection)
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

        private async Task MySQLLocker_ReloadAsync()
        {
            await MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
            RFLocker.Plugin.DbManager.ReadLocker();
            Json_Collection = MySQLLocker_LoadAsync();
            if (Json_Collection != null)
                return;
            Json_Collection = new List<PlayerVault>();
        }

        private async Task MigrateLockerAsync(EDatabase to)
        {
            await MySQLLocker_ReloadAsync();
            switch (to)
            {
                case EDatabase.LITEDB:
                    using (var db = new LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                    {
                        var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                        await col.DeleteAllAsync();
                        await col.InsertBulkAsync(Json_Collection);
                    }

                    break;
                case EDatabase.JSON:
                    await Json_DataStore.SaveAsync(Json_Collection);
                    break;
                case EDatabase.MYSQL:
                    using (var connection =
                        new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                    {
                        var deleteQuery = $"DELETE FROM {MySql_TableName};";
                        await connection.ExecuteAsync(deleteQuery);

                        foreach (var playerVault in Json_Collection)
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