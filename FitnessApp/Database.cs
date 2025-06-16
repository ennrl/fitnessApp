using System.Data.SQLite;
using System.IO;

namespace FitnessApp
{
    public static class Database
    {
        private static string dbPath = "fitness.db";
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // Создание таблиц
                    CreateTables(connection);
                }
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            // Таблица клиентов
            using (var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT
                )", connection))
            {
                command.ExecuteNonQuery();
            }

            // Таблица тренеров
            using (var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS Trainers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Specialization TEXT
                )", connection))
            {
                command.ExecuteNonQuery();
            }

            // Таблица тренировок
            using (var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS Workouts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT
                )", connection))
            {
                command.ExecuteNonQuery();
            }

            // Таблица расписания
            using (var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS Schedule (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WorkoutId INTEGER,
                    TrainerId INTEGER,
                    DateTime TEXT NOT NULL,
                    MaxParticipants INTEGER,
                    FOREIGN KEY(WorkoutId) REFERENCES Workouts(Id),
                    FOREIGN KEY(TrainerId) REFERENCES Trainers(Id)
                )", connection))
            {
                command.ExecuteNonQuery();
            }

            // Таблица записей на тренировки
            using (var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS Bookings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientId INTEGER,
                    ScheduleId INTEGER,
                    BookingDate TEXT NOT NULL,
                    FOREIGN KEY(ClientId) REFERENCES Clients(Id),
                    FOREIGN KEY(ScheduleId) REFERENCES Schedule(Id)
                )", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}