using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            Thread.Sleep(1000);
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

        [Fact]
        public void TestServerHandlesLoginSuccess()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("LOGIN");
            writer.WriteLine("user2@mail.ru");
            writer.WriteLine("user2");

            string response = reader.ReadLine();

            Assert.Equal("Добро пожаловать, User2", response);
        }

        [Fact]
        public void TestServerHandlesLogout()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("LOGOUT");
            string response = reader.ReadLine();

            Assert.Equal("Выход из системы выполнен.", response);
        }

        [Fact]
        public void TestRegisterExistingUser()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("REGISTER");
            writer.WriteLine("test1");
            writer.WriteLine("user2@mail.ru"); // Этот email уже есть в БД
            writer.WriteLine("111");
            writer.WriteLine("Pskov");
            writer.WriteLine("password123");

            string response = reader.ReadLine();
            Assert.Equal("Пользователь с таким email уже существует.", response);
        }

        [Fact]
        public void TestRegisterNewUser()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("REGISTER");
            writer.WriteLine("test1");
            writer.WriteLine("test1@mail.ru"); // Новый email (при повторе поменять)
            writer.WriteLine("111");
            writer.WriteLine("Pskov");
            writer.WriteLine("test1");

            string response = reader.ReadLine();
            Assert.Equal("Пользователь test1 зарегистрирован.", response);
        }

        [Fact]
        public void TestSendCatalog()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("GET_CATALOG");

            int catalogCount = int.Parse(reader.ReadLine());
            Assert.True(catalogCount > 0); // Должно быть больше 0 элементов в каталоге

            for (int i = 0; i < catalogCount; i++)
            {
                string catalogData = reader.ReadLine();
                Assert.NotNull(catalogData); // Данные не пустые

                string[] catalogFields = catalogData.Split('|');
                Assert.Equal(4, catalogFields.Length);

                int catalogId = int.Parse(catalogFields[0]);
                Assert.True(catalogId > 0);

                string name = catalogFields[1];
                Assert.False(string.IsNullOrEmpty(name));

                string category = catalogFields[2];
                Assert.False(string.IsNullOrEmpty(category));

                decimal price = decimal.Parse(catalogFields[3]);
                Assert.True(price >= 0);
            }
        }

        // SQL запросы (менять?)
        [Fact]
        public void TestSendUserId_ExistingUser() 
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("GET_USER_ID");
            writer.WriteLine("user2@mail.ru");

            string response = reader.ReadLine();
            int userId = int.Parse(response);
            Assert.True(userId > 0);
        }

        [Fact]
        public void TestSendUserId_NonExistingUser()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("GET_USER_ID");
            writer.WriteLine("nonexistent@mail.ru"); // Пользователя нет в БД

            string response = reader.ReadLine();
            Assert.Equal("Пользователь не найден.", response);
        }

        [Fact]
        public void TestSqlSearchQuery()
        {
            string searchQuery = "SELECT * FROM Clients WHERE Email = 'test1@mail.ru'";

            using var db = new AppDbContext();
            var result = db.Clients.FromSqlRaw(searchQuery).ToList();

            Assert.NotEmpty(result);
            Assert.Equal("test1@mail.ru", result[0].Email);
        }

        [Fact]
        public void TestSqlInsertQuery()
        {
            string insertQuery = "INSERT INTO Clients (Name, Email, Phone, Address, Password) " +
                                 "VALUES ('test2', 'test2@mail.ru', '1234567890', 'Pskov', 'test2')";

            ExecuteSqlQuery(insertQuery);

            using var db = new AppDbContext();
            var newClient = db.Clients.FirstOrDefault(c => c.Email == "test2@mail.ru");

            Assert.NotNull(newClient);
            Assert.Equal("test2", newClient.Name);
        }

        [Fact]
        public void TestSqlUpdateQuery()
        {
            string insertQuery = "INSERT INTO Clients (Name, Email, Phone, Address, Password) " +
                                 "VALUES ('test3', 'test3@mail.ru', '1234567890', 'Pskov', 'test3')";
            ExecuteSqlQuery(insertQuery);

            string updateQuery = "UPDATE Clients SET Name = 'test3+' WHERE Email = 'test3@mail.ru'";
            ExecuteSqlQuery(updateQuery);

            using var db = new AppDbContext();
            var updatedClient = db.Clients.FirstOrDefault(c => c.Email == "test3@mail.ru");

            Assert.NotNull(updatedClient);
            Assert.Equal("test3+", updatedClient.Name);
        }

        [Fact]
        public void TestSqlDeleteQuery()
        {
            string insertQuery = "INSERT INTO Clients (Name, Email, Phone, Address, Password) " +
                                 "VALUES ('test4', 'test4@mail.ru', '1234567890', 'Pskov', 'test4')";
            ExecuteSqlQuery(insertQuery);

            string deleteQuery = "DELETE FROM Clients WHERE Email = 'test4@mail.ru'";
            ExecuteSqlQuery(deleteQuery);

            using var db = new AppDbContext();
            var deletedClient = db.Clients.FirstOrDefault(c => c.Email == "test4@mail.ru");

            Assert.Null(deletedClient);
        }

        public static void ExecuteSqlQuery(string sqlQuery)
        {
            using var db = new AppDbContext();

            // Выполнение SQL-запроса
            var result = db.Database.ExecuteSqlRaw(sqlQuery);

            Console.WriteLine($"Запрос выполнен успешно.");
        }

        [Fact]
        public void TestCreateOrder()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            int userId = 14;
            int catalogId = 1;
            decimal totalPrice = 666;

            writer.WriteLine("CREATE_ORDER");
            writer.WriteLine(userId.ToString());
            writer.WriteLine(catalogId.ToString());
            writer.WriteLine(totalPrice.ToString());

            string response = reader.ReadLine();

            Assert.Equal("Заказ создан.", response);

            using var db = new AppDbContext();
            var order = db.Orders
                .FirstOrDefault(o => o.ClientId == userId && o.CatalogId == catalogId && o.TotalPrice == totalPrice);

            Assert.NotNull(order);  // Заказ существует в базе
            Assert.Equal(userId, order.ClientId);
            Assert.Equal(catalogId, order.CatalogId);
            Assert.Equal(totalPrice, order.TotalPrice);
        }

        [Fact]
        public void TestGetOrders()
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine("GET_ORDERS");

            int orderCount = int.Parse(reader.ReadLine());

            Assert.True(orderCount > 0);

            for (int i = 0; i < orderCount; i++)
            {
                string orderData = reader.ReadLine();
                string[] orderFields = orderData.Split('|');

                Assert.Equal(4, orderFields.Length);

                int orderId = int.Parse(orderFields[0]);
                string clientName = orderFields[1];
                string catalogName = orderFields[2];
                decimal totalPrice = decimal.Parse(orderFields[3]);

                Assert.True(orderId > 0); 
                Assert.True(totalPrice >= 0);

                using var db = new AppDbContext();
                var order = db.Orders
                    .Join(db.Clients, o => o.ClientId, c => c.Id, (o, c) => new { o, c.Name })
                    .Join(db.Catalogs, oc => oc.o.CatalogId, cat => cat.Id, (oc, cat) => new
                    {
                        oc.o.Id,
                        ClientName = oc.Name,
                        CatalogName = cat.Name,
                        oc.o.TotalPrice
                    })
                    .FirstOrDefault(o => o.Id == orderId);

                Assert.NotNull(order);
                Assert.Equal(clientName, order.ClientName);
                Assert.Equal(catalogName, order.CatalogName);
                Assert.Equal(totalPrice, order.TotalPrice);
            }
        }

    }
}
