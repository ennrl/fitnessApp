using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class ScheduleForm : Form
    {
        private DataGridView scheduleGrid;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;

        public ScheduleForm()
        {
            InitializeComponents();
            LoadScheduleData();
        }

        private void InitializeComponents()
        {
            this.Text = "Расписание тренировок";
            this.Size = new System.Drawing.Size(800, 600);

            scheduleGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };

            addButton = new Button
            {
                Text = "Добавить",
                Location = new System.Drawing.Point(10, 10)
            };
            addButton.Click += AddButton_Click;

            editButton = new Button
            {
                Text = "Изменить",
                Location = new System.Drawing.Point(100, 10)
            };
            editButton.Click += EditButton_Click;

            deleteButton = new Button
            {
                Text = "Удалить",
                Location = new System.Drawing.Point(190, 10)
            };
            deleteButton.Click += DeleteButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { addButton, editButton, deleteButton });
            this.Controls.AddRange(new Control[] { scheduleGrid, buttonPanel });
        }

        private void LoadScheduleData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    @"SELECT Schedule.Id, Workouts.Name as WorkoutName, 
                      Trainers.Name as TrainerName, Schedule.DateTime, 
                      Schedule.MaxParticipants
                      FROM Schedule 
                      JOIN Workouts ON Schedule.WorkoutId = Workouts.Id
                      JOIN Trainers ON Schedule.TrainerId = Trainers.Id", connection);

                var adapter = new SQLiteDataAdapter(command);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                scheduleGrid.DataSource = table;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddScheduleForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadScheduleData();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (scheduleGrid.SelectedRows.Count > 0)
            {
                var id = scheduleGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                var editForm = new AddScheduleForm(int.Parse(id));
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadScheduleData();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (scheduleGrid.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = scheduleGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(
                            "DELETE FROM Schedule WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                    LoadScheduleData();
                }
            }
        }
    }
}