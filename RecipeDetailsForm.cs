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
            Label lblName = new Label
            {
                Text = recipeName,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font(this.Font.FontFamily, 16, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
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

            // Настраиваем GroupBox'ы
            gbIngredients.Dock = DockStyle.Fill;
            gbInstructions.Dock = DockStyle.Fill;
            
            // Добавление элементов управления на форму
            mainPanel.Controls.Add(lblName, 0, 0);
            mainPanel.Controls.Add(gbIngredients, 0, 1);
            mainPanel.Controls.Add(gbInstructions, 0, 2);
            mainPanel.Controls.Add(infoPanel, 0, 3);
            
            this.Controls.Add(mainPanel);

            LoadRecipeDetails();
        }

        private void LoadRecipeDetails()
        {
            // Здесь будет код загрузки деталей рецепта из базы данных
        }
    }
}