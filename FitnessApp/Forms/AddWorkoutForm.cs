using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace FitnessApp
{
    public class AddWorkoutForm : Form
    {
        private TextBox nameBox;
        private TextBox descriptionBox;
        private Button saveButton;
        private Button cancelButton;
        private int? editId;

        public AddWorkoutForm(int? id = null)
        {
            editId = id;
            InitializeComponents();
            if (editId.HasValue)
            {
                LoadWorkoutData();
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

            var nameLabel = new Label
            {
                Text = "Название:",
                Location = new System.Drawing.Point(10, 10)
            };

            nameBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 30),
                Width = 360
            };

            var descriptionLabel = new Label
            {
                Text = "Описание:",
                Location = new System.Drawing.Point(10, 60)
            };

            descriptionBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 80),
                Width = 360,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
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
                nameLabel, nameBox,
                descriptionLabel, descriptionBox,
                saveButton, cancelButton 
            });
        }

        private void LoadWorkoutData()
        {
            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Name, Description FROM Workouts WHERE Id = @Id", 
                    connection);
                command.Parameters.AddWithValue("@Id", editId.Value);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nameBox.Text = reader.GetString(0);
                        descriptionBox.Text = reader.GetString(1);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Введите название тренировки!");
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=fitness.db;Version=3;"))
            {
                connection.Open();
                SQLiteCommand command;
                
                if (editId.HasValue)
                {
                    command = new SQLiteCommand(@"
                        UPDATE Workouts 
                        SET Name = @Name, 
                            Description = @Description
                        WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", editId.Value);
                }
                else
                {
                    command = new SQLiteCommand(@"
                        INSERT INTO Workouts 
                        (Name, Description)
                        VALUES (@Name, @Description)", 
                        connection);
                }

                command.Parameters.AddWithValue("@Name", nameBox.Text);
                command.Parameters.AddWithValue("@Description", descriptionBox.Text);

                command.ExecuteNonQuery();
            }
        }
    }
}