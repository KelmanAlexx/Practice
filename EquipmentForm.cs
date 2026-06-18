using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PrPractice1
{
    public partial class EquipmentForm : Form
    {
        private DataGridView dgvEquipment = new DataGridView();

        public EquipmentForm()
        {
            InitializeComponent();
            Text = "Реестр оборудования";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);

            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            dgvEquipment = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray }
            };

            Controls.Add(dgvEquipment);
        }

        private async void LoadData()
        {
            try
            {
                DataTable dt = await DbHelper.GetEquipmentAsync();
                dgvEquipment.DataSource = dt;

                if (dgvEquipment.Columns["id"] != null) dgvEquipment.Columns["id"].Visible = false;
                if (dgvEquipment.Columns["equipment_name"] != null) dgvEquipment.Columns["equipment_name"].HeaderText = "Наименование";
                if (dgvEquipment.Columns["supplier"] != null) dgvEquipment.Columns["supplier"].HeaderText = "Поставщик";
                if (dgvEquipment.Columns["serial_number"] != null) dgvEquipment.Columns["serial_number"].HeaderText = "Серийный номер (S/N)";
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
    }
}