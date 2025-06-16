using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class ClientsForm : Form
    {
        private DataGridView clientsGrid;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private TextBox searchBox;

        public ClientsForm()
        {
            InitializeComponents();
            LoadClientsData();
        }

        private void InitializeComponents()
        {
            this.Text = "Клиенты";
            this.Size = new System.Drawing.Size(800, 600);

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            searchBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 200,
                PlaceholderText = "Поиск по имени..."
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchPanel.Controls.Add(searchBox);

            clientsGrid = new DataGridView
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
            this.Controls.AddRange(new Control[] { searchPanel, clientsGrid, buttonPanel });
        }

        private void LoadClientsData(string searchTerm = "")
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Id, Name, Phone, Email FROM Clients " +
                    (string.IsNullOrEmpty(searchTerm) ? "" : "WHERE Name LIKE @Search"),
                    connection);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    command.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                }

                var adapter = new SQLiteDataAdapter(command);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                clientsGrid.DataSource = table;
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            LoadClientsData(searchBox.Text);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddClientForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadClientsData(searchBox.Text);
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (clientsGrid.SelectedRows.Count > 0)
            {
                var id = clientsGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                var editForm = new AddClientForm(int.Parse(id));
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadClientsData(searchBox.Text);
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (clientsGrid.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = clientsGrid.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(
                            "DELETE FROM Clients WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                    LoadClientsData(searchBox.Text);
                }
            }
        }
    }
}