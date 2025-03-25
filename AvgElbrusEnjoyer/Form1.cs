using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace AvgElbrusEnjoyer
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormLogin loginForm = new FormLogin();
            loginForm.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormRegister registerForm = new FormRegister();
            registerForm.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Закрывает всё приложение
        }
    }
}
