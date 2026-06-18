using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PrPractice1
{
    public partial class RequestForm : Form
    {
        private int requestId;
        private string role;
        private MainForm parentForm;

        private ComboBox cmbStatus = new ComboBox();
        private ComboBox cmbMaterials = new ComboBox();
        private TextBox txtQuantity = new TextBox();

        public RequestForm(int id, string currentStatus, string empName, string userRole, MainForm parent)
        {
            InitializeComponent();
            requestId = id;
            role = userRole;
            parentForm = parent;

            Text = $"Заявка #{requestId} - Исполнитель: {empName}";
            Size = new Size(450, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Font = new Font("Segoe UI", 10);

            InitializeUI(currentStatus);
            LoadMaterials();
        }

        private void InitializeUI(string currentStatus)
        {
            Controls.Add(new Label { Text = "Текущий статус:", Location = new Point(20, 20), AutoSize = true });
            
            cmbStatus = new ComboBox { Location = new Point(140, 17), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "Активна", "В работе", "Завершена" });
            cmbStatus.SelectedItem = currentStatus;
            Controls.Add(cmbStatus);

            Button btnUpdateStatus = new Button { Text = "Сменить статус", Location = new Point(300, 15), Size = new Size(120, 30), BackColor = Color.WhiteSmoke, FlatStyle = FlatStyle.Flat };
            btnUpdateStatus.Click += async (s, e) => 
            {
                if (await DbHelper.UpdateRequestStatusAsync(requestId, cmbStatus.SelectedItem.ToString()))
                {
                    MessageBox.Show("Статус обновлен (аудит записан триггером)!");
                    parentForm.LoadRequests();
                }
            };
            Controls.Add(btnUpdateStatus);


            GroupBox grpMaterials = new GroupBox { Text = "Расход материалов", Location = new Point(20, 80), Size = new Size(400, 150) };
            
            grpMaterials.Controls.Add(new Label { Text = "Материал:", Location = new Point(15, 30), AutoSize = true });
            cmbMaterials = new ComboBox { Location = new Point(100, 27), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "name", ValueMember = "id" };
            grpMaterials.Controls.Add(cmbMaterials);

            grpMaterials.Controls.Add(new Label { Text = "Кол-во:", Location = new Point(15, 70), AutoSize = true });
            txtQuantity = new TextBox { Location = new Point(100, 67), Size = new Size(100, 25) };
            grpMaterials.Controls.Add(txtQuantity);

            Button btnAddMaterial = new Button { Text = "Списать на объект", Location = new Point(100, 105), Size = new Size(150, 30), BackColor = Color.LightYellow, FlatStyle = FlatStyle.Flat };
            btnAddMaterial.Click += BtnAddMaterial_Click;
            grpMaterials.Controls.Add(btnAddMaterial);

            if (role == "Менеджер" || role == "Бухгалтер")
            {
                grpMaterials.Enabled = false;
            }

            Controls.Add(grpMaterials);
        }

        private async void LoadMaterials()
        {
            try
            {
                DataTable dt = await DbHelper.GetMaterialsDictionaryAsync();
                cmbMaterials.DataSource = dt;
            }
            catch { }
        }

        private async void BtnAddMaterial_Click(object sender, EventArgs e)
        {
            if (cmbMaterials.SelectedValue == null || !decimal.TryParse(txtQuantity.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Выберите материал и укажите корректное количество!");
                return;
            }

            int matId = Convert.ToInt32(cmbMaterials.SelectedValue);
            bool success = await DbHelper.AssignMaterialAsync(requestId, matId, qty);
            
            if (success)
            {
                MessageBox.Show("Материал успешно списан на заявку!");
                txtQuantity.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка списания. Возможно, материал уже добавлен к этой заявке.");
            }
        }
    }
}