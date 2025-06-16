using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
    public partial class DeliveryForm : Form
    {
        private DataGridView dataGridView1;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public DeliveryForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Поставки";
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
                Name = "OrderId",
                DataPropertyName = "OrderId",
                HeaderText = "№ Заказа",
                Width = 100
            });
            
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DeliveryDate",
                DataPropertyName = "DeliveryDate",
                HeaderText = "Дата доставки",
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

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                DataPropertyName = "Notes",
                HeaderText = "Примечания",
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
            try 
            {
                using (var conn = Database.GetConnection())
                {
                    var sql = @"SELECT Deliveries.Id, Deliveries.DeliveryDate, 
                               Deliveries.Status, Orders.Id as OrderId, Deliveries.Notes
                               FROM Deliveries 
                               LEFT JOIN Orders ON Deliveries.OrderId = Orders.Id";
                    var adapter = new SQLiteDataAdapter(sql, conn);
                    var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("OrderId", typeof(int));
                    table.Columns.Add("DeliveryDate", typeof(DateTime));
                    table.Columns.Add("Status", typeof(string));
                    table.Columns.Add("Notes", typeof(string));
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
            var dialog = new DeliveryDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var conn = Database.GetConnection())
                {
                    var sql = @"INSERT INTO Deliveries (OrderId, DeliveryDate, Status, Notes) 
                               VALUES (@OrderId, @DeliveryDate, @Status, @Notes)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", dialog.OrderId);
                        cmd.Parameters.AddWithValue("@DeliveryDate", dialog.DeliveryDate);
                        cmd.Parameters.AddWithValue("@Status", dialog.Status);
                        cmd.Parameters.AddWithValue("@Notes", dialog.Notes);
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
                var dialog = new DeliveryDialog(
                    Convert.ToInt32(row.Cells["OrderId"].Value),
                    Convert.ToDateTime(row.Cells["DeliveryDate"].Value),
                    row.Cells["Status"].Value.ToString(),
                    row.Cells["Notes"]?.Value?.ToString() ?? ""
                );

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var conn = Database.GetConnection())
                    {
                        var sql = @"UPDATE Deliveries 
                                   SET OrderId = @OrderId, DeliveryDate = @DeliveryDate, 
                                       Status = @Status, Notes = @Notes 
                                   WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", row.Cells["Id"].Value);
                            cmd.Parameters.AddWithValue("@OrderId", dialog.OrderId);
                            cmd.Parameters.AddWithValue("@DeliveryDate", dialog.DeliveryDate);
                            cmd.Parameters.AddWithValue("@Status", dialog.Status);
                            cmd.Parameters.AddWithValue("@Notes", dialog.Notes);
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
                if (MessageBox.Show("Удалить выбранную поставку?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var id = dataGridView1.SelectedRows[0].Cells["Id"].Value.ToString();
                    using (var conn = Database.GetConnection())
                    using (var cmd = new SQLiteCommand("DELETE FROM Deliveries WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                }
            }
        }
    }

    public class DeliveryDialog : Form
    {
        private ComboBox cmbOrder;
        private DateTimePicker dtpDeliveryDate;
        private ComboBox cmbStatus;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public int? OrderId { get; private set; }
        public DateTime DeliveryDate { get; private set; }
        public string Status { get; private set; }
        public string Notes { get; private set; }

        public DeliveryDialog(int? orderId = null, DateTime? deliveryDate = null, 
            string status = "В обработке", string notes = "")
        {
            InitializeComponent();
            LoadOrders();
            
            if (orderId.HasValue)
            {
                cmbOrder.SelectedValue = orderId;
                dtpDeliveryDate.Value = deliveryDate ?? DateTime.Now;
                cmbStatus.Text = status;
                txtNotes.Text = notes;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Поставка";
            this.Size = new System.Drawing.Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblOrder = new Label() { Text = "Заказ:", Left = 10, Top = 20 };
            cmbOrder = new ComboBox() { Left = 120, Top = 20, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblDate = new Label() { Text = "Дата поставки:", Left = 10, Top = 50 };
            dtpDeliveryDate = new DateTimePicker() { Left = 120, Top = 50, Width = 350 };

            var lblStatus = new Label() { Text = "Статус:", Left = 10, Top = 80 };
            cmbStatus = new ComboBox() { Left = 120, Top = 80, Width = 350 };
            cmbStatus.Items.AddRange(new string[] { "В обработке", "В пути", "Доставлено", "Отменено" });
            cmbStatus.SelectedIndex = 0;

            var lblNotes = new Label() { Text = "Примечания:", Left = 10, Top = 110 };
            txtNotes = new TextBox() { Left = 120, Top = 110, Width = 350, Height = 100, Multiline = true };

            btnSave = new Button() { Text = "Сохранить", Left = 120, Top = 300 };
            btnCancel = new Button() { Text = "Отмена", Left = 220, Top = 300 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { 
                lblOrder, cmbOrder,
                lblDate, dtpDeliveryDate,
                lblStatus, cmbStatus,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private void LoadOrders()
        {
            using (var conn = Database.GetConnection())
            {
                var adapter = new SQLiteDataAdapter(@"
                    SELECT Orders.Id, 
                           Orders.Id || ' - ' || Suppliers.Name || ' от ' || Orders.OrderDate as DisplayName
                    FROM Orders 
                    LEFT JOIN Suppliers ON Orders.SupplierId = Suppliers.Id", conn);
                var table = new DataTable();
                adapter.Fill(table);
                cmbOrder.DisplayMember = "DisplayName";
                cmbOrder.ValueMember = "Id";
                cmbOrder.DataSource = table;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbOrder.SelectedValue == null)
            {
                MessageBox.Show("Выберите заказ!");
                return;
            }

            OrderId = (int)cmbOrder.SelectedValue;
            DeliveryDate = dtpDeliveryDate.Value;
            Status = cmbStatus.Text;
            Notes = txtNotes.Text;
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