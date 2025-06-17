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
                Name = "SupplierName",
                DataPropertyName = "SupplierName",
                HeaderText = "Поставщик",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DeliveryDate",
                DataPropertyName = "DeliveryDate",
                HeaderText = "Дата поставки",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "d" }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Статус",
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                DataPropertyName = "Notes",
                HeaderText = "Примечания",
                Width = 300
            });

            dataGridView1.DataError += (s, e) => { e.ThrowException = false; };
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
                    var sql = @"SELECT d.Id, d.OrderId, s.Name as SupplierName, 
                               d.DeliveryDate, d.Status, d.Notes
                               FROM Deliveries d
                               LEFT JOIN Orders o ON d.OrderId = o.Id
                               LEFT JOIN Suppliers s ON o.SupplierId = s.Id";
                    var adapter = new SQLiteDataAdapter(sql, conn);
                    var table = new DataTable();
                    adapter.Fill(table);
                    
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = table;
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
            using (var dialog = new DeliveryDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var conn = Database.GetConnection())
                        {
                            using (var cmd = new SQLiteCommand(@"
                                INSERT INTO Deliveries (OrderId, DeliveryDate, Status, Notes) 
                                VALUES (@OrderId, @DeliveryDate, @Status, @Notes)", conn))
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка добавления поставки: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                using (var dialog = new DeliveryDialog(
                    Convert.ToInt32(row.Cells["OrderId"].Value),
                    Convert.ToDateTime(row.Cells["DeliveryDate"].Value),
                    row.Cells["Status"].Value.ToString(),
                    row.Cells["Notes"].Value?.ToString() ?? ""))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (var conn = Database.GetConnection())
                            {
                                using (var cmd = new SQLiteCommand(@"
                                    UPDATE Deliveries 
                                    SET OrderId = @OrderId, 
                                        DeliveryDate = @DeliveryDate, 
                                        Status = @Status, 
                                        Notes = @Notes 
                                    WHERE Id = @Id", conn))
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
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка обновления поставки: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
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
                    try
                    {
                        var id = dataGridView1.SelectedRows[0].Cells["Id"].Value;
                        using (var conn = Database.GetConnection())
                        using (var cmd = new SQLiteCommand("DELETE FROM Deliveries WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления поставки: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}