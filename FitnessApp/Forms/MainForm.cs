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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.Text = "Фитнес-центр";
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeUI()
        {
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