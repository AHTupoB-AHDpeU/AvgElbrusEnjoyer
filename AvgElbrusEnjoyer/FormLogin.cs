using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace AvgElbrusEnjoyer
{
    public partial class FormLogin : Form
    {
        public static bool IsAuthenticated { get; private set; }
        public static string UserName { get; private set; }
        public static int UserId { get; private set; }

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
                    writer.WriteLine("LOGIN");

                    writer.WriteLine(email);
                    writer.WriteLine(password);

                    string response = reader.ReadLine();
                    MessageBox.Show(response + "!");

                    if (response.Contains("Добро пожаловать"))
                    {
                        IsAuthenticated = true;
                        UserName = response.Replace("Добро пожаловать, ", "").Trim();
                        UserId = GetUserId(email);

                        if (Application.OpenForms["Form1"] is Form1 form1)
                        {
                            form1.UpdateStatusLabel();
                        }
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        IsAuthenticated = false;
                        UserName = null;
                        UserId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}");
            }
        }

        private int GetUserId(string email)
        {
            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine("GET_USER_ID");
                    writer.WriteLine(email);

                    string response = reader.ReadLine();

                    if (int.TryParse(response, out int userId))
                    {
                        return userId;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении UserId: {ex.Message}");
            }
            return 0;
        }
    }
}
