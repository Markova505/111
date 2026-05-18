using DairyDemo.Auth.Data.Models;

namespace DairyDemo.Auth.UI.Forms;

public class UserForm : Form
{
    private User _currentUser;
    private Label _welcomeLabel = null!;
    private Button _logoutButton = null!;

    public UserForm(User currentUser)
    {
        _currentUser = currentUser;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Личный кабинет пользователя";
        Size = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        _welcomeLabel = new Label
        {
            Text = $"Добро пожаловать, {_currentUser.Login}!\n\nВаша роль: {_currentUser.Role}",
            Location = new Point(20, 50),
            Size = new Size(340, 100),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Arial", 14, FontStyle.Regular)
        };
        Controls.Add(_welcomeLabel);

        _logoutButton = new Button
        {
            Text = "Выйти",
            Location = new Point(140, 180),
            Size = new Size(100, 35)
        };
        _logoutButton.Click += LogoutButton_Click;
        Controls.Add(_logoutButton);
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        var loginForm = Application.OpenForms.OfType<LoginForm>().FirstOrDefault();
        if (loginForm == null)
        {
            loginForm = new LoginForm();
        }
        loginForm.Show();
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        var loginForm = Application.OpenForms.OfType<LoginForm>().FirstOrDefault();
        if (loginForm != null)
        {
            Application.Exit();
        }
    }
}
