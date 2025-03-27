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

namespace AvgElbrusEnjoyer
{
    public partial class FormOrder : Form
    {
        public FormOrder()
        {
            InitializeComponent();
            LoadOrderData();
        }

        private void LoadOrderData()
        {
            listBox1.Items.Clear();

            try
            {
                // Создаем TCP-соединение с сервером
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    // Запрос к серверу на получение данных заказов
                    writer.WriteLine("GET_ORDERS");

                    // Читаем количество заказов
                    int count = int.Parse(reader.ReadLine());

                    // Читаем данные заказов и добавляем их в ListBox
                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('|');

                        if (parts.Length == 4)
                        {
                            // Формируем строку для отображения в ListBox
                            string itemText = $"Order ID: {parts[0]}, Client ID: {parts[1]}, Catalog ID: {parts[2]}, Total Price: {parts[3]} руб.";
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
