using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class RecipeSearchForm : Form
    {
        public int? SelectedRecipeId { get; private set; }
        private ListBox lstIngredients;
        private ListBox lstRecipes;
        public RecipeSearchForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Поиск рецептов";
            this.Size = new System.Drawing.Size(600, 500);

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Создание элементов управления
            Label lblIngredients = new Label
            {
                Text = "Доступные ингредиенты:",
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            this.lstIngredients = new ListBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(200, 300)
            };

            Button btnSearch = new Button
            {
                Text = "Найти рецепты",
                Location = new System.Drawing.Point(20, 370),
                Size = new System.Drawing.Size(200, 30)
            };
            btnSearch.Click += BtnSearch_Click;

            this.lstRecipes = new ListBox
            {
                Location = new System.Drawing.Point(250, 50),
                Size = new System.Drawing.Size(300, 300)
            };
            lstRecipes.DoubleClick += LstRecipes_DoubleClick;

            // Создаем панели для группировки элементов
            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            Panel rightPanel = new Panel { Dock = DockStyle.Fill };

            lstIngredients.Dock = DockStyle.Fill;
            lstRecipes.Dock = DockStyle.Fill;
            btnSearch.Dock = DockStyle.Bottom;

            leftPanel.Controls.Add(lstIngredients);
            leftPanel.Controls.Add(btnSearch);
            rightPanel.Controls.Add(lstRecipes);

            // Добавление элементов управления на форму
            mainPanel.Controls.Add(lblIngredients, 0, 0);
            mainPanel.Controls.Add(new Label { Text = "Найденные рецепты:", AutoSize = true }, 1, 0);
            mainPanel.Controls.Add(leftPanel, 0, 1);
            mainPanel.Controls.Add(rightPanel, 1, 1);

            this.Controls.Add(mainPanel);

            LoadIngredients();
        }

        private void LoadIngredients()
        {
            lstIngredients.SelectionMode = SelectionMode.MultiSimple;
            lstIngredients.DisplayMember = "Value";
            lstIngredients.ValueMember = "Key";

            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Name FROM Ingredients ORDER BY Name", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lstIngredients.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0),
                                reader.GetString(1)
                            ));
                        }
                    }
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {

            if (lstIngredients.SelectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один ингредиент", "Предупреждение", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                var selectedIngredientIds = new List<int>();
                foreach (KeyValuePair<int, string> item in lstIngredients.SelectedItems)
                {
                    selectedIngredientIds.Add(item.Key);
                }

                string ingredientList = string.Join(",", selectedIngredientIds);
                string sql = @"
                    SELECT DISTINCT r.Id, r.Name, r.CookingTime, r.Difficulty, r.Calories,
                    (SELECT COUNT(DISTINCT IngredientId) 
                     FROM RecipeIngredients 
                     WHERE RecipeId = r.Id AND IngredientId IN (" + ingredientList + @")) as MatchingIngredients
                    FROM Recipes r
                    INNER JOIN RecipeIngredients ri ON r.Id = ri.RecipeId
                    WHERE ri.IngredientId IN (" + ingredientList + @")
                    ORDER BY MatchingIngredients DESC, r.Name";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        lstRecipes.Items.Clear();
                        while (reader.Read())
                        {
                            string recipeName = reader.GetString(1);
                            int cookingTime = reader.GetInt32(2);
                            string difficulty = reader.GetString(3);
                            int calories = reader.GetInt32(4);
                            int matchingCount = reader.GetInt32(5);
                            
                            var recipe = new KeyValuePair<int, string>(
                                reader.GetInt32(0),
                                $"{recipeName} ({matchingCount} совпадений) - {cookingTime} мин., {difficulty}, {calories} ккал"
                            );                                lstRecipes.Items.Add(recipe);
                            }

                            if (lstRecipes.Items.Count > 0)
                            {
                                lstRecipes.DoubleClick += (s, args) => 
                                {
                                    if (lstRecipes.SelectedItem != null)
                                    {
                                        var selectedRecipe = (KeyValuePair<int, string>)lstRecipes.SelectedItem;
                                        SelectedRecipeId = selectedRecipe.Key;
                                        DialogResult = DialogResult.OK;
                                        Close();
                                    }
                                };
                            }
                            else
                        {
                            MessageBox.Show("Рецепты с выбранными ингредиентами не найдены", 
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void LstRecipes_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
            {
                var recipe = (KeyValuePair<int, string>)((ListBox)sender).SelectedItem;
                using (var form = new RecipeDetailsForm(recipe.Key))
                {
                    form.ShowDialog();
                }
            }
        }
    }
}