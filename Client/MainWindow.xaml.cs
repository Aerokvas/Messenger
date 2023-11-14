using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    public partial class MainWindow : Window
    {
        private string usersFileName = Path.Combine(@"D:\", "users.txt");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckLogin(string username, string password)
        {
            try
            {
                if (!File.Exists(usersFileName))
                {
                    MessageBox.Show("Тебя нет в базе, зарегистрируйся, дубина!");
                    return;
                }

                string[] existingUsers = File.ReadAllLines(usersFileName);
                bool userFound = false;

                foreach (string line in existingUsers)
                {
                    string[] user = line.Split(',');

                    if (user.Length >= 3 && user[0] == username && user[1] == password && user[2] == "0")
                    {
                        UpdateUserStatus(username, "1");
                        OpenChatWindow();
                        userFound = true;
                        break;
                    }
                    else if (user.Length >= 3 && user[0] == username && user[2] == "1")
                    {
                        MessageBox.Show("Пользователь уже на сервере, дубина!");
                        userFound = true;
                        break;
                    }
                }

                if (!userFound)
                {
                    MessageBox.Show("Неверное имя пользователя или пароль, дубина!");
                    this.usernameBox.Text = "";
                    this.passwordBox.Password = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateUserStatus(string username, string status)
        {
            try
            {
                if (!File.Exists(usersFileName))
                    File.Create(usersFileName).Close();

                string[] lines = File.ReadAllLines(usersFileName);
                List<string> updatedLines = new List<string>();

                foreach (string line in lines)
                {
                    string[] user = line.Split(',');
                    if (user.Length >= 3 && user[0] == username)
                        updatedLines.Add($"{user[0]},{user[1]},{status}");
                    else
                        updatedLines.Add(line);
                }

                File.WriteAllLines(usersFileName, updatedLines);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            String user = this.usernameBox.Text;
            String pass = this.passwordBox.Password;

            CheckLogin(user, pass);
        }

        private void ClearUsername(object sender, RoutedEventArgs e)
        {
            this.usernameBox.Text = "";
        }

        private void ClearPassword(object sender, MouseButtonEventArgs e)
        {
            this.passwordBox.Password = "";
        }

        private void UserForm(object sender, RoutedEventArgs e)
        {
            NewUserForm newUser = new NewUserForm();
            newUser.Show();
            Close();
        }

        private void OpenChatWindow()
        {
            ChatWindow chatWindow = new ChatWindow(this.usernameBox.Text);
            chatWindow.Show();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}