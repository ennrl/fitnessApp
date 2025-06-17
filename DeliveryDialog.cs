using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ConstructionMaterialsManagement
{
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
            else
            {
                dtpDeliveryDate.Value = DateTime.Now;
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Поставка";
            this.Size = new System.Drawing.Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblOrder = new Label() { Text = "Заказ:", Left = 10, Top = 20 };
            cmbOrder = new ComboBox() { Left = 120, Top = 20, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblDate = new Label() { Text = "Дата поставки:", Left = 10, Top = 50 };
            dtpDeliveryDate = new DateTimePicker() { Left = 120, Top = 50, Width = 350 };

            var lblStatus = new Label() { Text = "Статус:", Left = 10, Top = 80 };
            cmbStatus = new ComboBox() { Left = 120, Top = 80, Width = 350 };
            cmbStatus.Items.AddRange(new string[] { "В обработке", "В пути", "Доставлено", "Отменено" });

            var lblNotes = new Label() { Text = "Примечания:", Left = 10, Top = 110 };
            txtNotes = new TextBox() { Left = 120, Top = 110, Width = 350, Height = 80, Multiline = true };

            btnSave = new Button() { Text = "Сохранить", Left = 200, Top = 220 };
            btnCancel = new Button() { Text = "Отмена", Left = 300, Top = 220 };

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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbOrder.SelectedValue == null)
            {
                MessageBox.Show("Выберите заказ!");
                return;
            }

            try
            {
                OrderId = Convert.ToInt32(cmbOrder.SelectedValue);
                DeliveryDate = dtpDeliveryDate.Value;
                Status = cmbStatus.Text;
                Notes = txtNotes.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}