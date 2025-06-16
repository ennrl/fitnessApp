using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class MealPlannerForm : Form
    {
        private ListBox lstMeals;
        private ComboBox cmbMealType;
        private MonthCalendar calendar;
        
        public MealPlannerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Планировщик питания";
            this.Size = new System.Drawing.Size(800, 600);

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Создание элементов управления
            this.calendar = new MonthCalendar();
            this.calendar.Dock = DockStyle.Top;
            this.calendar.MaxSelectionCount = 1;
            this.calendar.DateSelected += Calendar_DateSelected;

            this.cmbMealType = new ComboBox();
            this.cmbMealType.Dock = DockStyle.Fill;
            this.cmbMealType.Items.AddRange(new string[] { "Завтрак", "Обед", "Ужин" });

            this.lstMeals = new ListBox();
            this.lstMeals.Dock = DockStyle.Fill;

            Button btnAddMeal = new Button
            {
                Text = "Добавить блюдо",
                Size = new System.Drawing.Size(150, 30)
            };
            btnAddMeal.Click += BtnAddMeal_Click;

            Button btnGeneratePlan = new Button
            {
                Text = "Сгенерировать план на неделю",
                Size = new System.Drawing.Size(200, 30)
            };
            btnGeneratePlan.Click += BtnGeneratePlan_Click;

            // Создаем правую панель для элементов управления
            TableLayoutPanel rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(5)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            buttonPanel.Controls.AddRange(new Control[] { btnAddMeal, btnGeneratePlan });

            // Добавляем элементы в правую панель
            rightPanel.Controls.Add(cmbMealType, 0, 0);
            rightPanel.Controls.Add(lstMeals, 0, 1);
            rightPanel.Controls.Add(buttonPanel, 0, 2);

            // Добавляем элементы в главную панель
            Panel calendarPanel = new Panel { Padding = new Padding(10) };
            calendarPanel.Controls.Add(calendar);
            mainPanel.Controls.Add(calendarPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // Добавляем главную панель на форму
            this.Controls.Add(mainPanel);
        }

        private void Calendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
                {
                    conn.Open();
                    string sql = @"
                        SELECT r.Name, mp.MealType, r.CookingTime, r.Calories
                        FROM MealPlans mp
                        JOIN Recipes r ON mp.RecipeId = r.Id
                        WHERE date(mp.Date) = date(@date)
                        ORDER BY 
                            CASE mp.MealType 
                                WHEN 'Завтрак' THEN 1 
                                WHEN 'Обед' THEN 2 
                                WHEN 'Ужин' THEN 3 
                            END";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", e.Start.ToString("yyyy-MM-dd"));
                        using (var reader = cmd.ExecuteReader())
                        {
                            lstMeals.Items.Clear();
                            int totalCalories = 0;
                            while (reader.Read())
                            {
                                string recipeName = reader.GetString(0);
                                string mealType = reader.GetString(1);
                                int cookingTime = reader.GetInt32(2);
                                int calories = reader.GetInt32(3);
                                totalCalories += calories;
                                lstMeals.Items.Add($"{mealType}: {recipeName} ({cookingTime} мин., {calories} ккал)");
                            }

                            if (lstMeals.Items.Count > 0)
                            {
                                lstMeals.Items.Add("");
                                lstMeals.Items.Add($"Общая калорийность за день: {totalCalories} ккал");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке меню: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddMeal_Click(object sender, EventArgs e)
        {
            if (cmbMealType.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип приема пищи", 
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new RecipeSearchForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.SelectedRecipeId.HasValue)
                {
                    try
                    {
                        using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
                        {
                            conn.Open();

                            // Проверяем, нет ли уже блюда для этого типа приема пищи в этот день
                            string checkSql = @"
                                SELECT COUNT(*)
                                FROM MealPlans
                                WHERE Date = @date AND MealType = @mealType";

                            using (var cmd = new SQLiteCommand(checkSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@date", calendar.SelectionStart.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@mealType", cmbMealType.SelectedItem.ToString());
                                int count = Convert.ToInt32(cmd.ExecuteScalar());

                                if (count > 0)
                                {
                                    if (MessageBox.Show(
                                        "На этот прием пищи уже назначено блюдо. Заменить?",
                                        "Подтверждение",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) != DialogResult.Yes)
                                    {
                                        return;
                                    }
                                }
                            }

                            // Добавляем или обновляем запись в плане
                            string sql = @"
                                INSERT OR REPLACE INTO MealPlans (Date, RecipeId, MealType)
                                VALUES (@date, @recipeId, @mealType)";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@date", calendar.SelectionStart.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@recipeId", form.SelectedRecipeId.Value);
                                cmd.Parameters.AddWithValue("@mealType", cmbMealType.SelectedItem.ToString());
                                cmd.ExecuteNonQuery();
                            }

                            // Обновляем список блюд
                            Calendar_DateSelected(calendar, new DateRangeEventArgs(calendar.SelectionStart, calendar.SelectionStart));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении блюда в план: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnGeneratePlan_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Загружаем диетические предпочтения
                            bool isVegetarian = false;
                            bool isVegan = false;
                            bool isGlutenFree = false;
                            int maxDailyCalories = 5000;

                            string prefsQuery = "SELECT IsVegetarian, IsVegan, IsGlutenFree, DailyCalories FROM UserPreferences WHERE Id = 1";
                            using (var cmd = new SQLiteCommand(prefsQuery, conn))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        isVegetarian = reader.GetBoolean(0);
                                        isVegan = reader.GetBoolean(1);
                                        isGlutenFree = reader.GetBoolean(2);
                                        maxDailyCalories = reader.GetInt32(3);
                                    }
                                }
                            }

                            // Получаем список подходящих рецептов с учетом диетических предпочтений
                            var recipes = new List<(int id, string name, int calories)>();
                            string sqlRecipes = @"
                                SELECT Id, Name, Calories 
                                FROM Recipes 
                                WHERE 1=1 
                                    AND (@isVegetarian = 0 OR IsVegetarian = 1)
                                    AND (@isVegan = 0 OR IsVegan = 1)
                                    AND (@isGlutenFree = 0 OR IsGlutenFree = 1)
                                ORDER BY RANDOM()";
                            using (var cmd = new SQLiteCommand(sqlRecipes, conn))
                            {
                                cmd.Parameters.AddWithValue("@isVegetarian", isVegetarian);
                                cmd.Parameters.AddWithValue("@isVegan", isVegan);
                                cmd.Parameters.AddWithValue("@isGlutenFree", isGlutenFree);
                                
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        recipes.Add((
                                            reader.GetInt32(0),
                                            reader.GetString(1),
                                            reader.GetInt32(2)
                                        ));
                                    }
                                }
                            }

                            if (recipes.Count < 21) // 3 приема пищи * 7 дней
                            {
                                throw new Exception("Недостаточно рецептов для составления плана на неделю");
                            }

                            // Генерируем план на неделю
                            Random rnd = new Random();
                            DateTime startDate = calendar.SelectionStart.Date;
                            string[] mealTypes = { "Завтрак", "Обед", "Ужин" };

                            string sqlInsert = @"
                                INSERT OR REPLACE INTO MealPlans (Date, RecipeId, MealType)
                                VALUES (@date, @recipeId, @mealType)";

                            for (int day = 0; day < 7; day++)
                            {
                                foreach (string mealType in mealTypes)
                                {
                                    // Выбираем случайный рецепт
                                    int recipeIndex = rnd.Next(recipes.Count);
                                    var recipe = recipes[recipeIndex];
                                    recipes.RemoveAt(recipeIndex); // Убираем использованный рецепт

                                    // Сохраняем в базу
                                    using (var cmd = new SQLiteCommand(sqlInsert, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@date", startDate.AddDays(day).ToString("yyyy-MM-dd"));
                                        cmd.Parameters.AddWithValue("@recipeId", recipe.id);
                                        cmd.Parameters.AddWithValue("@mealType", mealType);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                            Calendar_DateSelected(calendar, new DateRangeEventArgs(calendar.SelectionStart, calendar.SelectionStart));
                            MessageBox.Show("План питания на неделю успешно сгенерирован!", 
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации плана: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}