using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace lab10; 

public class Connector {

    private SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();

    public Connector() {
        connectionStringBuilder.DataSource = "./Database/app.db";
    }
    
    public void InitBD() {

        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            connection.Open();
            if ( !CreateTables(connection)) {
                Console.WriteLine("Error creating tables.");
                return;
            }
            if ( !FillTables(connection)) {
                Console.WriteLine("Error filing tables.");
                return;
            }

            AddUser("admin", "admin");
            
            Console.WriteLine("Db init success.");
        }
    }
    public bool CreateTables(SqliteConnection connection) {
        try {
            SqliteCommand cmd = connection.CreateCommand();
            
            // users
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY AUTOINCREMENT, login TEXT NOT NULL, password TEXT NOT NULL);";
            cmd.ExecuteNonQuery();

            // coffe
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS coffe (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, img TEXT, description TEXT);";
            cmd.ExecuteNonQuery();
            
            // recipes
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS recipes (id INTEGER PRIMARY KEY AUTOINCREMENT, ingredients TEXT NOT NULL, method TEXT NOT NULL);";
            cmd.ExecuteNonQuery();
            
            // categories
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS categories (id INTEGER NOT NULL, category TEXT NOT NULL, PRIMARY KEY (id, category));";
            cmd.ExecuteNonQuery();
            
            // ratings
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS ratings (id INTEGER NOT NULL, login TEXT NOT NULL, score INT, PRIMARY KEY (id, login));";
            cmd.ExecuteNonQuery();

            // to usunąć
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS data (Id INTEGER PRIMARY KEY AUTOINCREMENT, Data TEXT NOT NULL);";
            cmd.ExecuteNonQuery();
            
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return false;}
        return true;
    }
    
    
    private static (List<string>, List<List<string?>>) ReadFromCsv(string path, char sep=',') {
        var contnet = new List<List<string?>>();
        List<string> headers = new List<string>();
        using (StreamReader reader = new StreamReader(path))
        {
            string? line;
            if ((line = reader.ReadLine()) != null) {
                headers = new List<string>(line.Split(sep));
            }
            while ((line = reader.ReadLine()) != null)
            {
                var row = new List<string?>(line.Split(sep))
                    .Select(x=> x != "" ? x : null ).ToList();
                contnet.Add(row);
            }
        }

        return (headers, contnet);
    }
    
    private static int GetRowCount(SqliteConnection connection, string tableName)
    {
        string query = "SELECT COUNT(*) FROM " + tableName;
        
        using (SqliteCommand command = new SqliteCommand(query, connection))
        {
            int rowCount = Convert.ToInt32(command.ExecuteScalar());
            return rowCount;
        }
    }
    
    public bool FillTables(SqliteConnection connection) {
        try {

            string[] tabNames = {"coffe", "recipes", "categories"};
            foreach (var tabName in tabNames ) {
                
                var (headers, content) = ReadFromCsv($"./Database/table_{tabName}.csv", ';');
                if (GetRowCount(connection, tabName) == 0) {
                    using (var transaction = connection.BeginTransaction()) {
                        var insertCmdText = $"INSERT INTO {tabName} (" 
                                            + string.Join(", ", headers) + ") VALUES " 
                                            + string.Join(", ", content.Select(
                                                    row => "(" + string.Join(", ", 
                                                        row.Select(x => x != null ? '"'+x+'"' : "NULL")
                                                    ) + ")"
                                                )
                                            );
                        // Console.WriteLine(insertCmdText);
                        
                        SqliteCommand insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = insertCmdText;
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
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