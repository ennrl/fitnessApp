using System;
using System.Windows.Forms;

namespace FitnessApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Фитнес-центр";
            this.Size = new System.Drawing.Size(800, 600);

            // Создание меню
            MenuStrip mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Пункты меню
            ToolStripMenuItem scheduleMenu = new ToolStripMenuItem("Расписание");
            ToolStripMenuItem clientsMenu = new ToolStripMenuItem("Клиенты");
            ToolStripMenuItem trainersMenu = new ToolStripMenuItem("Тренеры");
            ToolStripMenuItem workoutsMenu = new ToolStripMenuItem("Тренировки");
            ToolStripMenuItem bookingsMenu = new ToolStripMenuItem("Записи");

            mainMenu.Items.AddRange(new ToolStripItem[] {
                scheduleMenu,
                clientsMenu,
                trainersMenu,
                workoutsMenu,
                bookingsMenu
            });

            // Обработчики событий
            scheduleMenu.Click += (s, e) => new ScheduleForm().Show();
            clientsMenu.Click += (s, e) => new ClientsForm().Show();
            trainersMenu.Click += (s, e) => new TrainersForm().Show();
            workoutsMenu.Click += (s, e) => new WorkoutsForm().Show();
            bookingsMenu.Click += (s, e) => new BookingsForm().Show();
        }
    }
}