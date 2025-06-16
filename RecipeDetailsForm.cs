using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class RecipeDetailsForm : Form
    {
        private int recipeId;
        private string recipeName;
        private Label lblTime;
        private Label lblCalories;
        private ListBox lstIngredients;
        private TextBox txtInstructions;
        private Label lblName;

        public RecipeDetailsForm(int recipeId)
        {
            this.recipeId = recipeId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Детали рецепта";
            this.Size = new System.Drawing.Size(600, 700);

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Создание элементов управления
            lblName = new Label
            {
                Text = "Загрузка...",
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font(this.Font.FontFamily, 16, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            GroupBox gbIngredients = new GroupBox
            {
                Text = "Ингредиенты",
                Dock = DockStyle.Fill
            };

            lstIngredients = new ListBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };
            gbIngredients.Controls.Add(lstIngredients);

            GroupBox gbInstructions = new GroupBox
            {
                Text = "Инструкция приготовления",
                Dock = DockStyle.Fill
            };

            txtInstructions = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };
            gbInstructions.Controls.Add(txtInstructions);

            lblTime = new Label
            {
                Text = "Время приготовления: загрузка...",
                AutoSize = true
            };

            lblCalories = new Label
            {
                Text = "Калорийность: загрузка...",
                AutoSize = true
            };

            // Создаем панель для информации
            TableLayoutPanel infoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true
            };
            infoPanel.Controls.Add(lblTime, 0, 0);
            infoPanel.Controls.Add(lblCalories, 1, 0);

            // Добавление элементов управления на форму
            mainPanel.Controls.Add(lblName, 0, 0);
            mainPanel.Controls.Add(gbIngredients, 0, 1);
            mainPanel.Controls.Add(gbInstructions, 0, 2);
            mainPanel.Controls.Add(infoPanel, 0, 3);
            
            this.Controls.Add(mainPanel);

            // Загружаем данные после создания всех контролов
            this.Load += (s, e) => LoadRecipeDetails();
        }

        private void LoadRecipeDetails()
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
                {
                    conn.Open();
                    
                    // Загружаем основную информацию о рецепте
                    string sql = @"SELECT Name, Instructions, CookingTime, Calories, Difficulty 
                                 FROM Recipes WHERE Id = @id";
                    
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", recipeId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                recipeName = reader.GetString(0);
                                txtInstructions.Text = reader.GetString(1);
                                int cookingTime = reader.GetInt32(2);
                                int calories = reader.GetInt32(3);
                                string difficulty = reader.GetString(4);
                                
                                lblName.Text = recipeName;
                                this.Text = recipeName;
                                lblTime.Text = $"Время приготовления: {cookingTime} минут";
                                lblCalories.Text = $"Калорийность: {calories} ккал";
                                
                                // Добавляем сложность к заголовку
                                this.Text += $" (Сложность: {difficulty})";
                            }
                            else
                            {
                                MessageBox.Show("Рецепт не найден", "Ошибка", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Close();
                                return;
                            }
                        }
                    }

                    // Загружаем ингредиенты рецепта с их количеством и единицами измерения
                    sql = @"SELECT i.Name, ri.Amount, i.Unit 
                           FROM RecipeIngredients ri 
                           JOIN Ingredients i ON ri.IngredientId = i.Id 
                           WHERE ri.RecipeId = @id
                           ORDER BY i.Name";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", recipeId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            lstIngredients.Items.Clear();
                            while (reader.Read())
                            {
                                string ingredientName = reader.GetString(0);
                                decimal amount = reader.GetDecimal(1);
                                string unit = reader.GetString(2);
                                
                                // Форматируем количество без лишних нулей после запятой
                                string amountStr = amount.ToString("0.##");
                                lstIngredients.Items.Add($"{ingredientName}: {amountStr} {unit}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке рецепта: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
}