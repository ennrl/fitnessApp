using System;
using System.Windows.Forms;

namespace SmartKitchenAssistant
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            DatabaseManager.InitializeDatabase();
        }

        private void InitializeComponent()
        {
            this.Text = "Умный кулинарный помощник";
            this.Size = new System.Drawing.Size(800, 600);

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(20),
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
            };

            // Создание кнопок
            Button btnSearchRecipes = new Button
            {
                Text = "Поиск рецептов",
                Size = new System.Drawing.Size(200, 40),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(10)
            };
            btnSearchRecipes.Click += BtnSearchRecipes_Click;

            Button btnDietaryPreferences = new Button
            {
                Text = "Диетические предпочтения",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(200, 40)
            };
            btnDietaryPreferences.Click += BtnDietaryPreferences_Click;

            Button btnMealPlanner = new Button
            {
                Text = "Планировщик питания",
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(200, 40)
            };
            btnMealPlanner.Click += BtnMealPlanner_Click;

            Button btnAddRecipe = new Button
            {
                Text = "Добавить рецепт",
                Location = new System.Drawing.Point(50, 200),
                Size = new System.Drawing.Size(200, 40)
            };
            btnAddRecipe.Click += BtnAddRecipe_Click;

            Button btnManageIngredients = new Button
            {
                Text = "Управление ингредиентами",
                Location = new System.Drawing.Point(50, 250),
                Size = new System.Drawing.Size(200, 40)
            };
            btnManageIngredients.Click += BtnManageIngredients_Click;

            // Добавление элементов управления на форму
            buttonPanel.Controls.Add(btnSearchRecipes);
            buttonPanel.Controls.Add(btnDietaryPreferences);
            buttonPanel.Controls.Add(btnMealPlanner);
            buttonPanel.Controls.Add(btnAddRecipe);
            buttonPanel.Controls.Add(btnManageIngredients);
            
            mainPanel.Controls.Add(buttonPanel, 0, 0);
            this.Controls.Add(mainPanel);
        }

        private void BtnSearchRecipes_Click(object sender, EventArgs e)
        {
            using (var form = new RecipeSearchForm())
            {
                form.ShowDialog();
            }
        }

        private void BtnDietaryPreferences_Click(object sender, EventArgs e)
        {
            using (var form = new DietaryPreferencesForm())
            {
                form.ShowDialog();
            }
        }

        private void BtnMealPlanner_Click(object sender, EventArgs e)
        {
            using (var form = new MealPlannerForm())
            {
                form.ShowDialog();
            }
        }

        private void BtnAddRecipe_Click(object sender, EventArgs e)
        {
            using (var form = new AddRecipeForm())
            {
                form.ShowDialog();
            }
        }

        private void BtnManageIngredients_Click(object sender, EventArgs e)
        {
            using (var form = new IngredientsManagerForm())
            {
                form.ShowDialog();
            }
        }
    }
}