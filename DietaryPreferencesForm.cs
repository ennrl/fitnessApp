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
            this.Controls.Add(chkVegetarian);
            this.Controls.Add(chkVegan);
            this.Controls.Add(chkGlutenFree);
            this.Controls.Add(nudCalories);
            this.Controls.Add(lblCalories);
            this.Controls.Add(btnSave);

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