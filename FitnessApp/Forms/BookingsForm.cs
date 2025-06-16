using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class BookingsForm : Form
    {
        private DataGridView bookingsGrid;
        private Button addButton;
        private Button deleteButton;
        private TextBox searchBox;

        public BookingsForm()
        {
            InitializeComponents();
            LoadBookingsData();
        }

        private void InitializeComponents()
        {
            this.Text = "Записи на тренировки";
            this.Size = new System.Drawing.Size(800, 600);

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            searchBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 200,
                PlaceholderText = "Поиск по клиенту..."
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchPanel.Controls.Add(searchBox);

            bookingsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };

            addButton = new Button
            {
                Text = "Добавить запись",
                Location = new System.Drawing.Point(10, 10)
            };
            addButton.Click += AddButton_Click;

            deleteButton = new Button
            {
                Text = "Отменить запись",
                Location = new System.Drawing.Point(120, 10)
            };
            deleteButton.Click += DeleteButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { addButton, deleteButton });
            this.Controls.AddRange(new Control[] { searchPanel, bookingsGrid, buttonPanel });
        }

        private void LoadBookingsData(string searchTerm = "")
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    @"SELECT Bookings.Id, 
                      Clients.Name as ClientName,
                      Workouts.Name as WorkoutName,
                      Trainers.Name as TrainerName,
                      Schedule.DateTime,
                      Bookings.BookingDate
                      FROM Bookings 
                      JOIN Clients ON Bookings.ClientId = Clients.Id
                      JOIN Schedule ON Bookings.ScheduleId = Schedule.Id
                      JOIN Workouts ON Schedule.WorkoutId = Workouts.Id
                      JOIN Trainers ON Schedule.TrainerId = Trainers.Id" +
                    (string.IsNullOrEmpty(searchTerm) ? "" : " WHERE Clients.Name LIKE @Search"),
                    connection);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    command.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                }

                var adapter = new SQLiteDataAdapter(command);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                bookingsGrid.DataSource = table;
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            LoadBookingsData(searchBox.Text);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddBookingForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadBookingsData(searchBox.Text);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (bookingsGrid.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Отменить выбранную запись?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = bookingsGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(
                            "DELETE FROM Bookings WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                    LoadBookingsData(searchBox.Text);
                }
            }
        }
    }
}