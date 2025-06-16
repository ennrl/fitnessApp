using System.Data.SQLite;
using System.IO;

namespace ConstructionMaterialsManagement
{
    public class Database
    {
        private static string dbPath = "construction.db";
        private static string connectionString;

        public static void Initialize()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                CreateTables();
            }
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        private static void CreateTables()
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE Suppliers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Contact TEXT,
                        Phone TEXT,
                        Address TEXT
                    );

                    CREATE TABLE Materials (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        Unit TEXT,
                        Price DECIMAL
                    );

                    CREATE TABLE Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SupplierId INTEGER,
                        OrderDate TEXT,
                        Status TEXT,
                        FOREIGN KEY(SupplierId) REFERENCES Suppliers(Id)
                    );

                    CREATE TABLE OrderDetails (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER,
                        MaterialId INTEGER,
                        Quantity INTEGER,
                        Price DECIMAL,
                        FOREIGN KEY(OrderId) REFERENCES Orders(Id),
                        FOREIGN KEY(MaterialId) REFERENCES Materials(Id)
                    );

                    CREATE TABLE Deliveries (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER,
                        DeliveryDate TEXT,
                        Status TEXT,
                        Notes TEXT,
                        FOREIGN KEY(OrderId) REFERENCES Orders(Id)
                    );";
                
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(connectionString);
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
            return connection;
        }
    }
}