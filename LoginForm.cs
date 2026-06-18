using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PrPractice1
{
    public partial class LoginForm : Form
    {
        private TextBox txtFullName = new TextBox();
        private TextBox txtPassword = new TextBox();

        public LoginForm()
        {
            InitializeComponent();
            Text = "Авторизация сотрудников";
            Size = new Size(350, 260);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 11);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Controls.Add(new Label { Text = "ФИО сотрудника:", Location = new Point(40, 30), Size = new Size(150, 25) });
            txtFullName = new TextBox { Location = new Point(40, 55), Size = new Size(250, 25) };
            Controls.Add(txtFullName);

            Controls.Add(new Label { Text = "Пароль:", Location = new Point(40, 95), Size = new Size(100, 25) });
            txtPassword = new TextBox { Location = new Point(40, 120), Size = new Size(250, 25), PasswordChar = '*' };
            Controls.Add(txtPassword);

            Button btnLogin = new Button { Text = "Войти", Location = new Point(40, 160), Size = new Size(250, 35), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnLogin.Click += BtnLogin_Click;
            Controls.Add(btnLogin);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFullName.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Введите ФИО и пароль!");
                return;
            }

            try
            {
                DataTable table = await DbHelper.CheckUserAsync(txtFullName.Text, txtPassword.Text);

                if (table.Rows.Count > 0)
                {
                    string name = table.Rows[0]["full_name"].ToString();
                    string role = table.Rows[0]["role"].ToString();
                    
                    Hide();
                    MainForm main = new MainForm(name, role);
                    main.FormClosed += (s, args) => Close();
                    main.Show();
                }
                else
                {
                    MessageBox.Show("Сотрудник не найден или пароль неверен!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка БД: " + ex.Message); }
        }
    }
}