using SimpleTCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Server
{
    public partial class MainWindow : Window
    {
        
        SimpleTcpServer server;
        List<ClientDetails> listOfClients = new List<ClientDetails>();

        class ClientDetails
        {
            String username;
            Socket userSocket;

            public ClientDetails(string username, Socket userSocket)
            {
                this.username = username;
                this.userSocket = userSocket;
            }

            public String getUsername()
            {
                return this.username;
            }

            public Socket getSocket()
            {
                return this.userSocket;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerLoaded(object sender, RoutedEventArgs e)
        {
            server = new SimpleTcpServer();
            server.Delimiter = 0x13; //enter key
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            string listToBeSent = "list:Все:";

            if (e.MessageString.Contains("прибыл"))
            {
                String user = e.MessageString.Substring(4, e.MessageString.Length - 4 - 18);
                Socket socket = e.TcpClient.Client;
                listOfClients.Add(new ClientDetails(user, socket));

                server.Broadcast(e.MessageString);
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += e.MessageString;
                });
            }

            if (e.MessageString.Contains("list"))
            {
                foreach (ClientDetails username in listOfClients)
                {
                    listToBeSent += username.getUsername() + ":";
                }
                List<byte> vs = new List<byte>();
                vs.AddRange(Encoding.UTF8.GetBytes(listToBeSent));
                e.TcpClient.Client.Send(vs.ToArray());
            }

            if (e.MessageString.Contains("#"))
            {
                if (e.MessageString.Substring(0, 3).Equals("Все"))
                {
                    if (!e.MessageString.Substring(4, 4).Equals("File"))
                    {
                        string toAll = "[Все] " + e.MessageString.Substring(4, e.MessageString.Length - 4) + "\n";
                        server.Broadcast(toAll);
                        txtStatus.Dispatcher.Invoke((Action)delegate ()
                        {
                            txtStatus.Text += toAll;
                        });
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Cannot send a file to everyone!");
                    }
                }
                else
                {
                    int posOfDel = e.MessageString.IndexOf('#');
                    string user = e.MessageString.Substring(0, posOfDel);
                    //if its not a file
                    if (!e.MessageString.Substring(user.Length + 1, 4).Equals("File"))
                    {
                        string msg = "[Приватка] " + e.MessageString.Substring(posOfDel + 1, e.MessageString.Length - posOfDel - 1) + "\n";
                        Socket replySock = findSocket(user, listOfClients);
                        if (replySock != null)
                        {
                            List<byte> vs = new List<byte>();
                            vs.AddRange(Encoding.UTF8.GetBytes(msg));
                            replySock.Send(vs.ToArray());
                            e.TcpClient.Client.Send(vs.ToArray());
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(user + " вышел покурить!");
                        }
                    }
                    
                }
            }

            if (e.MessageString.Contains("closed"))
            {
                string username = e.MessageString.ToString().Split(':')[0];

                for (int i = 0; i < listOfClients.Count(); i++)
                {
                    if (listOfClients[i].getUsername() == username)
                        listOfClients.RemoveAt(i);
                }

                server.Broadcast(">>> " + username + " вышел покурить! <<<\n");

                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += ">>> " + username + " вышел покурить! <<<\n";
                });

            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            server.Start(ip, 1111); ;
            btnStart.IsEnabled = false;
            txtStatus.Text = "Сервер воскрес!\n";
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (server.IsStarted)
            {
                server.Broadcast(">>>Сервер умер, попробуй потом!<<<");
                server.Stop();
                btnStart.IsEnabled = true;
                txtStatus.Text = "Сервер умер.\n";
            }
        }

        private Socket findSocket(string user, List<ClientDetails> list)
        {
            Socket userSocket;
            int counter = 0;

            foreach (ClientDetails details in list)
            {
                if (details.getUsername().Equals(user)) break;
                counter++;
            }

            ClientDetails resSock = list.ElementAt(counter);
            userSocket = resSock.getSocket();

            return userSocket;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
