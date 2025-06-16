using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class DietaryPreferencesForm : Form
    {
        public DietaryPreferencesForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Диетические предпочтения";
            this.Size = new System.Drawing.Size(400, 500);

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            for (int i = 0; i < 6; i++)
            {
                mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Создание элементов управления
            CheckBox chkVegetarian = new CheckBox
            {
                Text = "Вегетарианская диета",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(200, 20)
            };

            CheckBox chkVegan = new CheckBox
            {
                Text = "Веганская диета",
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(200, 20)
            };

            CheckBox chkGlutenFree = new CheckBox
            {
                Text = "Без глютена",
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(200, 20)
            };

            NumericUpDown nudCalories = new NumericUpDown
            {
                Location = new System.Drawing.Point(150, 120),
                Size = new System.Drawing.Size(100, 20),
                Maximum = 5000,
                Minimum = 500,
                Value = 2000
            };

            Label lblCalories = new Label
            {
                Text = "Калории в день:",
                Location = new System.Drawing.Point(20, 122),
                Size = new System.Drawing.Size(120, 20)
            };

            Button btnSave = new Button
            {
                Text = "Сохранить",
                Location = new System.Drawing.Point(20, 400),
                Size = new System.Drawing.Size(150, 30)
            };
            btnSave.Click += BtnSave_Click;

            // Добавление элементов управления на форму
            mainPanel.Controls.Add(chkVegetarian, 0, 0);
            mainPanel.SetColumnSpan(chkVegetarian, 2);
            
            mainPanel.Controls.Add(chkVegan, 0, 1);
            mainPanel.SetColumnSpan(chkVegan, 2);
            
            mainPanel.Controls.Add(chkGlutenFree, 0, 2);
            mainPanel.SetColumnSpan(chkGlutenFree, 2);
            
            mainPanel.Controls.Add(lblCalories, 0, 3);
            mainPanel.Controls.Add(nudCalories, 1, 3);
            
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            buttonPanel.Controls.Add(btnSave);
            
            mainPanel.Controls.Add(buttonPanel, 0, 4);
            mainPanel.SetColumnSpan(buttonPanel, 2);
            
            this.Controls.Add(mainPanel);

            LoadPreferences();
        }

        private void LoadPreferences()
        {
            // Здесь будет код загрузки предпочтений из базы данных
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Здесь будет код сохранения предпочтений в базу данных
            MessageBox.Show("Предпочтения сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}