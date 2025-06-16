using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class AddClientForm : Form
    {
        private TextBox nameBox;
        private TextBox phoneBox;
        private TextBox emailBox;
        private Button saveButton;
        private Button cancelButton;
        private int? editId;

        public AddClientForm(int? id = null)
        {
            editId = id;
            InitializeComponents();
            if (editId.HasValue)
            {
                LoadClientData();
            }
        }

        private void InitializeComponents()
        {
            this.Text = editId.HasValue ? "Изменить клиента" : "Добавить клиента";
            this.Size = new System.Drawing.Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var nameLabel = new Label
            {
                Text = "ФИО:",
                Location = new System.Drawing.Point(10, 10)
            };

            nameBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 30),
                Width = 360
            };

            var phoneLabel = new Label
            {
                Text = "Телефон:",
                Location = new System.Drawing.Point(10, 60)
            };

            phoneBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 80),
                Width = 360
            };

            var emailLabel = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(10, 110)
            };

            emailBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 130),
                Width = 360
            };

            saveButton = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(200, 170)
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(290, 170)
            };

            this.Controls.AddRange(new Control[] { 
                nameLabel, nameBox,
                phoneLabel, phoneBox,
                emailLabel, emailBox,
                saveButton, cancelButton 
            });
        }

        private void LoadClientData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Name, Phone, Email FROM Clients WHERE Id = @Id", 
                    connection);
                command.Parameters.AddWithValue("@Id", editId.Value);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nameBox.Text = reader.GetString(0);
                        phoneBox.Text = reader.GetString(1);
                        emailBox.Text = reader.GetString(2);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Введите ФИО клиента!");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                SQLiteCommand command;
                
                if (editId.HasValue)
                {
                    command = new SQLiteCommand(@"
                        UPDATE Clients 
                        SET Name = @Name, 
                            Phone = @Phone, 
                            Email = @Email
                        WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", editId.Value);
                }
                else
                {
                    command = new SQLiteCommand(@"
                        INSERT INTO Clients 
                        (Name, Phone, Email)
                        VALUES (@Name, @Phone, @Email)", 
                        connection);
                }

                command.Parameters.AddWithValue("@Name", nameBox.Text);
                command.Parameters.AddWithValue("@Phone", phoneBox.Text);
                command.Parameters.AddWithValue("@Email", emailBox.Text);

                command.ExecuteNonQuery();
            }
        }
    }
}