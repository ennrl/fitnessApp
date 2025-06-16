using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Collections.Generic;

namespace SmartKitchenAssistant
{
    public partial class AddRecipeForm : Form
    {
        private List<KeyValuePair<int, decimal>> selectedIngredients = new List<KeyValuePair<int, decimal>>();

        public AddRecipeForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление нового рецепта";
            this.Size = new System.Drawing.Size(700, 800);

            // Название рецепта
            Label lblName = new Label
            {
                Text = "Название рецепта:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(120, 20)
            };

            TextBox txtName = new TextBox
            {
                Location = new System.Drawing.Point(150, 20),
                Size = new System.Drawing.Size(500, 20)
            };

            // Ингредиенты
            Label lblIngredients = new Label
            {
                Text = "Ингредиенты:",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(120, 20)
            };

            ComboBox cmbIngredients = new ComboBox
            {
                Location = new System.Drawing.Point(150, 60),
                Size = new System.Drawing.Size(300, 20)
            };

            NumericUpDown nudAmount = new NumericUpDown
            {
                Location = new System.Drawing.Point(460, 60),
                Size = new System.Drawing.Size(80, 20),
                DecimalPlaces = 2,
                Value = 1
            };

            Button btnAddIngredient = new Button
            {
                Text = "Добавить",
                Location = new System.Drawing.Point(550, 60),
                Size = new System.Drawing.Size(100, 23)
            };

            ListBox lstSelectedIngredients = new ListBox
            {
                Location = new System.Drawing.Point(150, 90),
                Size = new System.Drawing.Size(500, 150)
            };

            // Инструкции
            Label lblInstructions = new Label
            {
                Text = "Инструкции:",
                Location = new System.Drawing.Point(20, 260),
                Size = new System.Drawing.Size(120, 20)
            };

            TextBox txtInstructions = new TextBox
            {
                Location = new System.Drawing.Point(150, 260),
                Size = new System.Drawing.Size(500, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Время приготовления
            Label lblTime = new Label
            {
                Text = "Время (мин.):",
                Location = new System.Drawing.Point(20, 480),
                Size = new System.Drawing.Size(120, 20)
            };

            NumericUpDown nudTime = new NumericUpDown
            {
                Location = new System.Drawing.Point(150, 480),
                Size = new System.Drawing.Size(100, 20),
                Maximum = 999
            };

            // Сложность
            Label lblDifficulty = new Label
            {
                Text = "Сложность:",
                Location = new System.Drawing.Point(20, 520),
                Size = new System.Drawing.Size(120, 20)
            };

            ComboBox cmbDifficulty = new ComboBox
            {
                Location = new System.Drawing.Point(150, 520),
                Size = new System.Drawing.Size(200, 20)
            };
            cmbDifficulty.Items.AddRange(new string[] { "Легкий", "Средний", "Сложный" });

            // Калорийность
            Label lblCalories = new Label
            {
                Text = "Калории:",
                Location = new System.Drawing.Point(20, 560),
                Size = new System.Drawing.Size(120, 20)
            };

            NumericUpDown nudCalories = new NumericUpDown
            {
                Location = new System.Drawing.Point(150, 560),
                Size = new System.Drawing.Size(100, 20),
                Maximum = 9999
            };

            // Кнопка сохранения
            Button btnSave = new Button
            {
                Text = "Сохранить рецепт",
                Location = new System.Drawing.Point(150, 600),
                Size = new System.Drawing.Size(200, 40)
            };
            btnSave.Click += (s, e) => SaveRecipe(txtName.Text, txtInstructions.Text, 
                (int)nudTime.Value, cmbDifficulty.Text, (int)nudCalories.Value);

            // Добавление элементов на форму
            this.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblIngredients, cmbIngredients, nudAmount, btnAddIngredient, lstSelectedIngredients,
                lblInstructions, txtInstructions,
                lblTime, nudTime,
                lblDifficulty, cmbDifficulty,
                lblCalories, nudCalories,
                btnSave
            });

            LoadIngredients(cmbIngredients);
            
            btnAddIngredient.Click += (s, e) => {
                if (cmbIngredients.SelectedItem != null)
                {
                    var ingredient = (KeyValuePair<int, string>)cmbIngredients.SelectedItem;
                    selectedIngredients.Add(new KeyValuePair<int, decimal>(ingredient.Key, nudAmount.Value));
                    lstSelectedIngredients.Items.Add($"{ingredient.Value}: {nudAmount.Value}");
                }
            };
        }

        private void LoadIngredients(ComboBox cmbIngredients)
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Name FROM Ingredients", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIngredients.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0),
                                reader.GetString(1)
                            ));
                        }
                    }
                }
            }
        }

        private void SaveRecipe(string name, string instructions, int time, string difficulty, int calories)
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Сохраняем рецепт
                        string sql = @"INSERT INTO Recipes (Name, Instructions, CookingTime, Difficulty, Calories) 
                                     VALUES (@name, @instructions, @time, @difficulty, @calories);
                                     SELECT last_insert_rowid();";
                        
                        int recipeId;
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@instructions", instructions);
                            cmd.Parameters.AddWithValue("@time", time);
                            cmd.Parameters.AddWithValue("@difficulty", difficulty);
                            cmd.Parameters.AddWithValue("@calories", calories);
                            recipeId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Сохраняем ингредиенты рецепта
                        foreach (var ingredient in selectedIngredients)
                        {
                            sql = @"INSERT INTO RecipeIngredients (RecipeId, IngredientId, Amount) 
                                   VALUES (@recipeId, @ingredientId, @amount)";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@recipeId", recipeId);
                                cmd.Parameters.AddWithValue("@ingredientId", ingredient.Key);
                                cmd.Parameters.AddWithValue("@amount", ingredient.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Рецепт успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при сохранении рецепта: {ex.Message}", "Ошибка", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}