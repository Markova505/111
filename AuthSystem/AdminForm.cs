using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace AuthSystem
{
    public partial class AdminForm : Form
    {
        private string currentAdminLogin;

        public AdminForm(string adminLogin)
        {
            currentAdminLogin = adminLogin;
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = $"Панель администратора - {currentAdminLogin}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var titleLabel = new Label
            {
                Text = "Управление пользователями",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(titleLabel);

            // DataGridView для отображения пользователей
            var usersGrid = new DataGridView
            {
                Name = "usersGrid",
                Location = new Point(20, 60),
                Size = new Size(840, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            usersGrid.Columns.Add("Id", "ID");
            usersGrid.Columns.Add("Login", "Логин");
            usersGrid.Columns.Add("Role", "Роль");
            usersGrid.Columns.Add("IsBlocked", "Заблокирован");
            this.Controls.Add(usersGrid);

            // Группа добавления нового пользователя
            var addGroup = new GroupBox
            {
                Text = "Добавить нового пользователя",
                Location = new Point(20, 320),
                Size = new Size(400, 180)
            };

            var addLoginLabel = new Label
            {
                Text = "Логин:",
                AutoSize = true,
                Location = new Point(15, 25)
            };
            addGroup.Controls.Add(addLoginLabel);

            var addLoginTextBox = new TextBox
            {
                Name = "addLoginTextBox",
                Location = new Point(15, 50),
                Size = new Size(170, 25)
            };
            addGroup.Controls.Add(addLoginTextBox);

            var addPasswordLabel = new Label
            {
                Text = "Пароль:",
                AutoSize = true,
                Location = new Point(200, 25)
            };
            addGroup.Controls.Add(addPasswordLabel);

            var addPasswordTextBox = new TextBox
            {
                Name = "addPasswordTextBox",
                Location = new Point(200, 50),
                Size = new Size(170, 25),
                PasswordChar = '*'
            };
            addGroup.Controls.Add(addPasswordTextBox);

            var addRoleLabel = new Label
            {
                Text = "Роль:",
                AutoSize = true,
                Location = new Point(15, 85)
            };
            addGroup.Controls.Add(addRoleLabel);

            var addRoleComboBox = new ComboBox
            {
                Name = "addRoleComboBox",
                Location = new Point(15, 110),
                Size = new Size(170, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            addRoleComboBox.Items.Add("Пользователь");
            addRoleComboBox.Items.Add("Администратор");
            addRoleComboBox.SelectedIndex = 0;
            addGroup.Controls.Add(addRoleComboBox);

            var addButton = new Button
            {
                Text = "Добавить",
                Location = new Point(200, 105),
                Size = new Size(170, 35)
            };
            addButton.Click += AddButton_Click;
            addGroup.Controls.Add(addButton);

            this.Controls.Add(addGroup);

            // Группа редактирования пользователя
            var editGroup = new GroupBox
            {
                Text = "Редактировать пользователя",
                Location = new Point(440, 320),
                Size = new Size(420, 180)
            };

            var editLoginLabel = new Label
            {
                Text = "Логин:",
                AutoSize = true,
                Location = new Point(15, 25)
            };
            editGroup.Controls.Add(editLoginLabel);

            var editLoginTextBox = new TextBox
            {
                Name = "editLoginTextBox",
                Location = new Point(15, 50),
                Size = new Size(170, 25),
                ReadOnly = true
            };
            editGroup.Controls.Add(editLoginTextBox);

            var editPasswordLabel = new Label
            {
                Text = "Новый пароль:",
                AutoSize = true,
                Location = new Point(200, 25)
            };
            editGroup.Controls.Add(editPasswordLabel);

            var editPasswordTextBox = new TextBox
            {
                Name = "editPasswordTextBox",
                Location = new Point(200, 50),
                Size = new Size(170, 25),
                PasswordChar = '*'
            };
            editGroup.Controls.Add(editPasswordTextBox);

            var editRoleLabel = new Label
            {
                Text = "Роль:",
                AutoSize = true,
                Location = new Point(15, 85)
            };
            editGroup.Controls.Add(editRoleLabel);

            var editRoleComboBox = new ComboBox
            {
                Name = "editRoleComboBox",
                Location = new Point(15, 110),
                Size = new Size(170, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            editRoleComboBox.Items.Add("Пользователь");
            editRoleComboBox.Items.Add("Администратор");
            editGroup.Controls.Add(editRoleComboBox);

            var unblockCheckBox = new CheckBox
            {
                Name = "unblockCheckBox",
                Text = "Снять блокировку",
                Location = new Point(200, 85),
                AutoSize = true
            };
            editGroup.Controls.Add(unblockCheckBox);

            var editButton = new Button
            {
                Text = "Сохранить",
                Location = new Point(200, 110),
                Size = new Size(170, 35)
            };
            editButton.Click += EditButton_Click;
            editGroup.Controls.Add(editButton);

            this.Controls.Add(editGroup);

            // Обработчик выбора строки
            usersGrid.SelectionChanged += UsersGrid_SelectionChanged;

            var refreshButton = new Button
            {
                Text = "Обновить список",
                Location = new Point(20, 520),
                Size = new Size(150, 35)
            };
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            var logoutButton = new Button
            {
                Text = "Выйти",
                Location = new Point(720, 520),
                Size = new Size(150, 35)
            };
            logoutButton.Click += LogoutButton_Click;
            this.Controls.Add(logoutButton);
        }

        private void LoadUsers()
        {
            var usersGrid = this.Controls.Find("usersGrid", true)[0] as DataGridView;
            if (usersGrid == null) return;

            usersGrid.Rows.Clear();

            using (var reader = DatabaseHelper.GetAllUsers())
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["Id"]);
                    string login = reader["Логин"].ToString();
                    string role = reader["Роль"].ToString();
                    bool isBlocked = Convert.ToBoolean(reader["Заблокирован"]);

                    usersGrid.Rows.Add(id, login, role, isBlocked ? "Да" : "Нет");
                }
            }
        }

        private void UsersGrid_SelectionChanged(object? sender, EventArgs e)
        {
            var usersGrid = sender as DataGridView;
            if (usersGrid == null || usersGrid.SelectedRows.Count == 0) return;

            var row = usersGrid.SelectedRows[0];
            
            var editLoginTextBox = this.Controls.Find("editLoginTextBox", true)[0] as TextBox;
            var editRoleComboBox = this.Controls.Find("editRoleComboBox", true)[0] as ComboBox;
            var unblockCheckBox = this.Controls.Find("unblockCheckBox", true)[0] as CheckBox;

            if (editLoginTextBox != null && editRoleComboBox != null && unblockCheckBox != null)
            {
                string login = row.Cells["Login"].Value?.ToString() ?? "";
                string role = row.Cells["Role"].Value?.ToString() ?? "";
                string isBlocked = row.Cells["IsBlocked"].Value?.ToString() ?? "Нет";

                editLoginTextBox.Text = login;
                editRoleComboBox.SelectedItem = role;
                unblockCheckBox.Checked = (isBlocked == "Да");
            }
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            var addLoginTextBox = this.Controls.Find("addLoginTextBox", true)[0] as TextBox;
            var addPasswordTextBox = this.Controls.Find("addPasswordTextBox", true)[0] as TextBox;
            var addRoleComboBox = this.Controls.Find("addRoleComboBox", true)[0] as ComboBox;

            if (addLoginTextBox == null || addPasswordTextBox == null || addRoleComboBox == null) return;

            string login = addLoginTextBox.Text.Trim();
            string password = addPasswordTextBox.Text;
            string role = addRoleComboBox.SelectedItem?.ToString() ?? "Пользователь";

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (DatabaseHelper.UserExists(login))
            {
                MessageBox.Show($"Пользователь с логином '{login}' уже существует", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (DatabaseHelper.AddUser(login, password, role))
            {
                MessageBox.Show("Пользователь успешно добавлен", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                addLoginTextBox.Clear();
                addPasswordTextBox.Clear();
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении пользователя", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            var editLoginTextBox = this.Controls.Find("editLoginTextBox", true)[0] as TextBox;
            var editPasswordTextBox = this.Controls.Find("editPasswordTextBox", true)[0] as TextBox;
            var editRoleComboBox = this.Controls.Find("editRoleComboBox", true)[0] as ComboBox;
            var unblockCheckBox = this.Controls.Find("unblockCheckBox", true)[0] as CheckBox;

            if (editLoginTextBox == null || editRoleComboBox == null || unblockCheckBox == null) return;

            string login = editLoginTextBox.Text.Trim();
            string newPassword = editPasswordTextBox.Text;
            string newRole = editRoleComboBox.SelectedItem?.ToString() ?? "Пользователь";
            bool unblock = unblockCheckBox.Checked;

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Выберите пользователя из списка", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (DatabaseHelper.UpdateUser(login, newPassword, newRole, unblock))
            {
                MessageBox.Show("Данные пользователя обновлены", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                editPasswordTextBox.Clear();
                unblockCheckBox.Checked = false;
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении данных", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LogoutButton_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
