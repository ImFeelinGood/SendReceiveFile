using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace RecieveFile_Server
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private ArrayList alSockets;

        public Form1()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
            lblStatus.Text = "Server IP Address: " + IPHost.AddressList[0].ToString();

            alSockets = new ArrayList();

            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.Start();
        }

        private void UpdateListBox(string message)
        {
            if (lbConnections.InvokeRequired)
            {
                lbConnections.Invoke(new MethodInvoker(() => UpdateListBox(message)));
            }
            else
            {
                lbConnections.Items.Add(message);
            }
        }

        private void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    UpdateListBox(handlerSocket.RemoteEndPoint.ToString() + " connected.");
                    lock (this)
                    {
                        alSockets.Add(handlerSocket);
                    }
                    Thread thdHandler = new Thread(handlerThread);
                    thdHandler.Start();
                }
            }
        }

        public void handlerThread()
        {
            Socket handlerSocket = (Socket)alSockets[alSockets.Count - 1];
            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];

            try
            {
                using (Stream fileStream = File.OpenWrite("C:/Users/dheda/Documents/test.txt"))
                {
                    while (true)
                    {
                        thisRead = networkStream.Read(dataByte, 0, blockSize);
                        if (thisRead == 0) break;
                        fileStream.Write(dataByte, 0, thisRead);
                    }
                }

                UpdateListBox("File Written");
            }
            catch (Exception ex)
            {
                UpdateListBox("Error: " + ex.Message);
            }
            finally
            {
                handlerSocket.Close();
                handlerSocket = null;
            }
        }

    }
}
