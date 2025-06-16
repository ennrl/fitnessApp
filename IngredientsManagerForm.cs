using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SmartKitchenAssistant
{
    public partial class IngredientsManagerForm : Form
    {
        private DataGridView dgvIngredients;

        public IngredientsManagerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление ингредиентами";
            this.Size = new System.Drawing.Size(600, 500);

            // Создаем DataGridView для отображения ингредиентов
            dgvIngredients = new DataGridView
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(540, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };

            // Добавляем кнопки
            Button btnAdd = new Button
            {
                Text = "Добавить",
                Location = new System.Drawing.Point(20, 390),
                Size = new System.Drawing.Size(120, 30)
            };
            btnAdd.Click += BtnAdd_Click;

            Button btnEdit = new Button
            {
                Text = "Изменить",
                Location = new System.Drawing.Point(150, 390),
                Size = new System.Drawing.Size(120, 30)
            };
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = new Button
            {
                Text = "Удалить",
                Location = new System.Drawing.Point(280, 390),
                Size = new System.Drawing.Size(120, 30)
            };
            btnDelete.Click += BtnDelete_Click;

            // Добавляем элементы на форму
            this.Controls.AddRange(new Control[] { dgvIngredients, btnAdd, btnEdit, btnDelete });

            LoadIngredients();
        }

        private void LoadIngredients()
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var adapter = new SQLiteDataAdapter("SELECT Id, Name, Category, Unit FROM Ingredients", conn))
                {
                    var table = new System.Data.DataTable();
                    adapter.Fill(table);
                    dgvIngredients.DataSource = table;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new IngredientEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadIngredients();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvIngredients.CurrentRow != null)
            {
                int id = Convert.ToInt32(dgvIngredients.CurrentRow.Cells["Id"].Value);
                using (var form = new IngredientEditForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadIngredients();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvIngredients.CurrentRow != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот ингредиент?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgvIngredients.CurrentRow.Cells["Id"].Value);
                    DeleteIngredient(id);
                    LoadIngredients();
                }
            }
        }

        private void DeleteIngredient(int id)
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Ingredients WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class IngredientEditForm : Form
    {
        private TextBox txtName;
        private TextBox txtCategory;
        private TextBox txtUnit;
        private int? ingredientId;

        public IngredientEditForm(int? id = null)
        {
            ingredientId = id;
            InitializeComponent();
            if (id.HasValue)
            {
                LoadIngredient(id.Value);
            }
        }

        private void InitializeComponent()
        {
            this.Text = ingredientId.HasValue ? "Изменить ингредиент" : "Добавить ингредиент";
            this.Size = new System.Drawing.Size(400, 250);

            // Название
            Label lblName = new Label
            {
                Text = "Название:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(100, 20)
            };

            txtName = new TextBox
            {
                Location = new System.Drawing.Point(130, 20),
                Size = new System.Drawing.Size(200, 20)
            };

            // Категория
            Label lblCategory = new Label
            {
                Text = "Категория:",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(100, 20)
            };

            txtCategory = new TextBox
            {
                Location = new System.Drawing.Point(130, 60),
                Size = new System.Drawing.Size(200, 20)
            };

            // Единица измерения
            Label lblUnit = new Label
            {
                Text = "Ед. измерения:",
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(100, 20)
            };

            txtUnit = new TextBox
            {
                Location = new System.Drawing.Point(130, 100),
                Size = new System.Drawing.Size(200, 20)
            };

            // Кнопки
            Button btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(130, 150),
                Size = new System.Drawing.Size(90, 30)
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(240, 150),
                Size = new System.Drawing.Size(90, 30)
            };

            // Добавляем элементы на форму
            this.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblCategory, txtCategory,
                lblUnit, txtUnit,
                btnSave, btnCancel
            });
        }

        private void LoadIngredient(int id)
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Name, Category, Unit FROM Ingredients WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtName.Text = reader.GetString(0);
                            txtCategory.Text = reader.GetString(1);
                            txtUnit.Text = reader.GetString(2);
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection($"Data Source=kitchen_assistant.db;Version=3;"))
            {
                conn.Open();
                string sql = ingredientId.HasValue
                    ? "UPDATE Ingredients SET Name = @name, Category = @category, Unit = @unit WHERE Id = @id"
                    : "INSERT INTO Ingredients (Name, Category, Unit) VALUES (@name, @category, @unit)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@unit", txtUnit.Text);
                    if (ingredientId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@id", ingredientId.Value);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}