using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
    public partial class MaterialsForm : Form
    {
        private DataGridView dataGridView1;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public MaterialsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Материалы";
            this.Size = new System.Drawing.Size(800, 600);

            dataGridView1 = new DataGridView();
            dataGridView1.Dock = DockStyle.Top;
            dataGridView1.Height = 500;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoGenerateColumns = false;
            
            // Настройка колонок
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = 50
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                DataPropertyName = "Name",
                HeaderText = "Название",
                Width = 200
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                DataPropertyName = "Description",
                HeaderText = "Описание",
                Width = 200
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unit",
                DataPropertyName = "Unit",
                HeaderText = "Ед. изм.",
                Width = 100
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                DataPropertyName = "Price",
                HeaderText = "Цена",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            // Добавляем обработчик ошибок
            dataGridView1.DataError += (s, e) => 
            {
                e.ThrowException = false;
            };

            this.Controls.Add(dataGridView1);

            var panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 50;

            btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Left = 10;
            btnAdd.Top = 10;
            btnAdd.Click += BtnAdd_Click;
            panel.Controls.Add(btnAdd);

            btnEdit = new Button();
            btnEdit.Text = "Изменить";
            btnEdit.Left = btnAdd.Right + 10;
            btnEdit.Top = 10;
            btnEdit.Click += BtnEdit_Click;
            panel.Controls.Add(btnEdit);

            btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Left = btnEdit.Right + 10;
            btnDelete.Top = 10;
            btnDelete.Click += BtnDelete_Click;
            panel.Controls.Add(btnDelete);

            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            try 
            {
                using (var conn = Database.GetConnection())
                {
                    var adapter = new SQLiteDataAdapter("SELECT Id, Name, Description, Unit, Price FROM Materials", conn);
                    var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Description", typeof(string));
                    table.Columns.Add("Unit", typeof(string));
                    table.Columns.Add("Price", typeof(decimal));
                    adapter.Fill(table);
                    
                    var bs = new BindingSource();
                    bs.DataSource = table;
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = bs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var dialog = new MaterialDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var conn = Database.GetConnection())
                {
                    var sql = @"INSERT INTO Materials (Name, Description, Unit, Price) 
                               VALUES (@Name, @Description, @Unit, @Price)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", dialog.MaterialName);
                        cmd.Parameters.AddWithValue("@Description", dialog.Description);
                        cmd.Parameters.AddWithValue("@Unit", dialog.Unit);
                        cmd.Parameters.AddWithValue("@Price", dialog.Price);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                var dialog = new MaterialDialog(
                    row.Cells["Name"].Value.ToString(),
                    row.Cells["Description"].Value?.ToString() ?? "",
                    row.Cells["Unit"].Value?.ToString() ?? "",
                    Convert.ToDecimal(row.Cells["Price"].Value)
                );

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var conn = Database.GetConnection())
                    {
                        var sql = @"UPDATE Materials 
                                   SET Name = @Name, Description = @Description, 
                                       Unit = @Unit, Price = @Price 
                                   WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", row.Cells["Id"].Value);
                            cmd.Parameters.AddWithValue("@Name", dialog.MaterialName);
                            cmd.Parameters.AddWithValue("@Description", dialog.Description);
                            cmd.Parameters.AddWithValue("@Unit", dialog.Unit);
                            cmd.Parameters.AddWithValue("@Price", dialog.Price);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранный материал?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = dataGridView1.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var conn = Database.GetConnection())
                    using (var cmd = new SQLiteCommand("DELETE FROM Materials WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                }
            }
        }
    }

    public class MaterialDialog : Form
    {
        private TextBox txtName;
        private TextBox txtDescription;
        private TextBox txtUnit;
        private NumericUpDown numPrice;
        private Button btnSave;
        private Button btnCancel;
        
        public string MaterialName { get; private set; }
        public string Description { get; private set; }
        public string Unit { get; private set; }
        public decimal Price { get; private set; }

        public MaterialDialog(string name = "", string description = "", string unit = "", decimal price = 0)
        {
            InitializeComponent();
            txtName.Text = name;
            txtDescription.Text = description;
            txtUnit.Text = unit;
            numPrice.Value = price;
        }

        private void InitializeComponent()
        {
            this.Text = "Материал";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblName = new Label() { Text = "Название:", Left = 10, Top = 20 };
            txtName = new TextBox() { Left = 120, Top = 20, Width = 250 };

            var lblDescription = new Label() { Text = "Описание:", Left = 10, Top = 50 };
            txtDescription = new TextBox() { Left = 120, Top = 50, Width = 250, Multiline = true, Height = 60 };

            var lblUnit = new Label() { Text = "Ед. измерения:", Left = 10, Top = 120 };
            txtUnit = new TextBox() { Left = 120, Top = 120, Width = 250 };

            var lblPrice = new Label() { Text = "Цена:", Left = 10, Top = 150 };
            numPrice = new NumericUpDown() { Left = 120, Top = 150, Width = 250, DecimalPlaces = 2, Maximum = 1000000 };

            btnSave = new Button() { Text = "Сохранить", Left = 120, Top = 200 };
            btnCancel = new Button() { Text = "Отмена", Left = 220, Top = 200 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { 
                lblName, txtName,
                lblDescription, txtDescription,
                lblUnit, txtUnit,
                lblPrice, numPrice,
                btnSave, btnCancel
            });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название материала!");
                return;
            }

            MaterialName = txtName.Text;
            Description = txtDescription.Text;
            Unit = txtUnit.Text;
            Price = numPrice.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}