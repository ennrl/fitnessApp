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

            // Создание элементов управления
            Label lblIngredients = new Label
            {
                Text = "Доступные ингредиенты:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 20)
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

            // Добавление элементов управления на форму
            this.Controls.Add(lblIngredients);
            this.Controls.Add(lstIngredients);
            this.Controls.Add(btnSearch);
            this.Controls.Add(lstRecipes);

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