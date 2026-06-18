using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PrPractice1
{
    public partial class MainForm : Form
    {
        private string userName;
        private string userRole;
        
        private DataGridView dgvRequests = new DataGridView();
        private ComboBox cmbFilterStatus = new ComboBox();
        private Button btnEquipment = new Button();
        private Button btnManageRequest = new Button();

        public MainForm(string name, string role)
        {
            InitializeComponent();
            userName = name;
            userRole = role;

            Text = $"Панель управления - {userName} ({userRole})";
            Size = new Size(900, 500);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            InitializeUI();
            ApplyRBAC();
            LoadRequests();
        }

        private void InitializeUI()
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };

            Label lblFilter = new Label { Text = "Фильтр задач:", Location = new Point(15, 20), AutoSize = true };
            cmbFilterStatus = new ComboBox { Location = new Point(120, 17), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterStatus.Items.AddRange(new string[] { "Все", "Активна", "В работе", "Завершена" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadRequests();

            btnManageRequest = new Button { Text = "Управление заявкой", Location = new Point(290, 15), Size = new Size(160, 30), BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnManageRequest.Click += BtnManageRequest_Click;

            btnEquipment = new Button { Text = "Справочник оборудования", Location = new Point(470, 15), Size = new Size(200, 30), BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
            btnEquipment.Click += (s, e) => new EquipmentForm().ShowDialog();

            topPanel.Controls.AddRange(new Control[] { lblFilter, cmbFilterStatus, btnManageRequest, btnEquipment });

            dgvRequests = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            dgvRequests.DoubleClick += BtnManageRequest_Click;

            Controls.Add(dgvRequests);
            Controls.Add(topPanel);
        }

        private void ApplyRBAC()
        {
            if (userRole == "Бухгалтер")
            {
                btnManageRequest.Visible = false;
            }
        }

        public async void LoadRequests()
        {
            try
            {
                string status = cmbFilterStatus.SelectedItem.ToString();
                DataTable dt = await DbHelper.GetRequestsAsync(status, userRole, userName);
                dgvRequests.DataSource = dt;

                if (dgvRequests.Columns["request_id"] != null) dgvRequests.Columns["request_id"].HeaderText = "ID";
                if (dgvRequests.Columns["address"] != null) dgvRequests.Columns["address"].HeaderText = "Объект";
                if (dgvRequests.Columns["client"] != null) dgvRequests.Columns["client"].HeaderText = "Клиент";
                if (dgvRequests.Columns["employee"] != null) dgvRequests.Columns["employee"].HeaderText = "Исполнитель";
                if (dgvRequests.Columns["status"] != null) dgvRequests.Columns["status"].HeaderText = "Статус";
                if (dgvRequests.Columns["request_date"] != null) dgvRequests.Columns["request_date"].HeaderText = "Дата";
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

        private void BtnManageRequest_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;

            int reqId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["request_id"].Value);
            string currentStatus = dgvRequests.SelectedRows[0].Cells["status"].Value.ToString();
            string employee = dgvRequests.SelectedRows[0].Cells["employee"].Value.ToString();

            RequestForm reqForm = new RequestForm(reqId, currentStatus, employee, userRole, this);
            reqForm.ShowDialog();
        }
    }
}