using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Linq;

namespace ConstructionMaterialsManagement
{
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

        public OrderDialog(int? supplierId = null, DateTime? orderDate = null, string status = "Новый")
        {
            InitializeComponent();
            LoadSuppliers();
            LoadMaterials();

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
            this.Size = new System.Drawing.Size(800, 600);
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
                Width = 760, 
                Height = 380,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false
            };
            
            dgvMaterials.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 0) // MaterialId column
                {
                    var row = dgvMaterials.Rows[e.RowIndex];
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
            };

            btnSave = new Button() { Text = "Сохранить", Left = 300, Top = 520 };
            btnCancel = new Button() { Text = "Отмена", Left = 400, Top = 520 };

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

                var materialColumn = new DataGridViewComboBoxColumn()
                {
                    DataSource = materialsTable,
                    ValueMember = "Id",
                    DisplayMember = "Name",
                    DataPropertyName = "MaterialId",
                    HeaderText = "Материал",
                    Width = 200
                };

                dgvMaterials.Columns.Clear();
                dgvMaterials.Columns.Add(materialColumn);
                dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "Quantity",
                    HeaderText = "Количество",
                    Width = 100
                });
                dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "Price",
                    HeaderText = "Цена",
                    Width = 100,
                    ReadOnly = true
                });
                dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "Total",
                    HeaderText = "Сумма",
                    Width = 100,
                    ReadOnly = true
                });

                dgvMaterials.DataError += (s, e) => { e.ThrowException = false; };
            }
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
                
                foreach (DataRow row in OrderDetails.Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        if (row["MaterialId"] == DBNull.Value || 
                            row["Quantity"] == DBNull.Value ||
                            Convert.ToInt32(row["Quantity"]) <= 0)
                        {
                            MessageBox.Show("Заполните все данные в таблице материалов корректно!", 
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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