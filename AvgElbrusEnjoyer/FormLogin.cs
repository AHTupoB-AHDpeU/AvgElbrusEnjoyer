using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace AvgElbrusEnjoyer
{
    public partial class FormLogin : Form
    {
        public static bool IsAuthenticated { get; private set; }
        public static string UserName { get; private set; }

        public FormLogin()
        {
            InitializeComponent();
        }

        private void textBoxPassword1_Click(object sender, EventArgs e)
        {
            LoginUser();
        }

        private void LoginUser()
        {
            string email = textBoxEmail.Text.Trim();
            string password = textBoxPassword1.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    // Отправка команды для авторизации
                    writer.WriteLine("LOGIN");

                    // Отправка данных для авторизации
                    writer.WriteLine(email);
                    writer.WriteLine(password);

                    // Получение ответа от сервера
                    string response = reader.ReadLine();
                    MessageBox.Show(response);

                    // Если авторизация успешна, то можно настроить переменные
                    if (response.Contains("Добро пожаловать"))
                    {
                        IsAuthenticated = true;
                        UserName = email;  // Или имя пользователя, если нужно
                    }
                    else
                    {
                        IsAuthenticated = false;
                        UserName = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}");
            }
        }
    }
}
