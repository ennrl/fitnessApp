using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
    public class SupplierDialog : Form
    {
        private TextBox txtName;
        private TextBox txtContact;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private Button btnSave;
        private Button btnCancel;
        public string SupplierName { get; private set; }
        public string Contact { get; private set; }
        public string Phone { get; private set; }
        public string Address { get; private set; }

        public SupplierDialog(string name = "", string contact = "", string phone = "", string address = "")
        {
            InitializeComponent();
            txtName.Text = name;
            txtContact.Text = contact;
            txtPhone.Text = phone;
            txtAddress.Text = address;
        }

        private void InitializeComponent()
        {
            this.Text = "Поставщик";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblName = new Label() { Text = "Название:", Left = 10, Top = 20 };
            txtName = new TextBox() { Left = 120, Top = 20, Width = 250 };

            var lblContact = new Label() { Text = "Контакт:", Left = 10, Top = 50 };
            txtContact = new TextBox() { Left = 120, Top = 50, Width = 250 };

            var lblPhone = new Label() { Text = "Телефон:", Left = 10, Top = 80 };
            txtPhone = new TextBox() { Left = 120, Top = 80, Width = 250 };

            var lblAddress = new Label() { Text = "Адрес:", Left = 10, Top = 110 };
            txtAddress = new TextBox() { Left = 120, Top = 110, Width = 250 };

            btnSave = new Button() { Text = "Сохранить", Left = 120, Top = 200 };
            btnCancel = new Button() { Text = "Отмена", Left = 220, Top = 200 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { 
                lblName, txtName, 
                lblContact, txtContact,
                lblPhone, txtPhone,
                lblAddress, txtAddress,
                btnSave, btnCancel
            });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Название поставщика обязательно для заполнения!");
                return;
            }

            SupplierName = txtName.Text;
            Contact = txtContact.Text;
            Phone = txtPhone.Text;
            Address = txtAddress.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public partial class SuppliersForm : Form
    {
        private DataGridView dataGridView1;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public SuppliersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Поставщики";
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
                Name = "Contact",
                DataPropertyName = "Contact",
                HeaderText = "Контакт",
                Width = 150
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                DataPropertyName = "Phone",
                HeaderText = "Телефон",
                Width = 150
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                DataPropertyName = "Address",
                HeaderText = "Адрес",
                Width = 200
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
            using (var conn = Database.GetConnection())
            {
                var adapter = new SQLiteDataAdapter("SELECT * FROM Suppliers", conn);
                var table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var dialog = new SupplierDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var conn = Database.GetConnection())
                {
                    var sql = @"INSERT INTO Suppliers (Name, Contact, Phone, Address) 
                               VALUES (@Name, @Contact, @Phone, @Address)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", dialog.SupplierName);
                        cmd.Parameters.AddWithValue("@Contact", dialog.Contact);
                        cmd.Parameters.AddWithValue("@Phone", dialog.Phone);
                        cmd.Parameters.AddWithValue("@Address", dialog.Address);
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
                var dialog = new SupplierDialog(
                    row.Cells["Name"].Value.ToString(),
                    row.Cells["Contact"].Value?.ToString() ?? "",
                    row.Cells["Phone"].Value?.ToString() ?? "",
                    row.Cells["Address"].Value?.ToString() ?? ""
                );

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var conn = Database.GetConnection())
                    {
                        var sql = @"UPDATE Suppliers 
                                   SET Name = @Name, Contact = @Contact, 
                                       Phone = @Phone, Address = @Address 
                                   WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", row.Cells["Id"].Value);
                            cmd.Parameters.AddWithValue("@Name", dialog.SupplierName);
                            cmd.Parameters.AddWithValue("@Contact", dialog.Contact);
                            cmd.Parameters.AddWithValue("@Phone", dialog.Phone);
                            cmd.Parameters.AddWithValue("@Address", dialog.Address);
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
                if (MessageBox.Show("Удалить выбранного поставщика?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = dataGridView1.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var conn = Database.GetConnection())
                    using (var cmd = new SQLiteCommand("DELETE FROM Suppliers WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                }
            }
        }
    }
}