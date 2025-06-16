using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class AddBookingForm : Form
    {
        private ComboBox clientCombo;
        private ComboBox scheduleCombo;
        private Button saveButton;
        private Button cancelButton;

        public AddBookingForm()
        {
            InitializeComponents();
            LoadClients();
            LoadSchedule();
        }

        private void InitializeComponents()
        {
            this.Text = "Добавить запись на тренировку";
            this.Size = new System.Drawing.Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var clientLabel = new Label
            {
                Text = "Клиент:",
                Location = new System.Drawing.Point(10, 10)
            };

            clientCombo = new ComboBox
            {
                Location = new System.Drawing.Point(10, 30),
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var scheduleLabel = new Label
            {
                Text = "Тренировка:",
                Location = new System.Drawing.Point(10, 60)
            };

            scheduleCombo = new ComboBox
            {
                Location = new System.Drawing.Point(10, 80),
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            saveButton = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(200, 120)
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(290, 120)
            };

            this.Controls.AddRange(new Control[] { 
                clientLabel, clientCombo,
                scheduleLabel, scheduleCombo,
                saveButton, cancelButton 
            });
        }

        private void LoadClients()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT Id, Name FROM Clients", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientCombo.Items.Add(new ComboBoxItem { 
                            Id = reader.GetInt32(0), 
                            Name = reader.GetString(1) 
                        });
                    }
                }
            }
        }

        private void LoadSchedule()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(@"
                    SELECT Schedule.Id, 
                    Workouts.Name || ' - ' || Trainers.Name || ' (' || Schedule.DateTime || ')' as Info
                    FROM Schedule
                    JOIN Workouts ON Schedule.WorkoutId = Workouts.Id
                    JOIN Trainers ON Schedule.TrainerId = Trainers.Id
                    WHERE DateTime > datetime('now')
                    ORDER BY DateTime", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        scheduleCombo.Items.Add(new ComboBoxItem { 
                            Id = reader.GetInt32(0), 
                            Name = reader.GetString(1) 
                        });
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (clientCombo.SelectedItem == null || scheduleCombo.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента и тренировку!");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(@"
                    INSERT INTO Bookings (ClientId, ScheduleId, BookingDate)
                    VALUES (@ClientId, @ScheduleId, datetime('now'))", connection);

                command.Parameters.AddWithValue("@ClientId", 
                    ((ComboBoxItem)clientCombo.SelectedItem).Id);
                command.Parameters.AddWithValue("@ScheduleId", 
                    ((ComboBoxItem)scheduleCombo.SelectedItem).Id);

                command.ExecuteNonQuery();
            }
        }
    }
}