using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp.Tests
{
    public class ServerTests
    {
        private Thread serverThread;

        public ServerTests()
        {
            serverThread = new Thread(() => Program.Main(new string[0]));
            serverThread.IsBackground = true;
            serverThread.Start();
            Thread.Sleep(1000); // Даем серверу время на запуск
        }

        [Fact]
        public void TestServerRespondsToInvalidCommand()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("UNKNOWN_COMMAND");
            string response = reader.ReadLine();

            Assert.Equal("Неверная команда.", response);
        }

        [Fact]
        public void TestServerHandlesLoginFailure()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("LOGIN");
            writer.WriteLine("nonexistent@example.com");
            writer.WriteLine("wrongpassword");

            string response = reader.ReadLine();

            Assert.Equal("Неверный email или пароль.", response);
        }
    }
}
