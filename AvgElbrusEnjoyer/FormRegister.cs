using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace AvgElbrusEnjoyer
{
    public partial class FormRegister : Form
    {
        public FormRegister()
        {
            InitializeComponent();
        }

        private void regist_Click(object sender, EventArgs e)
        {
            RegisterUser();
        }

        private void RegisterUser()
        {
            string name = textBoxUsername.Text.Trim();
            string email = textBoxEmail.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    // Отправка команды регистрации
                    writer.WriteLine("REGISTER");

                    // Отправка данных для регистрации
                    writer.WriteLine(name);
                    writer.WriteLine(email);
                    writer.WriteLine(password);

                    // Получение ответа от сервера
                    string response = reader.ReadLine();
                    MessageBox.Show(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}");
            }
        }
    }
}
