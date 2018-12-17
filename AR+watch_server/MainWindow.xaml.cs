using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AR_watch_server
{
    public partial class MainWindow : Window
    {
        public Server server;
        public Chart chart;

        public MainWindow()
        {
            InitializeComponent();
            server = new Server(this);
            server.Recv += new Server.InformHandle(recvData);
            uComboIP.ItemsSource = server.ips;
            uTextNetInfo.SetBinding(TextBlock.TextProperty, new Binding("netInfo") { Source = server });
            uTextConsole.SetBinding(TextBox.TextProperty, new Binding("console") { Source = server });
            uTextData.SetBinding(TextBox.TextProperty, new Binding("data") { Source = server });
            chart = new Chart(this, uCanvas);
        }

        private void recvData(object sender, string info)
        {
            string[] tags = info.Split(',');
            if (tags.Length == 5 && tags[0] == "Acc")
            {
                chart.add(Convert.ToSingle(tags[1]), Convert.ToSingle(tags[2]), Convert.ToSingle(tags[3]), Convert.ToSingle(tags[4]));
            }
        }

        private void uButtonListen_Click(object sender, RoutedEventArgs e)
        {
            server.Listen();
        }

        private void uButtonStop_Click(object sender, RoutedEventArgs e)
        {
            server.Stop();
        }

        private void uButtonSend_Click(object sender, RoutedEventArgs e)
        {
            server.BroadCast(uTextMessage.Text);
        }

        private void uButtonLogOn_Click(object sender, RoutedEventArgs e)
        {
            server.BroadCast("logon");
        }

        private void uButtonLogOff_Click(object sender, RoutedEventArgs e)
        {
            server.BroadCast("logoff");
        }

        private void uTextConsole_TextChanged(object sender, TextChangedEventArgs e)
        {
            uScrollConsole.ScrollToEnd();
        }
    }
}
