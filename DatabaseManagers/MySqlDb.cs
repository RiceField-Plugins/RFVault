using System;
using System.Collections.Generic;
using System.Linq;
using I18N.West;
using MySql.Data.MySqlClient;
using RFLocker.Models;
using Rocket.Core.Logging;

namespace RFLocker.DatabaseManagers
{
    public class MySqlDb
    {
        // internal const string CreateTableQuery = 
        //     "`EntryID` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT, " + 
        //     "`SteamID` VARCHAR(32) NOT NULL DEFAULT '0', " + 
        //     "`LockerName` VARCHAR(32) NOT NULL DEFAULT 'Default', " + 
        //     "`Info` TEXT NOT NULL, " + 
        //     "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + 
        //     "PRIMARY KEY (EntryID)";
        internal const string CreateTableQuery = 
            "`EntryID` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT, " + 
            "`SteamID` VARCHAR(32) NOT NULL DEFAULT '0', " + 
            "`LockerName` VARCHAR(32) NOT NULL DEFAULT 'Default', " + 
            "`Info` BLOB NOT NULL, " + 
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + 
            "PRIMARY KEY (EntryID)";
        public string Address;
        public string Name;
        public string Password;
        public uint Port;
        public string TableName;
        public string Username;
        
        // CONSTRUCTOR   
        public MySqlDb(string address, uint port, string username, string password, string name, string tableName, 
            string createTableQuery)
        {
            Address = address;
            Port = port;
            Username = username;
            Password = password;
            Name = name;
            TableName = tableName;
            
            var cp1250 = new CP1250();
            CreateTableSchema(createTableQuery);
        }

        // METHODS
        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (Port == 0)
                    Port = 3306;
                connection = new MySqlConnection(
                    $"SERVER={Address};DATABASE={Name};UID={Username};PASSWORD={Password};PORT={Port};");
            }
            catch (Exception ex)
            {
                Logger.LogError("[RFLocker] DbError: " + ex);
            }

            return connection;
        }
        private void CreateTableSchema(string createTableQuery)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"CREATE TABLE IF NOT EXISTS `{TableName}` ({createTableQuery});");
        }
        public object ExecuteQuery(EQueryType queryType, string query, params MySqlParameter[] parameters)
        {
            object result = null;
            MySqlDataReader reader = null;

            using (var connection = CreateConnection())
            {
                try
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    foreach (var parameter in parameters)
                        command.Parameters.Add(parameter);

                    connection.Open();
                    switch (queryType)
                    {
                        case EQueryType.Reader:
                            var readerResult = new List<Row>();

                            reader = command.ExecuteReader();
                            while (reader.Read())
                                try
                                {
                                    var values = new Dictionary<string, object>();

                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        var columnName = reader.GetName(i);
                                        values.Add(columnName, reader[columnName]);
                                    }

                                    readerResult.Add(new Row { Values = values });
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(
                                        $"[RFLocker] DbError: The following query threw an error during reader execution:\nQuery: \"{query}\"\nError: {ex.Message}");
                                }

                            result = readerResult;
                            break;
                        case EQueryType.Scalar:
                            result = command.ExecuteScalar();
                            break;
                        case EQueryType.NonQuery:
                            result = command.ExecuteNonQuery();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("[RFLocker] DbError: " + ex);
                }
                finally
                {
                    reader?.Close();
                    connection.Close();
                }
            }

            return result;
        }
        public bool IsDataExist(string tableName, string data, string column)
        {
            var scalar = ExecuteQuery(EQueryType.Scalar,
                $"SELECT * FROM `{tableName}` WHERE {column} = @data;",
                new MySqlParameter("@data", data));
            return scalar != null;
        }
        public void DeleteData(string tableName, string data, string column)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"DELETE FROM `{tableName}` WHERE {column}=@data;", 
                new MySqlParameter("@data", data));
        }
        public void DeleteLocker(ulong entryID)
        {
            ExecuteQuery(EQueryType.NonQuery, $"DELETE FROM `{TableName}` WHERE EntryID=@entryID;", 
                new MySqlParameter("@entryID", entryID));
        }
        public object GetData(string tableName, string data, string column, string selectedColumn)
        {
            var result = ExecuteQuery(EQueryType.Scalar,
                $"SELECT {selectedColumn} FROM `{tableName}` WHERE {column} = @data;",
                new MySqlParameter("@data", data));

            return result;
        }
        public uint GetLockerCount(string steamID)
        {
            var result = ExecuteQuery(EQueryType.Scalar,
                $"SELECT COUNT(*) FROM `{TableName}` WHERE SteamID = @steamID;",
                new MySqlParameter("@steamID", steamID));
            uint.TryParse(result.ToString(), out var count);
            return count;
        }
        public bool HasLocker(string steamID)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID;",
                new MySqlParameter("@steamID", steamID));

            return readerResult.Count > 0;
        }
        public bool HasLocker(string steamID, string lockerName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND LockerName = @lockerName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@lockerName", lockerName));

            return readerResult.Count != 0;
        }
        public bool IsLockerExist(string steamID, string lockerName)
        {
            var scalar = ExecuteQuery(EQueryType.Scalar,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND LockerName = @lockerName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@lockerName", lockerName));

            return scalar != null;
        }
        public void InsertLocker(string steamID, string lockerName, byte[] info)//string info)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"INSERT INTO `{TableName}` (SteamID,LockerName,Info) VALUES(@steamID,@lockerName,@info);", 
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@lockerName", lockerName),
                new MySqlParameter("@info", info));
        }
        public object ReadData(string tableName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader, $"SELECT * FROM `{tableName}`;");

            return readerResult;
        }
        public PlayerSerializableLocker ReadLocker(string steamID, string lockerName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND LockerName = @lockerName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@lockerName", lockerName));

            return readerResult?.Select(r => new PlayerSerializableLocker
            {
                EntryID = ulong.Parse(r.Values["EntryID"].ToString()),
                SteamID = ulong.Parse(r.Values["SteamID"].ToString()),
                LockerName = r.Values["LockerName"].ToString(),
                Info = r.Values["Info"] as byte[],
            }).FirstOrDefault();
        }
        public IEnumerable<PlayerSerializableLocker> ReadLocker(string steamID)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID;",
                new MySqlParameter("@steamID", steamID));

            return readerResult?.Select(r => new PlayerSerializableLocker
            {
                EntryID = ulong.Parse(r.Values["EntryID"].ToString()),
                SteamID = ulong.Parse(r.Values["SteamID"].ToString()),
                LockerName = r.Values["LockerName"].ToString(),
                Info = r.Values["Info"] as byte[],
            });
        }
        public void UpdateData(string tableName, string oldData, string data, string column)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"UPDATE `{tableName}` SET {oldData}=@newData WHERE {column}=@{oldData};", 
                new MySqlParameter("@newData", data));
        }
        public void UpdateLocker(string steamID, string lockerName, byte[] info)//string info)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"UPDATE `{TableName}` SET Info = @info WHERE SteamID = @steamID AND LockerName = @lockerName;", 
                new MySqlParameter("@steamID", steamID),new MySqlParameter("@lockerName", lockerName),
                new MySqlParameter("@info", info));
        }
    }

    public enum EQueryType
    {
        NonQuery,
        Reader,
        Scalar,
    }

    public class Row
    {
        public Dictionary<string, object> Values;
    }
}