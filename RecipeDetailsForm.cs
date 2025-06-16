using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class RecipeDetailsForm : Form
    {
        private string recipeName;

        public RecipeDetailsForm(string recipeName)
        {
            this.recipeName = recipeName;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Детали рецепта";
            this.Size = new System.Drawing.Size(600, 700);

            // Создание элементов управления
            Label lblName = new Label
            {
                Text = recipeName,
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(540, 30),
                Font = new System.Drawing.Font(this.Font.FontFamily, 16, System.Drawing.FontStyle.Bold)
            };

            GroupBox gbIngredients = new GroupBox
            {
                Text = "Ингредиенты",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(540, 200)
            };

            ListBox lstIngredients = new ListBox
            {
                Location = new System.Drawing.Point(10, 20),
                Size = new System.Drawing.Size(520, 170),
                Parent = gbIngredients
            };

            GroupBox gbInstructions = new GroupBox
            {
                Text = "Инструкция приготовления",
                Location = new System.Drawing.Point(20, 270),
                Size = new System.Drawing.Size(540, 300)
            };

            TextBox txtInstructions = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(10, 20),
                Size = new System.Drawing.Size(520, 270),
                Parent = gbInstructions
            };

            Label lblTime = new Label
            {
                Text = "Время приготовления: ",
                Location = new System.Drawing.Point(20, 580),
                Size = new System.Drawing.Size(200, 20)
            };

            Label lblCalories = new Label
            {
                Text = "Калорийность: ",
                Location = new System.Drawing.Point(20, 610),
                Size = new System.Drawing.Size(200, 20)
            };

            // Добавление элементов управления на форму
            this.Controls.Add(lblName);
            this.Controls.Add(gbIngredients);
            this.Controls.Add(gbInstructions);
            this.Controls.Add(lblTime);
            this.Controls.Add(lblCalories);

            LoadRecipeDetails();
        }

        private void LoadRecipeDetails()
        {
            // Здесь будет код загрузки деталей рецепта из базы данных
        }
    }
}