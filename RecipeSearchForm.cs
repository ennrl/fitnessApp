using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class RecipeSearchForm : Form
    {
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

            ListBox lstIngredients = new ListBox
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

            ListBox lstRecipes = new ListBox
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

            LoadIngredients(lstIngredients);
        }

        private void LoadIngredients(ListBox lstIngredients)
        {
            // Здесь будет код загрузки ингредиентов из базы данных
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            // Здесь будет код поиска рецептов
        }

        private void LstRecipes_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
            {
                using (var form = new RecipeDetailsForm(((ListBox)sender).SelectedItem.ToString()))
                {
                    form.ShowDialog();
                }
            }
        }
    }
}