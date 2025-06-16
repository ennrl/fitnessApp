using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

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

        public OrderDialog()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadMaterials();
            dtpOrderDate.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
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

            dgvMaterials = new DataGridView()
            {
                Left = 10,
                Top = 110,
                Width = 760,
                Height = 380,
                AllowUserToAddRows = true,
                AutoGenerateColumns = false
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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaterials()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    var adapter = new SQLiteDataAdapter("SELECT Id, Name, Price FROM Materials", conn);
                    materialsTable = new DataTable();
                    adapter.Fill(materialsTable);

                    OrderDetails = new DataTable();
                    OrderDetails.Columns.Add("MaterialId", typeof(int));
                    OrderDetails.Columns.Add("Quantity", typeof(int));
                    OrderDetails.Columns.Add("Price", typeof(decimal));
                    OrderDetails.Columns.Add("Total", typeof(decimal), "Quantity * Price");

                    OrderDetails.TableNewRow += (s, e) => {
                        e.Row["Quantity"] = 1;
                    };

                    var materialColumn = new DataGridViewComboBoxColumn()
                    {
                        DataSource = materialsTable,
                        ValueMember = "Id",
                        DisplayMember = "Name",
                        DataPropertyName = "MaterialId",
                        HeaderText = "Материал",
                        Width = 300
                    };

                    dgvMaterials.Columns.Clear();
                    dgvMaterials.Columns.Add(materialColumn);
                    dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "Quantity",
                        DataPropertyName = "Quantity",
                        HeaderText = "Количество",
                        Width = 100,
                        DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
                    });
                    dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "Price",
                        DataPropertyName = "Price",
                        HeaderText = "Цена",
                        Width = 100,
                        DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                        ReadOnly = true
                    });
                    dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "Total",
                        DataPropertyName = "Total",
                        HeaderText = "Сумма",
                        Width = 100,
                        DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                        ReadOnly = true
                    });

                    dgvMaterials.DataSource = OrderDetails;

                    dgvMaterials.CellValueChanged += (s, e) =>
                    {
                        if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                        {
                            var row = dgvMaterials.Rows[e.RowIndex];
                            if (row.Cells[0].Value != null && row.Cells[0].Value != DBNull.Value)
                            {
                                int materialId;
                                if (int.TryParse(row.Cells[0].Value.ToString(), out materialId))
                                {
                                    var material = materialsTable.Select($"Id = {materialId}").FirstOrDefault();
                                    if (material != null)
                                    {
                                        row.Cells["Price"].Value = material["Price"];
                                    }
                                }
                            }
                        }
                    };

                    dgvMaterials.DataError += (s, e) => { e.ThrowException = false; };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки материалов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            try
            {
                foreach (DataRow row in OrderDetails.Rows)
                {
                    if (row.RowState != DataRowState.Deleted && 
                        (row["MaterialId"] == DBNull.Value || 
                         row["Quantity"] == DBNull.Value ||
                         Convert.ToInt32(row["Quantity"]) <= 0))
                    {
                        MessageBox.Show("Заполните все данные в таблице материалов корректно!", 
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                SupplierId = Convert.ToInt32(cmbSupplier.SelectedValue);
                OrderDate = dtpOrderDate.Value;
                Status = cmbStatus.Text;

                OrderDetails.AcceptChanges();
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
            try
            {
                using (var conn = Database.GetConnection())
                {
                    // Загружаем основные данные заказа
                    using (var cmd = new SQLiteCommand(
                        "SELECT SupplierId, OrderDate, Status FROM Orders WHERE Id = @OrderId", conn))
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
                    OrderDetails.Clear();
                    using (var cmd = new SQLiteCommand(
                        "SELECT MaterialId, Quantity, Price FROM OrderDetails WHERE OrderId = @OrderId", conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = OrderDetails.NewRow();
                                row["MaterialId"] = reader["MaterialId"];
                                row["Quantity"] = reader["Quantity"];
                                row["Price"] = reader["Price"];
                                OrderDetails.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}