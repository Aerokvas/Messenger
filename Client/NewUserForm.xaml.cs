using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    public partial class NewUserForm : Window
    {
        public NewUserForm()
        {
            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            String newUser = this.newuser.Text;
            String newPassword = this.newpassword.Password;

            NewUserCreation(newUser, newPassword);
        }

        private void NewUserCreation(string username, string password)
        {
            string fileName = Path.Combine(@"D:\", "users.txt");

            try
            {
                if (!File.Exists(fileName))
                    File.Create(fileName).Close();

                string[] existingUsers = File.ReadAllLines(fileName);

                if (existingUsers.Any(line => line.Split(',')[0] == username))
                {
                    MessageBox.Show("Ты уже существуешь в базе, дубина!");
                    this.newuser.Text = "";
                    this.newpassword.Password = "";
                }
                else
                {
                    string newUserLine = $"{username},{password},0";
                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        sw.WriteLine(newUserLine);
                    }
                    MessageBox.Show($"Заходи {username} я создал!");
                    MainWindow backtoNorm = new MainWindow();
                    backtoNorm.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ClearUsername(object sender, RoutedEventArgs e)
        {
            this.newuser.Text = "";
        }

        private void ClearPassword(object sender, MouseButtonEventArgs e)
        {
            this.newpassword.Password = "";
        }

        private void GoBackToLogin(object sender, RoutedEventArgs e)
        {
            MainWindow goBack = new MainWindow();
            goBack.Show();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
