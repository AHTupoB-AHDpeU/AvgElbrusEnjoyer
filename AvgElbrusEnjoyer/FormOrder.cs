using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AvgElbrusEnjoyer
{
    public partial class FormOrder : Form
    {
        private string currentUserName;

        public FormOrder(string userName)
        {
            currentUserName = userName;
            InitializeComponent();
            LoadOrderData();
        }

        private void LoadOrderData()
        {
            listBox1.Items.Clear();

            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine("GET_ORDERS");

                    int count = int.Parse(reader.ReadLine());
                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('|');

                        if (parts.Length == 4 && parts[1] == currentUserName)
                        {
                            string itemText = $"{parts[2]} {parts[3]} руб.";
                            listBox1.Items.Add(itemText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
