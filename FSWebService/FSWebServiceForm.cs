using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;


namespace FSWebService
{
    public partial class FSWebServiceForm : Form
    {
        private static WebServer server = null;
        private static string port = ConfigurationManager.AppSettings["port"];
        private const int WM_USER_SIMCONNECT = 0x0402;
        private string ruleName = "FS Web Service";
        private SimController simController;
        private System.Timers.Timer simStartTimer = new System.Timers.Timer(1000);

        public FSWebServiceForm()
        {
            InitializeComponent();

        }

        private string ReceiveCallback(HttpListenerRequest request) 
        {
            // System.Diagnostics.Debug.WriteLine("{0}: {1}", request.UserHostAddress, request.RawUrl);
            string rawRequest = request.RawUrl;
            string[] parse = rawRequest.Split("?".ToCharArray());
            string command = parse[0];
            string parmstr =  parse.Length > 1 ? parse[1] : null;
            
            
            switch (command)
            {
                case "/get":
                {
                    switch (parmstr)
                    {
                        case "position":
                        {
                           // Debug.WriteLine($"Request: {command} params: {parmstr}");
                            return string.Format("{{ \"type\": \"Point\", \"coordinates\": [{0}, {1}] }}",
                                simController.userPos.Latitude, simController.userPos.Longitude);
                        }
                    }

                    return string.Empty;
                }


                default:
                {
                    string filename = "../../content/" + command.Replace("../","");
                    if (!File.Exists(filename))
                        return string.Empty;

                    return File.ReadAllText(filename);
                }
            }
        }

        public void AddText(string text)
        {
            mainTextBox.Text += (text + "\n");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mainTextBox.ScrollToCaret();
            simController = new SimController(this);
            bool animate = false;
            Debug.WriteLine("Connecting to Sim...");
            mainTextBox.Text += $" Establishing Connection to Sim ■{Environment.NewLine}";
            simStartTimer.Elapsed += (sender, ev) =>
            {
                animate = !animate;
                
                Invoke(new MethodInvoker(delegate() {mainTextBox.Text = animate ? mainTextBox.Text.Replace("■", " ")  :mainTextBox.Text.Replace(" ","■"); })); 
                Invoke(new MethodInvoker(delegate() {simController.Connect();})); 
                
                if (simController.IsConnected)
                {
                    //   AddFirewallException();
                    Invoke(new MethodInvoker(delegate() {mainTextBox.Text = $" Connected to Sim{Environment.NewLine} Starting Web Service...{Environment.NewLine}"; })); 
                    server = new WebServer(ReceiveCallback, "http://+:" + port + "/");
                    server.Run();
                    simStartTimer.Stop();
                    Invoke(new MethodInvoker(delegate() {WindowState = FormWindowState.Minimized;}));
                    return;
                }              
            };
            simStartTimer.Start();

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            simStartTimer.Stop();
            simController.Disconnect();
            notifyIcon.Visible = false; 
      //      RemoveFirewallException();
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                simController.ProcessReceive();
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void AddFirewallException() {
            using (Process p = new Process()) {
                p.StartInfo.FileName = "netsh";
                p.StartInfo.Arguments = "advfirewall firewall add rule name=\"" + ruleName + "\" dir=in action=allow protocol=TCP localport=" + port;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
            }
        }

        private void RemoveFirewallException() {
            using (Process p = new Process()) {
                p.StartInfo.FileName = "netsh";
                p.StartInfo.Arguments = "advfirewall firewall delete rule name=\"" + ruleName + "\" protocol=TCP localport=" + port;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
            }
        }

        private void mainTextBox_Resize(object sender, EventArgs e)
        {

            if (this.WindowState == FormWindowState.Minimized)  
            {  
                Hide();  
                notifyIcon.Visible = true;                  
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();  
            WindowState = FormWindowState.Normal;  
            notifyIcon.Visible = false; 
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            notifyIcon_MouseDoubleClick(sender, e);
        }
    }
}
