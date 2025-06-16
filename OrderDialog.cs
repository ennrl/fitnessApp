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
                OrderDetails.Columns.Add("Quantity", typeof(int));
                OrderDetails.Columns.Add("Price", typeof(decimal));

                var materialColumn = new DataGridViewComboBoxColumn()
                {
                    DataSource = materialsTable,
                    ValueMember = "Id",
                    DisplayMember = "Name",
                    DataPropertyName = "MaterialId",
                    HeaderText = "Материал",
                    Width = 300
                };

                dgvMaterials.DataSource = OrderDetails;
                dgvMaterials.Columns.Clear();
                dgvMaterials.Columns.Add(materialColumn);
                dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "Quantity",
                    DataPropertyName = "Quantity",
                    HeaderText = "Количество",
                    Width = 100
                });
                dgvMaterials.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "Price",
                    DataPropertyName = "Price",
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

                dgvMaterials.CellValueChanged += (s, e) =>
                {
                    if (e.RowIndex >= 0 && e.ColumnIndex == 0)
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

                dgvMaterials.CellValueChanged += (s, e) =>
                {
                    if (e.RowIndex >= 0)
                    {
                        var row = dgvMaterials.Rows[e.RowIndex];
                        if (row.Cells["Price"].Value != null && row.Cells["Quantity"].Value != null)
                        {
                            var price = Convert.ToDecimal(row.Cells["Price"].Value);
                            var quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            row.Cells["Total"].Value = price * quantity;
                        }
                    }
                };

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

            try
            {
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
            using (var conn = Database.GetConnection())
            {
                var cmd = new SQLiteCommand(
                    "SELECT SupplierId, OrderDate, Status FROM Orders WHERE Id = @OrderId", conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cmbSupplier.SelectedValue = reader.GetInt32(0);
                        dtpOrderDate.Value = reader.GetDateTime(1);
                        cmbStatus.Text = reader.GetString(2);
                    }
                }

                var detailsAdapter = new SQLiteDataAdapter(
                    "SELECT MaterialId, Quantity, Price FROM OrderDetails WHERE OrderId = @OrderId", conn);
                detailsAdapter.SelectCommand.Parameters.AddWithValue("@OrderId", orderId);

                OrderDetails.Clear();
                detailsAdapter.Fill(OrderDetails);
            }
        }
    }
}