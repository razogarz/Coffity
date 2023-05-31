using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace lab10; 

public class Connector {

    // private string dataSource = "./app.db";
    private SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();

    public Connector() {
        connectionStringBuilder.DataSource = "./app.db";
    }
    
    public void InitBD() {

        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            connection.Open();
            if ( !CreateTables(connection)) {
                Console.WriteLine("Error creating tables.");
                return;
            }

            AddUser("admin", "admin");
            
            Console.WriteLine("Db init success.");
        }
    }
    public bool CreateTables(SqliteConnection connection) {
        try {
            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Login TEXT NOT NULL, Password TEXT NOT NULL);";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Data (Id INTEGER PRIMARY KEY AUTOINCREMENT, Data TEXT NOT NULL);";
            cmd.ExecuteNonQuery();
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return false;}
        return true;
    }
    
    private static string getHashedData(string input) {
        using (var md5 = MD5.Create()) {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
    
    public bool AddUser(string login, string password) {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT * FROM Users WHERE Login='{login}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) {
                        return false;
                    }
                }

                var passHash = getHashedData(password);
                cmd.CommandText = $"INSERT INTO Users (Login, Password) VALUES ('{login}', '{passHash}');";
                cmd.ExecuteNonQuery();
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return false;}
        return true;
    }
    
    public bool AddData(string data) {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"INSERT INTO Data (Data) VALUES ('{data}');";
                cmd.ExecuteNonQuery();
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return false;}
        return true;
    }
    
    public bool ValidateUser(string login, string password) {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                
                var passHash = getHashedData(password);
                Console.WriteLine($"Validated user {login} with password {password} ({passHash})");
                cmd.CommandText = $"SELECT * FROM Users WHERE Login='{login}' AND Password='{passHash}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) {
                        return true;
                    }
                }
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return false;}
        return false;
    }

    public List<(int, string)>? getData() {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                
                cmd.CommandText = $"SELECT * FROM Data;";
                using (var reader = cmd.ExecuteReader()) {
                    var data = new List<(int,string)>();
                    while (reader.Read()) {
                        string str = "";
                        int id = Int32.Parse(reader.GetString(0));
                        for (int a = 1; a < reader.FieldCount; a++) {
                            str += " " + reader.GetString(a);
                        }
                        data.Add((id, str.Length > 0 ? str.Substring(1) : ""));
                    }
                    return data;
                }
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return null;}
    }

    public bool RemoveData(int id) {
        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString)) {
            try {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText =
                    $"DELETE FROM Data WHERE id = {id};";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }

}