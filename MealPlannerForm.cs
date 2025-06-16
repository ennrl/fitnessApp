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

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Создание элементов управления
            MonthCalendar calendar = new MonthCalendar
            {
                Dock = DockStyle.Top,
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

            // Создаем правую панель для элементов управления
            TableLayoutPanel rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(5)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Настраиваем свойства элементов
            cmbMealType.Dock = DockStyle.Fill;
            lstMeals.Dock = DockStyle.Fill;

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            buttonPanel.Controls.AddRange(new Control[] { btnAddMeal, btnGeneratePlan });

            // Добавляем элементы в правую панель
            rightPanel.Controls.Add(cmbMealType, 0, 0);
            rightPanel.Controls.Add(lstMeals, 0, 1);
            rightPanel.Controls.Add(buttonPanel, 0, 2);

            // Добавляем элементы в главную панель
            Panel calendarPanel = new Panel { Padding = new Padding(10) };
            calendarPanel.Controls.Add(calendar);
            mainPanel.Controls.Add(calendarPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // Добавляем главную панель на форму
            this.Controls.Add(mainPanel);
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