using DairyDemo.Auth.Data;
using DairyDemo.Auth.Data.Models;

namespace DairyDemo.Auth.UI.Forms;

public class AdminForm : Form
{
    private User _currentUser;
    private DataGridView _usersGrid = null!;
    private TextBox _newLoginTextBox = null!;
    private TextBox _newPasswordTextBox = null!;
    private ComboBox _roleComboBox = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _unblockButton = null!;
    private Button _logoutButton = null!;
    private Label _messageLabel = null!;

    public AdminForm(User currentUser)
    {
        _currentUser = currentUser;
        InitializeComponent();
        LoadUsers();
    }

    private void InitializeComponent()
    {
        Text = "Панель администратора";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var titleLabel = new Label
        {
            Text = $"Добро пожаловать, {_currentUser.Login} (Администратор)",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Arial", 14, FontStyle.Bold)
        };
        Controls.Add(titleLabel);

        // Сетка пользователей
        _usersGrid = new DataGridView
        {
            Location = new Point(20, 60),
            Size = new Size(750, 250),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true
        };
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID" });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Login", HeaderText = "Логин" });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Роль" });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsBlocked", HeaderText = "Заблокирован" });
        Controls.Add(_usersGrid);

        // Панель добавления пользователя
        var addGroup = new GroupBox
        {
            Text = "Добавить нового пользователя",
            Location = new Point(20, 320),
            Size = new Size(750, 100)
        };

        var loginLabel = new Label
        {
            Text = "Логин:",
            Location = new Point(10, 25),
            AutoSize = true
        };
        addGroup.Controls.Add(loginLabel);

        _newLoginTextBox = new TextBox
        {
            Location = new Point(70, 22),
            Size = new Size(150, 25)
        };
        addGroup.Controls.Add(_newLoginTextBox);

        var passwordLabel = new Label
        {
            Text = "Пароль:",
            Location = new Point(240, 25),
            AutoSize = true
        };
        addGroup.Controls.Add(passwordLabel);

        _newPasswordTextBox = new TextBox
        {
            Location = new Point(300, 22),
            Size = new Size(150, 25)
        };
        addGroup.Controls.Add(_newPasswordTextBox);

        var roleLabel = new Label
        {
            Text = "Роль:",
            Location = new Point(470, 25),
            AutoSize = true
        };
        addGroup.Controls.Add(roleLabel);

        _roleComboBox = new ComboBox
        {
            Location = new Point(520, 22),
            Size = new Size(100, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _roleComboBox.Items.AddRange(new object[] { "User", "Admin" });
        _roleComboBox.SelectedIndex = 0;
        addGroup.Controls.Add(_roleComboBox);

        _addButton = new Button
        {
            Text = "Добавить",
            Location = new Point(640, 20),
            Size = new Size(90, 30)
        };
        _addButton.Click += AddButton_Click;
        addGroup.Controls.Add(_addButton);

        Controls.Add(addGroup);

        // Кнопки управления
        _editButton = new Button
        {
            Text = "Изменить",
            Location = new Point(20, 440),
            Size = new Size(120, 35)
        };
        _editButton.Click += EditButton_Click;
        Controls.Add(_editButton);

        _unblockButton = new Button
        {
            Text = "Снять блокировку",
            Location = new Point(160, 440),
            Size = new Size(140, 35)
        };
        _unblockButton.Click += UnblockButton_Click;
        Controls.Add(_unblockButton);

        _logoutButton = new Button
        {
            Text = "Выйти",
            Location = new Point(640, 440),
            Size = new Size(120, 35)
        };
        _logoutButton.Click += LogoutButton_Click;
        Controls.Add(_logoutButton);

        _messageLabel = new Label
        {
            Location = new Point(20, 490),
            Size = new Size(750, 50),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_messageLabel);
    }

    private void LoadUsers()
    {
        _usersGrid.Rows.Clear();
        foreach (var user in Db.GetAllUsers())
        {
            _usersGrid.Rows.Add(user.Id, user.Login, user.Role, user.IsBlocked ? "Да" : "Нет");
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_newLoginTextBox.Text) || 
            string.IsNullOrWhiteSpace(_newPasswordTextBox.Text))
        {
            _messageLabel.Text = "Заполните все поля";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        var newUser = new User
        {
            Login = _newLoginTextBox.Text,
            PasswordHash = PasswordService.HashPassword(_newPasswordTextBox.Text),
            Role = _roleComboBox.SelectedItem?.ToString() ?? "User"
        };

        if (Db.AddUser(newUser))
        {
            _messageLabel.Text = "Пользователь успешно добавлен";
            _messageLabel.ForeColor = Color.Green;
            _newLoginTextBox.Clear();
            _newPasswordTextBox.Clear();
            LoadUsers();
        }
        else
        {
            _messageLabel.Text = "Пользователь с таким логином уже существует";
            _messageLabel.ForeColor = Color.Red;
        }
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        if (_usersGrid.SelectedRows.Count == 0)
        {
            _messageLabel.Text = "Выберите пользователя для редактирования";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        var selectedRow = _usersGrid.SelectedRows[0];
        int userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
        var user = Db.GetUserById(userId);

        if (user == null)
        {
            _messageLabel.Text = "Пользователь не найден";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        using var editForm = new EditUserForm(user);
        if (editForm.ShowDialog() == DialogResult.OK)
        {
            user.Login = editForm.UpdatedLogin;
            user.Role = editForm.UpdatedRole;
            
            if (!string.IsNullOrWhiteSpace(editForm.UpdatedPassword))
            {
                user.PasswordHash = PasswordService.HashPassword(editForm.UpdatedPassword);
            }

            if (Db.UpdateUser(user))
            {
                _messageLabel.Text = "Данные пользователя обновлены";
                _messageLabel.ForeColor = Color.Green;
                LoadUsers();
            }
            else
            {
                _messageLabel.Text = "Ошибка обновления (возможно логин занят)";
                _messageLabel.ForeColor = Color.Red;
            }
        }
    }

    private void UnblockButton_Click(object? sender, EventArgs e)
    {
        if (_usersGrid.SelectedRows.Count == 0)
        {
            _messageLabel.Text = "Выберите пользователя для разблокировки";
            _messageLabel.ForeColor = Color.Red;
            return;
        }

        var selectedRow = _usersGrid.SelectedRows[0];
        int userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
        
        Db.UnblockUser(userId);
        _messageLabel.Text = "Блокировка снята";
        _messageLabel.ForeColor = Color.Green;
        LoadUsers();
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

// Форма редактирования пользователя
public class EditUserForm : Form
{
    private TextBox _loginTextBox = null!;
    private TextBox _passwordTextBox = null!;
    private ComboBox _roleComboBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;

    public string UpdatedLogin { get; private set; } = string.Empty;
    public string UpdatedPassword { get; private set; } = string.Empty;
    public string UpdatedRole { get; private set; } = string.Empty;

    public EditUserForm(User user)
    {
        Text = "Редактирование пользователя";
        Size = new Size(350, 250);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
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
            Size = new Size(290, 25),
            Text = user.Login
        };
        Controls.Add(_loginTextBox);

        var passwordLabel = new Label
        {
            Text = "Новый пароль (оставьте пустым, чтобы не менять):",
            Location = new Point(20, 80),
            AutoSize = true
        };
        Controls.Add(passwordLabel);

        _passwordTextBox = new TextBox
        {
            Location = new Point(20, 105),
            Size = new Size(290, 25),
            PasswordChar = '*'
        };
        Controls.Add(_passwordTextBox);

        var roleLabel = new Label
        {
            Text = "Роль:",
            Location = new Point(20, 140),
            AutoSize = true
        };
        Controls.Add(roleLabel);

        _roleComboBox = new ComboBox
        {
            Location = new Point(20, 165),
            Size = new Size(290, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _roleComboBox.Items.AddRange(new object[] { "User", "Admin" });
        _roleComboBox.SelectedItem = user.Role;
        Controls.Add(_roleComboBox);

        _saveButton = new Button
        {
            Text = "Сохранить",
            Location = new Point(20, 200),
            Size = new Size(100, 30)
        };
        _saveButton.Click += SaveButton_Click;
        Controls.Add(_saveButton);

        _cancelButton = new Button
        {
            Text = "Отмена",
            Location = new Point(210, 200),
            Size = new Size(100, 30)
        };
        _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        Controls.Add(_cancelButton);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_loginTextBox.Text))
        {
            MessageBox.Show("Введите логин", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        UpdatedLogin = _loginTextBox.Text;
        UpdatedPassword = _passwordTextBox.Text;
        UpdatedRole = _roleComboBox.SelectedItem?.ToString() ?? "User";
        DialogResult = DialogResult.OK;
    }
}
