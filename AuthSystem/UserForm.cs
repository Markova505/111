using System;
using System.Drawing;
using System.Windows.Forms;

namespace AuthSystem
{
    public partial class UserForm : Form
    {
        private string currentLogin;

        public UserForm(string login)
        {
            currentLogin = login;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Рабочий стол пользователя - {currentLogin}";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            var welcomeLabel = new Label
            {
                Text = $"Добро пожаловать, {currentLogin}!",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 50)
            };
            this.Controls.Add(welcomeLabel);

            var infoLabel = new Label
            {
                Text = "Вы вошли в систему как обычный пользователь.",
                Font = new Font("Arial", 12),
                AutoSize = true,
                Location = new Point(50, 100)
            };
            this.Controls.Add(infoLabel);

            var roleLabel = new Label
            {
                Text = $"Ваша роль: Пользователь",
                Font = new Font("Arial", 12),
                AutoSize = true,
                Location = new Point(50, 140),
                ForeColor = Color.Blue
            };
            this.Controls.Add(roleLabel);

            var logoutButton = new Button
            {
                Text = "Выйти",
                Location = new Point(50, 200),
                Size = new Size(150, 40)
            };
            logoutButton.Click += LogoutButton_Click;
            this.Controls.Add(logoutButton);
        }

        private void LogoutButton_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
