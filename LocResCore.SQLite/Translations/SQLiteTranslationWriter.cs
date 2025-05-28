using LocResCore.SQLite.Utilities;
using LocResCore.Translations;
using Serilog;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Transactions;

namespace LocResCore.SQLite.Translations
{
    public class SQLiteTranslationWriter : ITranslationWriter
    {
        public string[] Extensions { get => new string[] { ".db", ".db3", ".sqlite", ".sqlite3" }; }

        public void Write(string filePath, TranslationPool pool)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            using (TransactionScope transaction = new TransactionScope())
            {
                using (SQLiteConnection connection = CreateConnection(filePath))
                {
                    connection.Open();
                    CreateSchema(connection);
                    Write(connection, pool);
                    connection.Close();
                }
                transaction.Complete();
            }
        }

        public void Write(string filePath, TranslationItem item)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using (TransactionScope transaction = new TransactionScope())
            {
                using (SQLiteConnection connection = CreateConnection(filePath))
                {
                    connection.Open();
                    CreateSchema(connection);
                    Write(connection, item);
                    connection.Close();
                }
                transaction.Complete();
            }
        }

        private void Write(SQLiteConnection connection, TranslationPool pool)
        {
            foreach (TranslationItem item in pool)
            {
                Write(connection, item);
            }
            Log.Information($"Wrote [{pool.Count()}] cultures.");
        }

        private void Write(SQLiteConnection connection, TranslationItem item)
        {
            int cultureId = UpsertCulture(connection, item);
            DeleteValue(connection, cultureId);
            foreach (TranslationValue value in item)
            {
                InsertValue(connection, value, cultureId);
            }
            Log.Information($"Culture [{item.Culture}]. Wrote [{item.Count}/{item.Count}] resources.");
        }

        private int UpsertCulture(SQLiteConnection connection, TranslationItem item)
        {
            string sql = $"SELECT ID FROM LOC_CULTURE WHERE CODE = @Code";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@Code", item.Culture);
            int id = command.ExecuteScalar().ConvertToInt32();
            if (id == 0)
            {
                sql = $"SELECT MAX(ID) FROM LOC_CULTURE";
                command = new SQLiteCommand(sql, connection);
                id = command.ExecuteScalar().ConvertToInt32();
                id++;

                sql = $"INSERT INTO LOC_CULTURE (ID, CODE) VALUES (@Id, @Code)";
                command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Code", item.Culture);
                command.ExecuteNonQuery();
            }
            return id;
        }

        private int UpsertNamespace(SQLiteConnection connection, TranslationValue value)
        {
            string sql = "SELECT ID FROM LOC_NAMESPACE WHERE CODE = @Code";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@Code", value.NameSpace);
            int id = command.ExecuteScalar().ConvertToInt32();
            if (id == 0)
            {
                sql = "SELECT MAX(ID) FROM LOC_NAMESPACE";
                command = new SQLiteCommand(sql, connection);
                id = command.ExecuteScalar().ConvertToInt32();
                id++;

                sql = "INSERT INTO LOC_NAMESPACE (ID, CODE) VALUES (@Id, @Code)";
                command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Code", value.NameSpace);
                command.ExecuteNonQuery();
            }
            return id;
        }

        private int UpsertKey(SQLiteConnection connection, TranslationValue value)
        {
            string sql = "SELECT ID FROM LOC_KEY WHERE CODE = @Code";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@Code", value.Key);
            int id = command.ExecuteScalar().ConvertToInt32();
            if (id == 0)
            {
                sql = "SELECT MAX(ID) FROM LOC_KEY";
                command = new SQLiteCommand(sql, connection);
                id = command.ExecuteScalar().ConvertToInt32();
                id++;

                int nameSpaceId = UpsertNamespace(connection, value);

                sql = "INSERT INTO LOC_KEY (ID, NAMESPACE_ID, CODE, HASH) VALUES (@Id, @NameSpaceId, @Code, @Hash)";
                command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@NameSpaceId", nameSpaceId);
                command.Parameters.AddWithValue("@Code", value.Key);
                command.Parameters.AddWithValue("@Hash", value.Hash.ConvertToInt64());
                command.ExecuteNonQuery();
            }
            return id;
        }

        private void DeleteValue(SQLiteConnection connection, int cultureId)
        {
            string sql = "DELETE FROM LOC_VALUE WHERE CULTURE_ID = @CultureId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@CultureId", cultureId);
            command.ExecuteNonQuery();
        }

        private void InsertValue(SQLiteConnection connection, TranslationValue value, int cultureId)
        {
            int keyId = UpsertKey(connection, value);
            string sql = "INSERT INTO LOC_VALUE (CULTURE_ID, KEY_ID, ORIGINAL, VALUE) VALUES (@CultureId, @KeyId, @Original, @Value)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@CultureId", cultureId);
            command.Parameters.AddWithValue("@KeyId", keyId);
            command.Parameters.AddWithValue("@Original", value.EscapeOriginalValue);
            command.Parameters.AddWithValue("@Value", value.EscapeValue);
            command.ExecuteNonQuery();
        }

        private SQLiteConnection CreateConnection(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SQLiteConnection.CreateFile(filePath);
            }

            string connectionString = $"Data Source={filePath};Version=3;";
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            return connection;
        }

        private void CreateSchema(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS ""DBA_VERSION"" (
	            ""ID""   INTEGER NOT NULL,
	            ""CODE"" TEXT    NOT NULL UNIQUE,
	            PRIMARY KEY (""ID""))";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ""LOC_CULTURE"" (
	            ""ID""   INTEGER NOT NULL,
	            ""CODE"" TEXT    NOT NULL UNIQUE,
	            PRIMARY KEY (""ID""))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ""LOC_NAMESPACE"" (
	            ""ID""	 INTEGER NOT NULL,
	            ""CODE"" TEXT    NOT NULL UNIQUE,
	            PRIMARY KEY (""ID""))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ""LOC_KEY"" (
                ""ID""           INTEGER NOT NULL,
                ""NAMESPACE_ID"" INTEGER NOT NULL,
                ""CODE""         TEXT    NOT NULL UNIQUE,
	            ""HASH""         BIGINT  NOT NULL,
                PRIMARY KEY (""ID""),
                FOREIGN KEY (""NAMESPACE_ID"") REFERENCES ""LOC_NAMESPACE""(""ID""))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ""LOC_VALUE"" (
	            ""CULTURE_ID"" INTEGER NOT NULL,
	            ""KEY_ID""     INTEGER NOT NULL,
	            ""ORIGINAL""   TEXT,
	            ""VALUE""      TEXT,
	            PRIMARY KEY (""CULTURE_ID"",""KEY_ID""),
	            FOREIGN KEY (""CULTURE_ID"") REFERENCES ""LOC_CULTURE""(""ID""),
	            FOREIGN KEY (""KEY_ID"") REFERENCES ""LOC_KEY""(""ID""))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = @"
                CREATE VIEW IF NOT EXISTS V_VALUE AS
                SELECT CASE
                         WHEN V.VALUE IS NULL THEN 1
                         ELSE 0
                       END AS IS_EMPTY,
                       CASE
                         WHEN V.ORIGINAL <> V.VALUE AND V.VALUE IS NOT NULL THEN 1
                         ELSE 0
                       END AS IS_MODIFIED,
                       C.CODE AS CULTURE,
                       N.CODE AS NAMESPACE,
                       K.CODE AS KEY,
                       K.HASH AS HASH,
                       V.ORIGINAL AS ORIGINAL,
                       V.VALUE AS VALUE
                FROM   LOC_VALUE V
                JOIN   LOC_CULTURE C ON V.CULTURE_ID = C.ID
                JOIN   LOC_KEY K ON V.KEY_ID = K.ID
                JOIN   LOC_NAMESPACE N ON K.NAMESPACE_ID = N.ID";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "SELECT MAX(ID) FROM DBA_VERSION";
            command = new SQLiteCommand(sql, connection);
            int versionId = command.ExecuteScalar().ConvertToInt32();

            if (versionId == 0)
            {
                sql = @"INSERT INTO ""DBA_VERSION"" (""ID"", ""CODE"") values (1, '1.0.0')";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
