using ProtocolSample;
using SSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientSample
{
    public partial class View : Form
    {
        SSyncClient Client = new SSyncClient();
        public static View Self;
        public View()
        {
            InitializeComponent();
            Self = this;
            SSyncCore.Initialize(Assembly.GetAssembly(typeof(ChatMessage)), Assembly.GetAssembly(typeof(View)));
            Client.OnConnected += Client_OnConnected;
            Client.OnFailedToConnect += Client_OnFailedToConnect;
            richTextBox2.KeyPress += richTextBox2_KeyPress;

        }

        void richTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                richTextBox2.Text = richTextBox2.Text.Replace("\n", string.Empty);
                button2_Click(null, null);
            }
        }
        


        void Client_OnFailedToConnect(Exception ex)
        {
            MessageBox.Show("Impossible de se connecter au serveur");
        }


        public void OnChatMessageReceived(string message)
        {
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(message + Environment.NewLine)));
            richTextBox1.Invoke(new Action(() => richTextBox1.ScrollToCaret()));
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Client.Connect("127.0.0.1", 500);

        }

        void Client_OnConnected()
        {
            button1.Hide();
            richTextBox1.Show();
            richTextBox2.Show();
            button2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Client.Send(new ChatRequestMessage(richTextBox2.Text));
            richTextBox2.Clear();

        }


    }
}
