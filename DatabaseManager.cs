using System;
using System.Data.SQLite;
using System.IO;

namespace SmartKitchenAssistant
{
    public class DatabaseManager
    {
        private static string dbPath = "kitchen_assistant.db";
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                CreateTables();
            }
        }

        private static void CreateTables()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Создание таблицы ингредиентов
                string createIngredientsTable = @"
                    CREATE TABLE IF NOT EXISTS Ingredients (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Category TEXT,
                        Unit TEXT
                    )";

                // Создание таблицы рецептов
                string createRecipesTable = @"
                    CREATE TABLE IF NOT EXISTS Recipes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Instructions TEXT,
                        CookingTime INTEGER,
                        Difficulty TEXT,
                        Calories INTEGER
                    )";

                // Создание таблицы диетических предпочтений
                string createDietaryPreferencesTable = @"
                    CREATE TABLE IF NOT EXISTS DietaryPreferences (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        Restrictions TEXT
                    )";

                // Создание таблицы планов питания
                string createMealPlansTable = @"
                    CREATE TABLE IF NOT EXISTS MealPlans (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        RecipeId INTEGER,
                        MealType TEXT,
                        FOREIGN KEY(RecipeId) REFERENCES Recipes(Id)
                    )";

                // Создание таблицы связи рецептов и ингредиентов
                string createRecipeIngredientsTable = @"
                    CREATE TABLE IF NOT EXISTS RecipeIngredients (
                        RecipeId INTEGER,
                        IngredientId INTEGER,
                        Amount REAL,
                        FOREIGN KEY(RecipeId) REFERENCES Recipes(Id),
                        FOREIGN KEY(IngredientId) REFERENCES Ingredients(Id),
                        PRIMARY KEY(RecipeId, IngredientId)
                    )";

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = createIngredientsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createDietaryPreferencesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createMealPlansTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipeIngredientsTable;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}