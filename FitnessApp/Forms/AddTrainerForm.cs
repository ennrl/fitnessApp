using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class AddTrainerForm : Form
    {
        private TextBox nameBox;
        private TextBox specializationBox;
        private Button saveButton;
        private Button cancelButton;
        private int? editId;

        public AddTrainerForm(int? id = null)
        {
            editId = id;
            InitializeComponents();
            if (editId.HasValue)
            {
                LoadTrainerData();
            }
        }

        private void InitializeComponents()
        {
            this.Text = editId.HasValue ? "Изменить тренера" : "Добавить тренера";
            this.Size = new System.Drawing.Size(400, 200);
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

            var specializationLabel = new Label
            {
                Text = "Специализация:",
                Location = new System.Drawing.Point(10, 60)
            };

            specializationBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 80),
                Width = 360
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
                nameLabel, nameBox,
                specializationLabel, specializationBox,
                saveButton, cancelButton 
            });
        }

        private void LoadTrainerData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Name, Specialization FROM Trainers WHERE Id = @Id", 
                    connection);
                command.Parameters.AddWithValue("@Id", editId.Value);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nameBox.Text = reader.GetString(0);
                        specializationBox.Text = reader.GetString(1);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Введите ФИО тренера!");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                SQLiteCommand command;
                
                if (editId.HasValue)
                {
                    command = new SQLiteCommand(@"
                        UPDATE Trainers 
                        SET Name = @Name, 
                            Specialization = @Specialization
                        WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", editId.Value);
                }
                else
                {
                    command = new SQLiteCommand(@"
                        INSERT INTO Trainers 
                        (Name, Specialization)
                        VALUES (@Name, @Specialization)", 
                        connection);
                }

                command.Parameters.AddWithValue("@Name", nameBox.Text);
                command.Parameters.AddWithValue("@Specialization", specializationBox.Text);

                command.ExecuteNonQuery();
            }
        }
    }
}