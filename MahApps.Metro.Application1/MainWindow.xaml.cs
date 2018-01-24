using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Diagnostics;
using LiveCharts;
using LiveCharts.Wpf;

namespace MahApps.Metro.Application1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : MetroWindow
    {

        [DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static Int32 SendArp(Int32 destIpAddress, Int32 srcIpAddress,
        byte[] macAddress, ref Int32 macAddressLength);

        public MainWindow()
        {
            InitializeComponent();
        }

        Thread myThread = null;
        private IEnumerable<NetworkInterface> interfaces;
        private object end_received_bytes;
        private object end_sent_bytes;

        public void scan(string subnet)
        {
            Ping myPing;
            PingReply reply;
            IPAddress addr;
            int count = 0;

            for (int i = 1; i <= 255; i++)
            {
                string subnetn = "." + i.ToString();
                myPing = new Ping();
                string ip = subnet.ToString() + subnetn.ToString();
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        textBlock.Text = "Scanning: " + ip;
                    }));
                    addr = IPAddress.Parse(ip);
                    reply = myPing.Send(addr, 100);
                    
                    if (reply.Status == IPStatus.Success)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            string mac = GetMacAddress(ip);
                            string host = "";

                            try
                            {
                                host = Dns.GetHostEntry(ip.Trim()).HostName.ToString();
                            }

                            catch (Exception)
                            {
                                if (ip.Equals(GetLocalIPAddress()))
                                    host = "This PC";
                                    //host = GetLocalIPAddress();
                                else
                                    host = "Not available";
                            }
                            
                            if (ip.Equals(subnet.ToString() + ".1"))
                                host = "Core Switch";

                            if (ip.Equals(GetLocalIPAddress()))
                                mac = "This PC";

                            dataGridView1.Items.Add(new Test { id = (++count).ToString(),
                                                               ip = ip.ToString(),
                                                               mac = mac.ToString(),
                                                               host = host});
                        }));
                    }
                }
                catch
                {
                    //MessageBox.Show(e.Message);
                }

                if (i == 255)
                {
                    this.Dispatcher.Invoke((Action)(async () =>
                    {
                        await this.ShowMessageAsync("Scan Completed", count + " computers are currently online in your department.");
                        button1.Content = "Start Scan";
                    }));
                }
            }
        }

        /*
        private string getData()
        {
            NetworkInterface[] interfaces
                = NetworkInterface.GetAllNetworkInterfaces();

            string data = "";
            
            foreach (NetworkInterface ni in interfaces)
            {                
                    double sent = Convert.ToDouble(ni.GetIPv4Statistics().BytesSent);
                    double recv = Convert.ToDouble(ni.GetIPv4Statistics().BytesReceived);
                    double sent_GB = sent / 1024 / 1024 / 1024;
                    double recv_GB = recv / 1024 / 1024 / 1024;
                    data = "Upload: " + sent_GB + " GB, Download: " + recv_GB + " GB";
            }
            return data;
        }*/
        /*public double getNetworkUtilization(string networkCard)
        {

            const int numberOfIterations = 10;

            PerformanceCounter bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
            float bandwidth = bandwidthCounter.NextValue();//valor fixo 10Mb/100Mn/

            PerformanceCounter dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);

            PerformanceCounter dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

            float sendSum = 0;
            float receiveSum = 0;

            for (int index = 0; index < numberOfIterations; index++)
            {
                sendSum += dataSentCounter.NextValue();
                receiveSum += dataReceivedCounter.NextValue();
            }
            float dataSent = sendSum;
            float dataReceived = receiveSum;


            double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
            return utilization;
        }
        */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (button1.Content.ToString() == "Restart Scan")
            {
                myThread.Abort();
            }

            dataGridView1.Items.Clear();
            textBlock.Visibility = Visibility.Visible;
            button1.Content = "Restart Scan";
            button2.Visibility = Visibility.Visible;
            myThread = new Thread(() => scan(Properties.Settings.Default.subnet.ToString()));
            myThread.Start();

        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {      
            if(button2.Content.Equals("Chart View"))
                button2.Content = "Grid View";
            else
                button2.Content = "Chart View";
        }
        
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + " : " + substrings[4] + " : " + substrings[5] + " : " + substrings[6]
                         + " : " + substrings[7] + " : "
                         + substrings[8].Substring(0, 2);
                return macAddress.ToUpper();
            }
            else
            {
                return "Not available";
            }
        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("Not available");
        }

        public string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }
        
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (button1.Content.ToString() == "Restart Scan")
                myThread.Abort();

            textBlock.Visibility = Visibility.Hidden;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() => scan(Properties.Settings.Default.subnet.ToString()));            
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.mac != null)
                this.Title += " - Subnet: " + Properties.Settings.Default.subnet + ".250";

            if ((bool)Properties.Settings.Default.firstRun == true)
            {
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.Save();

                Window1 a = new Window1();
                a.ShowDialog();
            }

            string localmac = GetMACAddress();
            //MessageBox.Show(localmac);
            if (localmac != Properties.Settings.Default.mac)
            {
                await this.ShowMessageAsync("Error", "You are not registered to use this application, please contact administrator.");
                this.Hide();
                Window1 a = new Window1();
                a.not_re = true;
                a.ShowDialog();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window1 a = new Window1();
            a.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            about a = new about();
            a.ShowDialog();
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }

    public class Test
    {
        public string id { get; set; }
        public string ip { get; set; }
        public string mac { get; set; }
        public string host { get; set; }
        public string data { get; set; }
    }
}
