using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
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
            dataGridView1.AutoGenerateColumns = false;

            // Настройка колонок
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialName",
                DataPropertyName = "MaterialName",
                HeaderText = "Материал",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                DataPropertyName = "Quantity",
                HeaderText = "Количество",
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

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                DataPropertyName = "Total",
                HeaderText = "Сумма",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            this.Controls.Add(dataGridView1);
        }

        private void LoadData()
        {
            try
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
                    
                    var bs = new BindingSource();
                    bs.DataSource = table;
                    dataGridView1.DataSource = bs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}