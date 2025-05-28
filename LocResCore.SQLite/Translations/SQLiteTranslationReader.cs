using LocResCore.SQLite.Utilities;
using LocResCore.Translations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace LocResCore.SQLite.Translations
{
    public class SQLiteTranslationReader : ITranslationReader
    {
        public string[] Extensions { get => new string[] { ".db", ".db3", ".sqlite", ".sqlite3" }; }

        public void Read(out TranslationPool pool, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (SQLiteConnection connection = CreateConnection(filePath))
            {
                connection.Open();
                Read(out pool, connection);
                connection.Close();
            }
        }

        public void Read(out TranslationItem item, string filePath, string culture)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (SQLiteConnection connection = CreateConnection(filePath))
            {
                connection.Open();
                Read(out item, connection, culture);
                connection.Close();
            }
        }

        private void Read(out TranslationPool pool, SQLiteConnection connection)
        {
            pool = new TranslationPool();
            bool checkSchema = CheckSchema(connection);
            if (checkSchema)
            {
                var cultureDic = ReadCultures(connection);
                foreach (var cultureRec in cultureDic)
                {
                    Read(out TranslationItem item, connection, cultureRec.Value);
                    pool.Add(item);
                }
            }
        }

        private void Read(out TranslationItem item, SQLiteConnection connection, string culture)
        {
            item = new TranslationItem(culture);
            bool checkSchema = CheckSchema(connection);
            if (checkSchema)
            {
                string sql = @"
                    SELECT C.CODE AS CULTURE,
                           N.CODE AS NAMESPACE,
                           K.CODE AS KEY,
                           K.HASH AS HASH,
                           V.ORIGINAL AS ORIGINAL,
                           V.VALUE AS VALUE
                    FROM   LOC_VALUE V
                    JOIN   LOC_CULTURE C ON V.CULTURE_ID = C.ID
                    JOIN   LOC_KEY K ON V.KEY_ID = K.ID
                    JOIN   LOC_NAMESPACE N ON K.NAMESPACE_ID = N.ID
                    WHERE  C.CODE = @Code
                    ORDER BY K.ID ASC";

                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@Code", culture);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string nameSpace = reader["NAMESPACE"].ConvertToString();
                    string key = reader["KEY"].ConvertToString();
                    uint hash = reader["HASH"].ConvertToUInt32();
                    string originalValue = reader["ORIGINAL"].ConvertToString();
                    string value = reader["VALUE"].ConvertToString();
                    item.Add(new TranslationValue(nameSpace, key, hash, originalValue, value));
                }
            }
        }

        private Dictionary<int, string> ReadCultures(SQLiteConnection connection)
        {
            Dictionary<int, string> cultureDic = new Dictionary<int, string>();
            try
            {
                string sql = "SELECT ID, CODE FROM LOC_CULTURE ORDER BY ID ASC";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader["ID"].ConvertToInt32();
                    string code = reader["CODE"].ConvertToString();
                    cultureDic[id] = code;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception during query cultures!", ex);
                throw;
            }
            return cultureDic;
        }

        private SQLiteConnection CreateConnection(string filePath)
        {
            string connectionString = $"Data Source={filePath};Version=3;";
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            return connection;
        }

        private bool CheckSchema(SQLiteConnection connection)
        {
            bool check = false;
            try
            {
                string sql = "SELECT ID, CODE FROM DBA_VERSION ORDER BY ID DESC LIMIT 1";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string code = reader["CODE"].ConvertToString();
                    check = string.Equals(code, "1.0.0");
                    Log.Information($"DB Schema Version is: {code}");
                    if (!check)
                        Log.Error($"DB Schema Version should be 1.0.0, found {code} instead!");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception during DB Schema Version check!", ex);
                throw;
            }
            return check;
        }
    }
}
