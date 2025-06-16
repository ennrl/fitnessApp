using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class MealPlannerForm : Form
    {
        public MealPlannerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Планировщик питания";
            this.Size = new System.Drawing.Size(800, 600);

            // Создание элементов управления
            MonthCalendar calendar = new MonthCalendar
            {
                Location = new System.Drawing.Point(20, 20),
                MaxSelectionCount = 1
            };
            calendar.DateSelected += Calendar_DateSelected;

            ComboBox cmbMealType = new ComboBox
            {
                Location = new System.Drawing.Point(250, 20),
                Size = new System.Drawing.Size(150, 20)
            };
            cmbMealType.Items.AddRange(new string[] { "Завтрак", "Обед", "Ужин" });

            ListBox lstMeals = new ListBox
            {
                Location = new System.Drawing.Point(250, 50),
                Size = new System.Drawing.Size(500, 400)
            };

            Button btnAddMeal = new Button
            {
                Text = "Добавить блюдо",
                Location = new System.Drawing.Point(250, 470),
                Size = new System.Drawing.Size(150, 30)
            };
            btnAddMeal.Click += BtnAddMeal_Click;

            Button btnGeneratePlan = new Button
            {
                Text = "Сгенерировать план на неделю",
                Location = new System.Drawing.Point(420, 470),
                Size = new System.Drawing.Size(200, 30)
            };
            btnGeneratePlan.Click += BtnGeneratePlan_Click;

            // Добавление элементов управления на форму
            this.Controls.Add(calendar);
            this.Controls.Add(cmbMealType);
            this.Controls.Add(lstMeals);
            this.Controls.Add(btnAddMeal);
            this.Controls.Add(btnGeneratePlan);
        }

        private void Calendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            // Здесь будет код загрузки меню на выбранную дату
        }

        private void BtnAddMeal_Click(object sender, EventArgs e)
        {
            using (var form = new RecipeSearchForm())
            {
                form.ShowDialog();
            }
        }

        private void BtnGeneratePlan_Click(object sender, EventArgs e)
        {
            // Здесь будет код генерации плана питания на неделю
            MessageBox.Show("План питания сгенерирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}