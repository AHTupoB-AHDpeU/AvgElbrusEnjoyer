using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
            LoadCatalogData();
            SetupListView();
            UpdateStatusLabel();
        }

        public void UpdateStatusLabel()
        {
            if (FormLogin.IsAuthenticated)
            {
                label1.Text = $"Добро пожаловать, {FormLogin.UserName}";
                button3.Enabled = true;
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                label1.Text = "Пожалуйста, авторизируйтесь на сайте.";
                button3.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void SetupListView()
        {
            listView1.View = View.Details; // Включаем режим таблицы
            listView1.Columns.Clear(); // Убираем старые колонки
            listView1.Columns.Add("Каталог", listView1.Width - 5); // Одна колонка на всю ширину
        }

        private void LoadCatalogData()
        {
            listView1.Items.Clear();

            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine("GET_CATALOG");

                    int count = int.Parse(reader.ReadLine()); // Получаем количество записей

                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('|');

                        if (parts.Length == 4)
                        {
                            ListViewItem item = new ListViewItem(parts[1]); // Name
                            item.SubItems.Add(parts[0]); // Id
                            item.SubItems.Add(parts[2]); // Category
                            item.SubItems.Add(parts[3]); // Price

                            listView1.Items.Add(item);
                        }
                    }
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent); // Автоподгонка ширины
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки каталога: {ex.Message}");
            }
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
            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine("LOGOUT");
                    string response = reader.ReadLine();
                    MessageBox.Show(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выходе: " + ex.Message);
            }

            Application.Exit();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string selectedName = selectedItem.SubItems[0].Text;
                string selectedCategory = selectedItem.SubItems[2].Text;
                string selectedPrice = selectedItem.SubItems[3].Text;
                int userId = FormLogin.UserId; // ID клиента, который авторизован

                MessageBox.Show($"Вы выбрали: {selectedName}\nОписание: {selectedCategory}\nЦена: {selectedPrice} руб.");

                int catalogId = int.Parse(selectedItem.SubItems[1].Text);
                decimal totalPrice = decimal.Parse(selectedPrice);

                SendOrderToServer(userId, catalogId, totalPrice);
            }
            /*if (listView1.SelectedItems.Count == 0 || !FormLogin.IsAuthenticated)
                return;

            ListViewItem selectedItem = listView1.SelectedItems[0];
            int catalogId = int.Parse(selectedItem.SubItems[1].Text); // ID товара
            decimal price = decimal.Parse(selectedItem.SubItems[3].Text); // Цена
            int clientId = FormLogin.UserId; // ID клиента

            // Отправка данных о заказе через TCP
            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine("CREATE_ORDER");
                    writer.WriteLine(clientId);
                    writer.WriteLine(catalogId);
                    writer.WriteLine(price);

                    string response = reader.ReadLine();
                    MessageBox.Show(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении в заказ: " + ex.Message);
            }*/
        }

        private void SendOrderToServer(int userId, int catalogId, decimal totalPrice)
        {
            try
            {
                // Создаем TCP-соединение с сервером
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    // Отправляем команду и параметры
                    writer.WriteLine("CREATE_ORDER");
                    writer.WriteLine(userId);
                    writer.WriteLine(catalogId);
                    writer.WriteLine(totalPrice);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке заказа на сервер: {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormOrder orderForm = new FormOrder();
            orderForm.ShowDialog();
        }
    }
}
