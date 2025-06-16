using System;
using System.Windows.Forms;

namespace ConstructionMaterialsManagement
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Database.Initialize();
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new MenuStrip();
            this.suppliersToolStripMenuItem = new ToolStripMenuItem();
            this.materialsToolStripMenuItem = new ToolStripMenuItem();
            this.ordersToolStripMenuItem = new ToolStripMenuItem();
            this.deliveriesToolStripMenuItem = new ToolStripMenuItem();

            // MainForm
            this.Text = "Управление поставками материалов";
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;

            // MenuStrip
            this.menuStrip1.Items.AddRange(new ToolStripItem[] {
                suppliersToolStripMenuItem,
                materialsToolStripMenuItem,
                ordersToolStripMenuItem,
                deliveriesToolStripMenuItem
            });
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;

            // Menu items
            this.suppliersToolStripMenuItem.Text = "Поставщики";
            this.materialsToolStripMenuItem.Text = "Материалы";
            this.ordersToolStripMenuItem.Text = "Заказы";
            this.deliveriesToolStripMenuItem.Text = "Поставки";

            // Events
            this.suppliersToolStripMenuItem.Click += new EventHandler(SuppliersMenuItem_Click);
            this.materialsToolStripMenuItem.Click += new EventHandler(MaterialsMenuItem_Click);
            this.ordersToolStripMenuItem.Click += new EventHandler(OrdersMenuItem_Click);
            this.deliveriesToolStripMenuItem.Click += new EventHandler(DeliveriesMenuItem_Click);
        }

        private void SuppliersMenuItem_Click(object sender, EventArgs e)
        {
            var form = new SuppliersForm();
            form.MdiParent = this;
            form.Show();
        }

        private void MaterialsMenuItem_Click(object sender, EventArgs e)
        {
            var form = new MaterialsForm();
            form.MdiParent = this;
            form.Show();
        }

        private void OrdersMenuItem_Click(object sender, EventArgs e)
        {
            var form = new OrdersForm();
            form.MdiParent = this;
            form.Show();
        }

        private void DeliveriesMenuItem_Click(object sender, EventArgs e)
        {
            var form = new DeliveryForm();
            form.MdiParent = this;
            form.Show();
        }

        private MenuStrip menuStrip1;
        private ToolStripMenuItem suppliersToolStripMenuItem;
        private ToolStripMenuItem materialsToolStripMenuItem;
        private ToolStripMenuItem ordersToolStripMenuItem;
        private ToolStripMenuItem deliveriesToolStripMenuItem;
    }
}