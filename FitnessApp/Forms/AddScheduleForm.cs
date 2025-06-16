using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class AddScheduleForm : Form
    {
        private ComboBox workoutCombo;
        private ComboBox trainerCombo;
        private DateTimePicker datePicker;
        private NumericUpDown participantsNumeric;
        private Button saveButton;
        private Button cancelButton;
        private int? editId;

        public AddScheduleForm(int? id = null)
        {
            editId = id;
            InitializeComponents();
            LoadWorkouts();
            LoadTrainers();
            if (editId.HasValue)
            {
                LoadScheduleData();
            }
        }

        private void InitializeComponents()
        {
            this.Text = editId.HasValue ? "Изменить тренировку" : "Добавить тренировку";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var workoutLabel = new Label
            {
                Text = "Тренировка:",
                Location = new System.Drawing.Point(10, 10)
            };

            workoutCombo = new ComboBox
            {
                Location = new System.Drawing.Point(10, 30),
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var trainerLabel = new Label
            {
                Text = "Тренер:",
                Location = new System.Drawing.Point(10, 60)
            };

            trainerCombo = new ComboBox
            {
                Location = new System.Drawing.Point(10, 80),
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var dateLabel = new Label
            {
                Text = "Дата и время:",
                Location = new System.Drawing.Point(10, 110)
            };

            datePicker = new DateTimePicker
            {
                Location = new System.Drawing.Point(10, 130),
                Width = 360,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };

            var participantsLabel = new Label
            {
                Text = "Максимум участников:",
                Location = new System.Drawing.Point(10, 160)
            };

            participantsNumeric = new NumericUpDown
            {
                Location = new System.Drawing.Point(10, 180),
                Width = 360,
                Minimum = 1,
                Maximum = 100,
                Value = 10
            };

            saveButton = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(200, 220)
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(290, 220)
            };

            this.Controls.AddRange(new Control[] { 
                workoutLabel, workoutCombo, 
                trainerLabel, trainerCombo,
                dateLabel, datePicker,
                participantsLabel, participantsNumeric,
                saveButton, cancelButton 
            });
        }

        private void LoadWorkouts()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT Id, Name FROM Workouts", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        workoutCombo.Items.Add(new ComboBoxItem { 
                            Id = reader.GetInt32(0), 
                            Name = reader.GetString(1) 
                        });
                    }
                }
            }
        }

        private void LoadTrainers()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT Id, Name FROM Trainers", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trainerCombo.Items.Add(new ComboBoxItem { 
                            Id = reader.GetInt32(0), 
                            Name = reader.GetString(1) 
                        });
                    }
                }
            }
        }

        private void LoadScheduleData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT WorkoutId, TrainerId, DateTime, MaxParticipants FROM Schedule WHERE Id = @Id", 
                    connection);
                command.Parameters.AddWithValue("@Id", editId.Value);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        SetComboBoxValue(workoutCombo, reader.GetInt32(0));
                        SetComboBoxValue(trainerCombo, reader.GetInt32(1));
                        datePicker.Value = DateTime.Parse(reader.GetString(2));
                        participantsNumeric.Value = reader.GetInt32(3);
                    }
                }
            }
        }

        private void SetComboBoxValue(ComboBox combo, int id)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Id == id)
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (workoutCombo.SelectedItem == null || trainerCombo.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                SQLiteCommand command;
                
                if (editId.HasValue)
                {
                    command = new SQLiteCommand(@"
                        UPDATE Schedule 
                        SET WorkoutId = @WorkoutId, 
                            TrainerId = @TrainerId, 
                            DateTime = @DateTime, 
                            MaxParticipants = @MaxParticipants
                        WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", editId.Value);
                }
                else
                {
                    command = new SQLiteCommand(@"
                        INSERT INTO Schedule 
                        (WorkoutId, TrainerId, DateTime, MaxParticipants)
                        VALUES (@WorkoutId, @TrainerId, @DateTime, @MaxParticipants)", 
                        connection);
                }

                command.Parameters.AddWithValue("@WorkoutId", 
                    ((ComboBoxItem)workoutCombo.SelectedItem).Id);
                command.Parameters.AddWithValue("@TrainerId", 
                    ((ComboBoxItem)trainerCombo.SelectedItem).Id);
                command.Parameters.AddWithValue("@DateTime", 
                    datePicker.Value.ToString("yyyy-MM-dd HH:mm"));
                command.Parameters.AddWithValue("@MaxParticipants", 
                    participantsNumeric.Value);

                command.ExecuteNonQuery();
            }
        }
    }

    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}