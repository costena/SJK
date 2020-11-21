using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SJK
{
    public class Session
    {
        private MySqlConnection connection;
        private bool shortConnection = false;
        #region public methods
        public Session(bool shortConnection = false)
        {
            this.shortConnection = shortConnection;
        }
        public void Connect(string connectionString)
        {
            connection?.Close();
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }
        public async Task ConnectAsync(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
        }

        public void Disconnect()
        {
            connection.Close();
        }

        private void CheckConnect()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }
        private async Task CheckConnectAsync()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }
        
        public void Insert<T>(T record)
        {
            CheckConnect();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var keyValues = GetKeyValues(record);
            List<string> keys = new List<string>();
            List<string> values = new List<string>();
            FieldInfo autoValue = null;
            foreach(var keyValue in keyValues)
            {
                if (keyValue.Key is PrimaryKeyAttribute && ((PrimaryKeyAttribute)keyValue.Key).AutoIncrement)
                {
                    autoValue = keyValue.Value;
                }
                else
                {
                    keys.Add($"`{keyValue.Key.KeyName}`");
                    values.Add($"'{keyValue.Value.GetValue(record)}'");
                }
            }
            MySqlCommand sql = new MySqlCommand($"INSERT INTO `{table.TableName}`({string.Join(", ", keys)}) VALUES({string.Join(", ", values)})", connection);
            sql.ExecuteNonQuery();
            if (autoValue != null)
            {
                sql = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
                MySqlDataReader reader = sql.ExecuteReader();
                if (reader.Read())
                {
                    autoValue.SetValue(record, reader[0]);
                }
                reader.Close();
            }
        }
        public async Task InsertAsync<T>(T record)
        {
            await CheckConnectAsync();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var keyValues = GetKeyValues(record);
            List<string> keys = new List<string>();
            List<string> values = new List<string>();
            FieldInfo autoValue = null;
            foreach (var keyValue in keyValues)
            {
                if (keyValue.Key is PrimaryKeyAttribute && ((PrimaryKeyAttribute)keyValue.Key).AutoIncrement)
                {
                    autoValue = keyValue.Value;
                }
                else
                {
                    keys.Add($"`{keyValue.Key.KeyName}`");
                    values.Add($"'{keyValue.Value.GetValue(record)}'");
                }
            }
            MySqlCommand sql = new MySqlCommand($"INSERT INTO `{table.TableName}`({string.Join(", ", keys)}) VALUES({string.Join(", ", values)})", connection);
            await sql.ExecuteNonQueryAsync();
            if (autoValue != null)
            {
                sql = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
                MySqlDataReader reader = sql.ExecuteReader();
                if (reader.Read())
                {
                    autoValue.SetValue(record, reader[0]);
                }
                reader.Close();
            }
        }

        public int Delete<T>(T record)
        {
            CheckConnect();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var primaryKeyValues = GetPrimaryKeyValues(record);
            List<string> conditions = new List<string>();
            foreach(var primaryKeyValue in primaryKeyValues)
            {
                conditions.Add($"`{primaryKeyValue.Key.KeyName}` = '{primaryKeyValue.Value.GetValue(record)}'");
            }
            MySqlCommand sql = new MySqlCommand($"DELETE FROM `{table.TableName}` WHERE {string.Join(" AND ", conditions)}", connection);
            return sql.ExecuteNonQuery();
        }
        public async Task<int> DeleteAsync<T>(T record)
        {
            await CheckConnectAsync();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var primaryKeyValues = GetPrimaryKeyValues(record);
            List<string> conditions = new List<string>();
            foreach (var primaryKeyValue in primaryKeyValues)
            {
                conditions.Add($"{primaryKeyValue.Key.KeyName} = {primaryKeyValue.Value.GetValue(record)}");
            }
            MySqlCommand sql = new MySqlCommand($"DELETE FROM `{table.TableName}` WHERE {string.Join(" AND ", conditions)}", connection);
            return await sql.ExecuteNonQueryAsync();
        }

        public int Update<T>(T record)
        {
            CheckConnect();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var keyValues = GetKeyValues(record);
            List<string> sets = new List<string>();
            List<string> conditions = new List<string>();
            foreach(var keyValue in keyValues)
            {
                if(keyValue.Key is PrimaryKeyAttribute)
                {
                    conditions.Add($"`{keyValue.Key.KeyName}` = '{keyValue.Value.GetValue(record)}'");
                }
                else
                {
                    sets.Add($"`{keyValue.Key.KeyName}` = '{keyValue.Value.GetValue(record)}'");
                }
            }
            MySqlCommand sql = new MySqlCommand($"UPDATE `{table.TableName}` SET {string.Join(", ", sets)} WHERE {string.Join(" AND ", conditions)}", connection);
            return sql.ExecuteNonQuery();
        }
        public async Task<int> UpdateAsync<T>(T record)
        {
            await CheckConnectAsync();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            var keyValues = GetKeyValues(record);
            List<string> sets = new List<string>();
            List<string> conditions = new List<string>();
            foreach (var keyValue in keyValues)
            {
                if (keyValue.Key is PrimaryKeyAttribute)
                {
                    conditions.Add($"{keyValue.Key.KeyName} =  {keyValue.Value.GetValue(record)}");
                }
                else
                {
                    sets.Add($"{keyValue.Key.KeyName} = {keyValue.Value.GetValue(record)}");
                }
            }
            MySqlCommand sql = new MySqlCommand($"UPDATE `{table.TableName}` SET {string.Join(", ", sets)} WHERE {string.Join(" AND ", conditions)}", connection);
            return await sql.ExecuteNonQueryAsync();
        }

        public T[] Find<T>(params Condition[] conditions)
        {
            CheckConnect();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            string[] conditionStrings = new string[conditions.Length];
            MySqlCommand sql;
            if (conditionStrings.Length > 0)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    conditionStrings[i] = conditions[i].ToString();
                }
                sql = new MySqlCommand($"SELECT * FROM `{table.TableName}` WHERE {string.Join(" AND ", conditionStrings)}", connection);
            }
            else
            {
                sql = new MySqlCommand($"SELECT * FROM `{table.TableName}`", connection);
            }
            MySqlDataReader reader = sql.ExecuteReader();
            try
            {
                List<T> ts = new List<T>();
                while (reader.Read())
                {
                    T t = Activator.CreateInstance<T>();
                    var keyValues = GetKeyValues(t);
                    foreach (var keyValue in keyValues)
                    {
                        keyValue.Value.SetValue(t, reader[keyValue.Key.KeyName]);
                    }
                    ts.Add(t);
                }
                return ts.ToArray();
            }
            finally
            {
                reader.Close();
            }
        }
        public async Task<List<T>> FindAsync<T>(params Condition[] conditions)
        {
            await CheckConnectAsync();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            string[] conditionStrings = new string[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
            {
                conditionStrings[i] = conditions[i].ToString();
            }
            MySqlCommand sql = new MySqlCommand($"SELECT * FROM `{table.TableName}` WHERE {string.Join(" AND ", conditionStrings)}", connection);
            DbDataReader reader = await sql.ExecuteReaderAsync();
            List<T> result = new List<T>();
            while (reader.Read())
            {
                T t = Activator.CreateInstance<T>();
                var keyValues = GetKeyValues(t);
                foreach (var keyValue in keyValues)
                {
                    keyValue.Value.SetValue(t, reader[keyValue.Key.KeyName]);
                }
                result.Add(t);
            }
            reader.Close();
            return result;
        }

        public T FindFirst<T>(params Condition[] conditions) where T : class
        {
            CheckConnect();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            string[] conditionStrings = new string[conditions.Length];
            MySqlCommand sql = null;
            if (conditions.Length > 0)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    conditionStrings[i] = conditions[i].ToString();
                }
                sql = new MySqlCommand($"SELECT * FROM `{table.TableName}` WHERE {string.Join(" AND ", conditionStrings)} LIMIT 1", connection);
            }
            else
            {
                sql = new MySqlCommand($"SELECT * FROM `{table.TableName}` LIMIT 1", connection);
            }
            MySqlDataReader reader = sql.ExecuteReader();
            T result = Activator.CreateInstance<T>();
            if (reader.Read())
            {
                var keyValues = GetKeyValues(result);
                foreach (var keyValue in keyValues)
                {
                    keyValue.Value.SetValue(result, reader[keyValue.Key.KeyName]);
                }
                reader.Close();
                return result;
            }
            reader.Close();
            return null;
        }
        public async Task<T> FindFirstAsync<T>(params Condition[] conditions) where T : class
        {
            await CheckConnectAsync();
            TableAttribute table = typeof(T).GetCustomAttribute<TableAttribute>();
            string[] conditionStrings = new string[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
            {
                conditionStrings[i] = conditions[i].ToString();
            }
            MySqlCommand sql = new MySqlCommand($"SELECT * FROM `{table.TableName}` WHERE {string.Join(" AND ", conditionStrings)}", connection);
            MySqlDataReader reader = (MySqlDataReader)await sql.ExecuteReaderAsync();
            T result = Activator.CreateInstance<T>();
            if (reader.Read())
            {
                var keyValues = GetKeyValues(result);
                foreach (var keyValue in keyValues)
                {
                    keyValue.Value.SetValue(result, reader[keyValue.Key.KeyName]);
                }
                reader.Close();
                return result;
            }
            reader.Close();
            return null;
        }
        #endregion
        private Dictionary<KeyAttribute, FieldInfo> GetKeyValues(object record)
        {
            Dictionary<KeyAttribute, FieldInfo> result = new Dictionary<KeyAttribute, FieldInfo>();
            foreach(FieldInfo fieldInfo in record.GetType().GetFields())
            {
                KeyAttribute key = fieldInfo.GetCustomAttribute<KeyAttribute>();
                if (key != null)
                {
                    result.Add(key, fieldInfo);
                }
            }
            return result;
        }
        private Dictionary<PrimaryKeyAttribute, FieldInfo> GetPrimaryKeyValues(object record)
        {
            Dictionary<PrimaryKeyAttribute, FieldInfo> result = new Dictionary<PrimaryKeyAttribute, FieldInfo>();
            foreach (FieldInfo fieldInfo in record.GetType().GetFields())
            {
                PrimaryKeyAttribute key = fieldInfo.GetCustomAttribute<PrimaryKeyAttribute>();
                if (key != null)
                {
                    result.Add(key, fieldInfo);
                }
            }
            return result;
        }
    }
}
