using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;

namespace lab10; 

public class Connector {

    private SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();

    private SortedSet<string> _categories = new SortedSet<string>();

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
            if ( !LoadData(connection)) {
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
    
    private bool LoadData(SqliteConnection connection) {
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
    
    private static string HashData(string input) {
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

                var passHash = HashData(password);
                cmd.CommandText = $"INSERT INTO Users (Login, Password) VALUES ('{login}', '{passHash}');";
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
                
                var passHash = HashData(password);
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

    public List<(int,string,string,string)>? GetCoffe(HashSet<string>? categories = null, string? sort = null) {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT *
                    FROM coffe
                    ";
                if (categories != null && categories.Count > 0) {
                    cmd.CommandText += " WHERE id IN (" + string.Join(" INTERSECT ", categories.Select(x => 
                        $@" SELECT id
                            from categories
                            WHERE category == '{x}'"
                    ));
                    cmd.CommandText += ")";
                }

                if (sort != null) {
                    cmd.CommandText += $" ORDER BY name {sort}";
                }
                cmd.CommandText += ";";
                using (var reader = cmd.ExecuteReader()) {
                    var data = new List<(int,string,string,string)>();
                    while (reader.Read()) {
                        int id = Int32.Parse(reader.GetString(0));
                        string name = reader.GetString(1);
                        string img = reader.GetString(2);
                        string desc = reader.GetString(3);
                        data.Add((id, name, img, desc));
                    }
                    return data;
                }
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return null;}
    }
    
    public (int,string,string,string,string,string,List<string>)? GetCoffeWithRecipeAndCategories(int id) {
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                
                cmd.CommandText = $@"
                    SELECT c.id, c.name, c.img, c.description, r.ingredients, r.method, GROUP_CONCAT(cat.category, ';')
                    FROM coffe as c
                             JOIN recipes as r ON c.id = r.id
                             JOIN categories as cat ON cat.id = c.id
                    WHERE r.id = {id}
                    GROUP BY c.id, c.name, c.img, c.description, r.ingredients, r.method
                    LIMIT 1;
                    ";
                
                using (var reader = cmd.ExecuteReader()) {
                    var data = new List<(int,string,string,string)>();
                    reader.Read();
                    string name = reader.GetString(1);
                    string img = reader.GetString(2);
                    string desc = reader.GetString(3);
                    string ing = reader.GetString(4);
                    string met = reader.GetString(5); 
                    List<string> cat = reader.GetString(6).Split(';').ToList();
                    return (id, name, img, desc, ing, met, cat);
                }
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return null;}
    }

    public SortedSet<string> GetCategories() {
        if (_categories.Count != 0) {
            return _categories;
        }
        try {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                
                cmd.CommandText = @"
                    SELECT DISTINCT category
                    FROM categories;
                    ";
                
                using (var reader = cmd.ExecuteReader()) {
                    var data = new SortedSet<string>();
                    while (reader.Read()) {
                        string cat = reader.GetString(0);
                        data.Add(cat);
                    }
                    _categories = data;
                    return _categories;
                }
            }
        } catch (Exception e){ 
            Console.WriteLine(e.Message);
            return null;}
    }
    
    //
    // public bool AddData(string data) {
    //     try {
    //         using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
    //         {
    //             connection.Open();
    //             SqliteCommand cmd = connection.CreateCommand();
    //             cmd.CommandText = $"INSERT INTO Data (Data) VALUES ('{data}');";
    //             cmd.ExecuteNonQuery();
    //         }
    //     } catch (Exception e){ 
    //         Console.WriteLine(e.Message);
    //         return false;}
    //     return true;
    // }
    //
    // public bool RemoveData(int id) {
    //     using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString)) {
    //         try {
    //             connection.Open();
    //             SqliteCommand cmd = connection.CreateCommand();
    //             cmd.CommandText =
    //                 $"DELETE FROM Data WHERE id = {id};";
    //             cmd.ExecuteNonQuery();
    //         }
    //         catch (Exception e) {
    //             Console.WriteLine(e.Message);
    //             return false;
    //         }
    //
    //         return true;
    //     }
    // }

    public void UpdateCoffee(int id, string name, string image, string desc)
    {
        int idd = id;
        string namee = name;
        string imagee = image;
        string descc = desc;
        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            connection.Open();
            SqliteCommand cmd = connection.CreateCommand();

            cmd.CommandText = $"UPDATE coffe SET name = '{namee}', img = '{imagee}', description = '{descc}' WHERE id = {idd};";
            cmd.ExecuteNonQuery();
        }
    }
}