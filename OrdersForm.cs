using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
    public class OrdersForm : Form
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

            dataGridView1.DataError += (s, e) => { e.ThrowException = false; };
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
            using (var dialog = new OrderDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var conn = Database.GetConnection())
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT INTO Orders (SupplierId, OrderDate, Status) VALUES (@SupplierId, @OrderDate, @Status); SELECT last_insert_rowid();", 
                                conn))
                            {
                                cmd.Parameters.AddWithValue("@SupplierId", dialog.SupplierId);
                                cmd.Parameters.AddWithValue("@OrderDate", dialog.OrderDate);
                                cmd.Parameters.AddWithValue("@Status", dialog.Status);
                                var orderId = Convert.ToInt64(cmd.ExecuteScalar());

                                foreach (DataRow row in dialog.OrderDetails.Rows)
                                {
                                    using (var detailCmd = new SQLiteCommand(
                                        "INSERT INTO OrderDetails (OrderId, MaterialId, Quantity, Price) VALUES (@OrderId, @MaterialId, @Quantity, @Price)", 
                                        conn))
                                    {
                                        detailCmd.Parameters.AddWithValue("@OrderId", orderId);
                                        detailCmd.Parameters.AddWithValue("@MaterialId", row["MaterialId"]);
                                        detailCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                                        detailCmd.Parameters.AddWithValue("@Price", row["Price"]);
                                        detailCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения заказа: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                using (var dialog = new OrderDialog())
                {
                    dialog.LoadOrder(id);
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (var conn = Database.GetConnection())
                            {
                                using (var cmd = new SQLiteCommand(
                                    "UPDATE Orders SET SupplierId = @SupplierId, OrderDate = @OrderDate, Status = @Status WHERE Id = @Id", 
                                    conn))
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
                                    using (var cmd = new SQLiteCommand(
                                        "INSERT INTO OrderDetails (OrderId, MaterialId, Quantity, Price) VALUES (@OrderId, @MaterialId, @Quantity, @Price)", 
                                        conn))
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
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка обновления заказа: {ex.Message}", "Ошибка",
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
                if (MessageBox.Show("Удалить выбранный заказ?", "Подтверждение", 
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                using (var detailsForm = new OrderDetailsForm(orderId))
                {
                    detailsForm.ShowDialog();
                }
            }
        }
    }
}