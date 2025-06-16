using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
    public partial class OrdersForm : Form
    {
        private DataGridView dataGridView1;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnViewDetails;

        public OrdersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заказы";
            this.Size = new System.Drawing.Size(1000, 600);

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
                Name = "SupplierName",
                DataPropertyName = "SupplierName",
                HeaderText = "Поставщик",
                Width = 200
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderDate",
                DataPropertyName = "OrderDate",
                HeaderText = "Дата заказа",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "d" }
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Статус",
                Width = 150
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
            btnAdd.Text = "Новый заказ";
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

            btnViewDetails = new Button();
            btnViewDetails.Text = "Детали заказа";
            btnViewDetails.Left = btnDelete.Right + 10;
            btnViewDetails.Top = 10;
            btnViewDetails.Click += BtnViewDetails_Click;
            panel.Controls.Add(btnViewDetails);

            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            try 
            {
                using (var conn = Database.GetConnection())
                {
                    var sql = @"SELECT Orders.Id, Orders.OrderDate, Orders.Status, 
                               Suppliers.Name as SupplierName
                               FROM Orders 
                               LEFT JOIN Suppliers ON Orders.SupplierId = Suppliers.Id";
                    var adapter = new SQLiteDataAdapter(sql, conn);
                    var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("SupplierName", typeof(string));
                    table.Columns.Add("OrderDate", typeof(DateTime));
                    table.Columns.Add("Status", typeof(string));
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
            using (var dialog = new OrderDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var conn = Database.GetConnection())
                    {
                        using (var cmd = new SQLiteCommand("INSERT INTO Orders (SupplierId, OrderDate, Status) VALUES (@SupplierId, @OrderDate, @Status)", conn))
                        {
                            cmd.Parameters.AddWithValue("@SupplierId", dialog.SupplierId);
                            cmd.Parameters.AddWithValue("@OrderDate", dialog.OrderDate);
                            cmd.Parameters.AddWithValue("@Status", dialog.Status);
                            cmd.ExecuteNonQuery();
                        }

                        var orderId = conn.LastInsertRowId;

                        foreach (DataRow row in dialog.OrderDetails.Rows)
                        {
                            using (var cmd = new SQLiteCommand("INSERT INTO OrderDetails (OrderId, MaterialId, Quantity, Price) VALUES (@OrderId, @MaterialId, @Quantity, @Price)", conn))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", orderId);
                                cmd.Parameters.AddWithValue("@MaterialId", row["MaterialId"]);
                                cmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                                cmd.Parameters.AddWithValue("@Price", row["Price"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    LoadData();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var id = (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
                using (var dialog = new OrderDialog())
                {
                    dialog.LoadOrder(id);
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        using (var conn = Database.GetConnection())
                        {
                            using (var cmd = new SQLiteCommand("UPDATE Orders SET SupplierId = @SupplierId, OrderDate = @OrderDate, Status = @Status WHERE Id = @Id", conn))
                            {
                                cmd.Parameters.AddWithValue("@SupplierId", dialog.SupplierId);
                                cmd.Parameters.AddWithValue("@OrderDate", dialog.OrderDate);
                                cmd.Parameters.AddWithValue("@Status", dialog.Status);
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SQLiteCommand("DELETE FROM OrderDetails WHERE OrderId = @OrderId", conn))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", id);
                                cmd.ExecuteNonQuery();
                            }

                            foreach (DataRow row in dialog.OrderDetails.Rows)
                            {
                                using (var cmd = new SQLiteCommand("INSERT INTO OrderDetails (OrderId, MaterialId, Quantity, Price) VALUES (@OrderId, @MaterialId, @Quantity, @Price)", conn))
                                {
                                    cmd.Parameters.AddWithValue("@OrderId", id);
                                    cmd.Parameters.AddWithValue("@MaterialId", row["MaterialId"]);
                                    cmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                                    cmd.Parameters.AddWithValue("@Price", row["Price"]);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        LoadData();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранный заказ?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = dataGridView1.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var conn = Database.GetConnection())
                    {
                        using (var cmd = new SQLiteCommand("DELETE FROM OrderDetails WHERE OrderId = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SQLiteCommand("DELETE FROM Orders WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadData();
                }
            }
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var orderId = (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
                using (var detailsForm = new OrderDetailsForm(orderId))
                {
                    detailsForm.ShowDialog();
                }
            }
        }
    }

    public class OrderDialog : Form
    {
        private ComboBox cmbSupplier;
        private DateTimePicker dtpOrderDate;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;
        private DataGridView dgvMaterials;
        private DataTable materialsTable;
        
        public int? SupplierId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public string Status { get; private set; }
        public DataTable OrderDetails { get; private set; }
        private ComboBox cmbSupplier;
        private DateTimePicker dtpOrderDate;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;
        private DataGridView dgvMaterials;
        public int? SupplierId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public string Status { get; private set; }
        public DataTable OrderDetails { get; private set; }

        private DataTable materialsTable; // Добавить поле для хранения материалов

        public OrderDialog(int? supplierId = null, DateTime? orderDate = null, string status = "Новый")
        {
            InitializeComponent();
            LoadSuppliers();
            LoadMaterials();

            // Добавляем обработчик ошибок
            dgvMaterials.DataError += (s, e) => 
            {
                e.ThrowException = false;
            };

            if (supplierId.HasValue)
            {
                cmbSupplier.SelectedValue = supplierId;
                dtpOrderDate.Value = orderDate ?? DateTime.Now;
                cmbStatus.Text = status;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Заказ";
            this.Size = new System.Drawing.Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblSupplier = new Label() { Text = "Поставщик:", Left = 10, Top = 20 };
            cmbSupplier = new ComboBox() { Left = 120, Top = 20, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblDate = new Label() { Text = "Дата:", Left = 10, Top = 50 };
            dtpOrderDate = new DateTimePicker() { Left = 120, Top = 50, Width = 250 };

            var lblStatus = new Label() { Text = "Статус:", Left = 10, Top = 80 };
            cmbStatus = new ComboBox() { Left = 120, Top = 80, Width = 250 };
            cmbStatus.Items.AddRange(new string[] { "Новый", "В обработке", "Выполнен", "Отменен" });
            cmbStatus.SelectedIndex = 0;

            dgvMaterials = new DataGridView() 
            { 
                Left = 10, 
                Top = 110, 
                Width = 560, 
                Height = 300,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false
            };
            
            dgvMaterials.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var row = dgvMaterials.Rows[e.RowIndex];
                    if (e.ColumnIndex == 0) // MaterialId column
                    {
                        if (row.Cells[0].Value != null)
                        {
                            var materialId = Convert.ToInt32(row.Cells[0].Value);
                            var material = materialsTable.Select($"Id = {materialId}").FirstOrDefault();
                            if (material != null)
                            {
                                row.Cells["Price"].Value = material["Price"];
                            }
                        }
                    }
                }
            };

            btnSave = new Button() { Text = "Сохранить", Left = 200, Top = 420 };
            btnCancel = new Button() { Text = "Отмена", Left = 300, Top = 420 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { 
                lblSupplier, cmbSupplier,
                lblDate, dtpOrderDate,
                lblStatus, cmbStatus,
                dgvMaterials,
                btnSave, btnCancel
            });
        }

        private void LoadSuppliers()
        {
            using (var conn = Database.GetConnection())
            {
                var adapter = new SQLiteDataAdapter("SELECT Id, Name FROM Suppliers", conn);
                var table = new DataTable();
                adapter.Fill(table);
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "Id";
                cmbSupplier.DataSource = table;
            }
        }

        private void LoadMaterials()
        {
            using (var conn = Database.GetConnection())
            {
                var adapter = new SQLiteDataAdapter("SELECT Id, Name, Price FROM Materials", conn);
                materialsTable = new DataTable();
                adapter.Fill(materialsTable);

                OrderDetails = new DataTable();
                OrderDetails.Columns.Add("MaterialId", typeof(int));
                OrderDetails.Columns.Add("MaterialName", typeof(string));
                OrderDetails.Columns.Add("Quantity", typeof(int));
                OrderDetails.Columns.Add("Price", typeof(decimal));
                OrderDetails.Columns.Add("Total", typeof(decimal), "Quantity * Price");

                dgvMaterials.DataSource = OrderDetails;
                dgvMaterials.Columns.Clear();

                var materialColumn = new DataGridViewComboBoxColumn()
                {
                    DataSource = materialsTable,
                    ValueMember = "Id",
                    DisplayMember = "Name",
                    DataPropertyName = "MaterialId",
                    HeaderText = "Материал",
                    Width = 200
                };
                dgvMaterials.Columns.Add(materialColumn);
                dgvMaterials.Columns.Add("Quantity", "Количество");
                dgvMaterials.Columns.Add("Price", "Цена");
                dgvMaterials.Columns.Add("Total", "Сумма");
            }
            // Добавляем обработчик ошибок
            dgvMaterials.DataError += (s, e) => { e.ThrowException = false; };
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            if (OrderDetails.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте материалы в заказ!");
                return;
            }

            try
            {
                SupplierId = Convert.ToInt32(cmbSupplier.SelectedValue);
                OrderDate = dtpOrderDate.Value;
                Status = cmbStatus.Text;
                
                // Проверяем корректность данных в OrderDetails
                foreach (DataRow row in OrderDetails.Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        if (row["MaterialId"] == DBNull.Value || row["Quantity"] == DBNull.Value)
                        {
                            MessageBox.Show("Заполните все данные в таблице материалов!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void LoadOrder(int orderId)
        {
            using (var conn = Database.GetConnection())
            {
                // Загружаем основные данные заказа
                var orderSql = @"SELECT SupplierId, OrderDate, Status 
                                FROM Orders WHERE Id = @OrderId";
                using (var cmd = new SQLiteCommand(orderSql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cmbSupplier.SelectedValue = reader["SupplierId"];
                            dtpOrderDate.Value = Convert.ToDateTime(reader["OrderDate"]);
                            cmbStatus.Text = reader["Status"].ToString();
                        }
                    }
                }

                // Загружаем детали заказа
                var detailsSql = @"SELECT MaterialId, Quantity, Price
                                 FROM OrderDetails 
                                 WHERE OrderId = @OrderId";
                
                var adapter = new SQLiteDataAdapter(detailsSql, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@OrderId", orderId);

                var detailsTable = new DataTable();
                adapter.Fill(detailsTable);

                OrderDetails.Clear();
                foreach (DataRow detailRow in detailsTable.Rows)
                {
                    var newRow = OrderDetails.NewRow();
                    newRow["MaterialId"] = detailRow["MaterialId"];
                    newRow["Quantity"] = detailRow["Quantity"];
                    newRow["Price"] = detailRow["Price"];
                    OrderDetails.Rows.Add(newRow);

                }
                }
            }
        }
    }

    public class OrderDetailsForm : Form
    {
        private DataGridView dataGridView1;
        private int orderId;

        public OrderDetailsForm(int orderId)
        {
            this.orderId = orderId;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Детали заказа";
            this.Size = new System.Drawing.Size(800, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            dataGridView1 = new DataGridView();
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            this.Controls.Add(dataGridView1);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                var sql = @"SELECT m.Name as MaterialName, od.Quantity, od.Price, 
                           (od.Quantity * od.Price) as Total
                           FROM OrderDetails od
                           JOIN Materials m ON od.MaterialId = m.Id
                           WHERE od.OrderId = @OrderId";
                
                var adapter = new SQLiteDataAdapter(sql, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@OrderId", orderId);
                var table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }
    }
}