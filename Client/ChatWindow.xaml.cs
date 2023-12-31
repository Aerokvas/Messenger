﻿using SimpleTCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Client
{
    public partial class ChatWindow : Window
    {
        private string username;
        Boolean ifFileSelected = false;

        SimpleTcpClient client;
        private string usersFileName = Path.Combine(@"D:\", "users.txt");

        public ChatWindow(string usr)
        {
            InitializeComponent();
            username = usr;
            this.Title = usr;
        }

        private void ChatWindowLoaded(object sender, EventArgs e)
        {
            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.Connect("127.0.0.1", 1111);
                client.Write(">>> " + username + " прибыл! <<<\n");
                btnConnect.IsEnabled = false;
                connectedToTheServerAs.Text = "Ты на сервере: " + username;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Сервер пока спит");
                btnConnect.IsEnabled = true;
            }
        }

        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            if (e.MessageString.Length > 4 && e.MessageString.Contains("list"))
            {
                String msg = e.MessageString.Substring(4, e.MessageString.Length - 4);
                string[] listClients = new string[msg.Split(':').Length - 2];
                int cp = 0;
                for (int i = 0; i < msg.Split(':').Length; i++)
                {
                    if (msg.Split(':')[i] != "" && msg.Split(':')[i] != this.username)
                    {
                        listClients[cp] = msg.Split(':')[i];
                        cp++;
                    }
                }
                listbox.Items.Dispatcher.Invoke((Action)delegate ()
                {
                    listbox.ItemsSource = listClients;
                });
            }
            else
            {
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    if (e.MessageString.Contains("прибыл") && !e.MessageString.Contains(username))
                    {
                        txtStatus.Text += e.MessageString;
                    }
                    else if (e.MessageString.Contains("прибыл") && e.MessageString.Contains(username))
                    {
                        txtStatus.Text += "";
                    }
                    else
                    {
                        txtStatus.Text += e.MessageString;
                    }
                });

            }

            if (e.MessageString.Equals("Все"))
            {
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += "";
                });

            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.IsEnabled)
            {
                System.Windows.MessageBox.Show("Нажми connect, дубина!");
            }
            else if (listbox.SelectedIndex != -1)
            {
                if (txtMessage.Text.Length > 0 && !ifFileSelected && !txtMessage.Text.Equals(" ") && !txtMessage.Text.Equals("\n"))
                {
                    client.WriteLine(listbox.SelectedItem.ToString() + "#" + username + ": " + txtMessage.Text);
                }
                else
                {
                    return;
                    //System.Windows.MessageBox.Show("Please type something or choose a file to send!");
                }
            }
            else
            {
                    return;
                //System.Windows.MessageBox.Show("Please select someone to send the message to!");
            }

            txtMessage.Text = "";

        }

        private void txtMessage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!btnConnect.IsEnabled)
                client.WriteLine("list");
            else
                System.Windows.MessageBox.Show("Нажми connect, дубина!!");

        }

        private void Click_clear(object sender, EventArgs e)
        {
            txtStatus.Text = "";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!btnConnect.IsEnabled)
            {
                client.Write(username + ":closed");
                client.TcpClient.Close();
            }
            UpdateUserStatus(username, "0");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateUserStatus(username, "0");
            Close();
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
                    if (user.Length >= 2 && user[0] == username)
                    {
                        updatedLines.Add($"{user[0]},{user[1]},{status}");
                    }
                    else
                    {
                        updatedLines.Add(line);
                    }
                }

                File.WriteAllLines(usersFileName, updatedLines);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
