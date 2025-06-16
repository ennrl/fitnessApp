using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class TrainersForm : Form
    {
        private DataGridView trainersGrid;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;

        public TrainersForm()
        {
            InitializeComponents();
            LoadTrainersData();
        }

        private void InitializeComponents()
        {
            this.Text = "Тренеры";
            this.Size = new System.Drawing.Size(800, 600);

            trainersGrid = new DataGridView
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
            this.Controls.AddRange(new Control[] { trainersGrid, buttonPanel });
        }

        private void LoadTrainersData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Id, Name, Specialization FROM Trainers", connection);
                var adapter = new SQLiteDataAdapter(command);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                trainersGrid.DataSource = table;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddTrainerForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadTrainersData();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (trainersGrid.SelectedRows.Count > 0)
            {
                var id = trainersGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                var editForm = new AddTrainerForm(int.Parse(id));
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTrainersData();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (trainersGrid.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранного тренера?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = trainersGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(
                            "DELETE FROM Trainers WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                    LoadTrainersData();
                }
            }
        }
    }
}