using DairyDemo.Auth.Data;
using DairyDemo.Auth.Data.Models;
using DairyDemo.Auth.Services;
using DairyDemo.Auth.UI.Controls;

namespace DairyDemo.Auth.UI.Forms;

public class LoginForm : Form
{
    private TextBox _loginTextBox = null!;
    private TextBox _passwordTextBox = null!;
    private Button _loginButton = null!;
    private Label _messageLabel = null!;
    private CaptchaPuzzleControl _captchaControl = null!;
    private AuthService _authService = null!;
    private int _captchaFailedAttempts = 0;
    private const int MaxCaptchaFailedAttempts = 3;

    public LoginForm()
    {
        InitializeComponent();
        _authService = new AuthService();
        InitializeCaptcha();
    }

    private void InitializeComponent()
    {
        Text = "Авторизация";
        Size = new Size(400, 500);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var loginLabel = new Label
        {
            Text = "Логин:",
            Location = new Point(20, 20),
            AutoSize = true
        };
        Controls.Add(loginLabel);

        _loginTextBox = new TextBox
        {
            Location = new Point(20, 45),
            Size = new Size(340, 25),
            Name = "LoginTextBox"
        };
        Controls.Add(_loginTextBox);

        var passwordLabel = new Label
        {
            Text = "Пароль:",
            Location = new Point(20, 80),
            AutoSize = true
        };
        Controls.Add(passwordLabel);

        _passwordTextBox = new TextBox
        {
            Location = new Point(20, 105),
            Size = new Size(340, 25),
            PasswordChar = '*',
            Name = "PasswordTextBox"
        };
        Controls.Add(_passwordTextBox);

        var captchaLabel = new Label
        {
            Text = "Соберите пазл (кликните по двум фрагментам для обмена):",
            Location = new Point(20, 145),
            AutoSize = true
        };
        Controls.Add(captchaLabel);

        _captchaControl = new CaptchaPuzzleControl
        {
            Location = new Point(20, 170),
            Size = new Size(340, 200),
            BackColor = Color.White
        };
        _captchaControl.PuzzleCompleted += OnCaptchaCompleted;
        Controls.Add(_captchaControl);

        _loginButton = new Button
        {
            Text = "Войти",
            Location = new Point(20, 380),
            Size = new Size(340, 35),
            Enabled = false
        };
        _loginButton.Click += LoginButton_Click;
        Controls.Add(_loginButton);

        _messageLabel = new Label
        {
            Location = new Point(20, 425),
            Size = new Size(340, 50),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_messageLabel);
    }

    private void InitializeCaptcha()
    {
        string captchaFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "captcha");
        
        // Если папка не существует, создаем её
        if (!Directory.Exists(captchaFolder))
        {
            Directory.CreateDirectory(captchaFolder);
        }

        _captchaControl.InitializeCaptcha(captchaFolder);
    }

    private void OnCaptchaCompleted(object? sender, EventArgs e)
    {
        _captchaFailedAttempts = 0;
        _loginButton.Enabled = true;
        _messageLabel.Text = "Пазл собран верно! Теперь можете войти.";
        _messageLabel.ForeColor = Color.Green;
    }

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_loginTextBox.Text) || 
            string.IsNullOrWhiteSpace(_passwordTextBox.Text))
        {
            _messageLabel.Text = "Заполните все поля";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        if (!_captchaControl.IsSolved())
        {
            _messageLabel.Text = "Сначала соберите пазл";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        var result = _authService.Authenticate(_loginTextBox.Text, _passwordTextBox.Text);

        if (result.Success)
        {
            _messageLabel.Text = result.Message;
            _messageLabel.ForeColor = Color.Green;

            // Открываем соответствующую форму
            await Task.Delay(500);
            if (result.User?.Role == "Admin")
            {
                var adminForm = new AdminForm(result.User);
                adminForm.Show();
                Hide();
            }
            else
            {
                var userForm = new UserForm(result.User);
                userForm.Show();
                Hide();
            }
        }
        else
        {
            _messageLabel.Text = result.Message;
            _messageLabel.ForeColor = Color.Red;

            // Проверяем, заблокирован ли пользователь
            if (result.Message.Contains("заблокированы"))
            {
                _loginButton.Enabled = false;
                return;
            }

            // Сбрасываем капчу при неверном пароле
            _captchaFailedAttempts++;
            if (_captchaFailedAttempts >= MaxCaptchaFailedAttempts)
            {
                // Блокируем пользователя после 3 неудачных попыток капчи
                var user = Db.GetUserByLogin(_loginTextBox.Text);
                if (user != null)
                {
                    user.FailedAttempts = MaxCaptchaFailedAttempts;
                    user.IsBlocked = true;
                    _messageLabel.Text = "Вы заблокированы. Обратитесь к администратору";
                    _loginButton.Enabled = false;
                }
            }
            else
            {
                // Перезапускаем капчу
                _captchaControl.InitializeCaptcha(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "captcha"));
                _loginButton.Enabled = false;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        // Не закрываем приложение полностью, если открыты другие формы
    }
}
