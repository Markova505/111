using System;
using System.Drawing;
using System.Windows.Forms;

namespace AuthSystem
{
    public partial class LoginForm : Form
    {
        private int captchaAttempts = 0;
        private int loginAttempts = 0;
        private const int MaxTotalAttempts = 3;
        private string? currentLogin = null;
        private bool isCaptchaPassed = false;

        public LoginForm()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
        }

        private void InitializeComponent()
        {
            this.Text = "Авторизация";
            this.Size = new Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var titleLabel = new Label
            {
                Text = "Форма авторизации",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 20)
            };
            this.Controls.Add(titleLabel);

            var loginLabel = new Label
            {
                Text = "Логин:",
                AutoSize = true,
                Location = new Point(30, 70)
            };
            this.Controls.Add(loginLabel);

            var loginTextBox = new TextBox
            {
                Name = "loginTextBox",
                Location = new Point(30, 95),
                Size = new Size(320, 25),
                Required = true
            };
            this.Controls.Add(loginTextBox);

            var passwordLabel = new Label
            {
                Text = "Пароль:",
                AutoSize = true,
                Location = new Point(30, 130)
            };
            this.Controls.Add(passwordLabel);

            var passwordTextBox = new TextBox
            {
                Name = "passwordTextBox",
                Location = new Point(30, 155),
                Size = new Size(320, 25),
                PasswordChar = '*',
                Required = true
            };
            this.Controls.Add(passwordTextBox);

            var captchaButton = new Button
            {
                Text = "Пройти капчу",
                Location = new Point(30, 195),
                Size = new Size(150, 30)
            };
            captchaButton.Click += CaptchaButton_Click;
            this.Controls.Add(captchaButton);

            var captchaStatusLabel = new Label
            {
                Name = "captchaStatusLabel",
                Text = "Капча не пройдена",
                AutoSize = true,
                Location = new Point(200, 200),
                ForeColor = Color.Red
            };
            this.Controls.Add(captchaStatusLabel);

            var loginButton = new Button
            {
                Text = "Войти",
                Name = "loginButton",
                Location = new Point(30, 240),
                Size = new Size(150, 35),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            loginButton.Click += LoginButton_Click;
            this.Controls.Add(loginButton);

            this.AcceptButton = loginButton;
        }

        private void CaptchaButton_Click(object? sender, EventArgs e)
        {
            if (loginAttempts >= MaxTotalAttempts)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            using (var captchaForm = new PuzzleCaptchaForm())
            {
                var result = captchaForm.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    isCaptchaPassed = true;
                    var captchaStatusLabel = this.Controls.Find("captchaStatusLabel", true)[0] as Label;
                    if (captchaStatusLabel != null)
                    {
                        captchaStatusLabel.Text = "Капча пройдена ✓";
                        captchaStatusLabel.ForeColor = Color.Green;
                    }
                    captchaAttempts = 0; // Сбрасываем попытки капчи при успешном прохождении
                }
                else if (result == DialogResult.Abort)
                {
                    captchaAttempts++;
                    loginAttempts++;
                    
                    if (loginAttempts >= MaxTotalAttempts)
                    {
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                }
            }
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            if (loginAttempts >= MaxTotalAttempts)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            var loginTextBox = this.Controls.Find("loginTextBox", true)[0] as TextBox;
            var passwordTextBox = this.Controls.Find("passwordTextBox", true)[0] as TextBox;

            if (loginTextBox == null || passwordTextBox == null)
                return;

            string login = loginTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            // Проверка обязательных полей
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Поля Логин и Пароль обязательны для заполнения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка капчи
            if (!isCaptchaPassed)
            {
                MessageBox.Show("Сначала пройдите капчу", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка блокировки пользователя
            if (DatabaseHelper.IsUserBlocked(login))
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            currentLogin = login;

            // Валидация пользователя
            if (DatabaseHelper.ValidateUser(login, password))
            {
                MessageBox.Show("Вы успешно авторизовались", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                string role = DatabaseHelper.GetUserRole(login);
                
                // Открываем соответствующую форму в зависимости от роли
                this.Hide();
                
                if (role == "Администратор")
                {
                    using (var adminForm = new AdminForm(login))
                    {
                        adminForm.ShowDialog();
                    }
                }
                else
                {
                    using (var userForm = new UserForm(login))
                    {
                        userForm.ShowDialog();
                    }
                }
                
                this.Close();
            }
            else
            {
                loginAttempts++;
                
                if (loginAttempts >= MaxTotalAttempts)
                {
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    passwordTextBox.Clear();
                }
            }
        }
    }
}
