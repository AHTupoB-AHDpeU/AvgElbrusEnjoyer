// Паттерн
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class Program
{
    public interface ICommand
    {
        void Execute(StreamWriter writer, StreamReader reader);
    }

    public class RegisterCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            using var db = new AppDbContext();
            string name = reader.ReadLine();
            string email = reader.ReadLine();
            string phone = reader.ReadLine();
            string address = reader.ReadLine();
            string password = reader.ReadLine();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            if (db.Clients.Any(c => c.Email == email))
            {
                writer.WriteLine("Пользователь с таким email уже существует.");
                return;
            }

            var newClient = new Client
            {
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                Password = hashedPassword
            };

            db.Clients.Add(newClient);
            db.SaveChanges();
            writer.WriteLine($"Пользователь {name} зарегистрирован.");
        }
    }

    public class LoginCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            using var db = new AppDbContext();
            string email = reader.ReadLine();
            string password = reader.ReadLine();
            var client = db.Clients.FirstOrDefault(c => c.Email == email);

            if (client == null || !BCrypt.Net.BCrypt.Verify(password, client.Password))
            {
                writer.WriteLine("Неверный email или пароль.");
                return;
            }

            writer.WriteLine($"Добро пожаловать, {client.Name}");
        }
    }

    public class LogoutCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            writer.WriteLine("Выход из системы выполнен.");
            writer.Flush();
        }
    }

    public class GetCatalogCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            using var db = new AppDbContext();
            var catalogs = db.Catalogs.ToList();
            writer.WriteLine(catalogs.Count);
            foreach (var item in catalogs)
            {
                writer.WriteLine($"{item.Id}|{item.Name}|{item.Category}|{item.Price}");
            }
        }
    }

    public class CreateOrderCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            int userId = int.Parse(reader.ReadLine());
            int catalogId = int.Parse(reader.ReadLine());
            decimal totalPrice = decimal.Parse(reader.ReadLine());

            using var db = new AppDbContext();
            var order = new Order
            {
                ClientId = userId,
                CatalogId = catalogId,
                TotalPrice = totalPrice
            };

            db.Orders.Add(order);
            db.SaveChanges();
            writer.WriteLine("Заказ создан.");
        }
    }

    public class GetOrdersCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            using var db = new AppDbContext();
            var orders = db.Orders
                .Join(db.Clients, o => o.ClientId, c => c.Id, (o, c) => new { o, c.Name })
                .Join(db.Catalogs, oc => oc.o.CatalogId, cat => cat.Id, (oc, cat) => new
                {
                    oc.o.Id,
                    ClientName = oc.Name,
                    CatalogName = cat.Name,
                    oc.o.TotalPrice
                })
                .ToList();

            writer.WriteLine(orders.Count);
            foreach (var order in orders)
            {
                writer.WriteLine($"{order.Id}|{order.ClientName}|{order.CatalogName}|{order.TotalPrice:F2}");
            }
        }
    }

    public class GetUserIdCommand : ICommand
    {
        public void Execute(StreamWriter writer, StreamReader reader)
        {
            string email = reader.ReadLine();
            using var db = new AppDbContext();
            var client = db.Clients.FirstOrDefault(c => c.Email == email);
            writer.WriteLine(client?.Id.ToString() ?? "Пользователь не найден");
        }
    }

    public static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Сервер запущен.");

        // Словарь команд
        var commands = new System.Collections.Generic.Dictionary<string, ICommand>
        {
            { "REGISTER", new RegisterCommand() },
            { "LOGIN", new LoginCommand() },
            { "LOGOUT", new LogoutCommand() },
            { "GET_CATALOG", new GetCatalogCommand() },
            { "CREATE_ORDER", new CreateOrderCommand() },
            { "GET_ORDERS", new GetOrdersCommand() },
            { "GET_USER_ID", new GetUserIdCommand() }
        };

        // Поток для SQL-запросов
        var sqlThread = new Thread(StartSqlConsole);
        sqlThread.Start();

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Подключение установлено.");
            var clientThread = new Thread(() => HandleClient(client, commands));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client, System.Collections.Generic.Dictionary<string, ICommand> commands)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        while (true)
        {
            var commandName = reader.ReadLine();
            if (commandName == null) break;

            Console.WriteLine($"Получена команда: {commandName}");

            if (commands.TryGetValue(commandName, out var command))
            {
                command.Execute(writer, reader);
            }
            else
            {
                writer.WriteLine("Неверная команда.");
            }
        }
        client.Close();
    }

    public static void StartSqlConsole()
    {
        while (true)
        {
            Console.WriteLine("Введите SQL-запрос или 'exit' для выхода:");
            var sqlQuery = Console.ReadLine();
            if (sqlQuery.ToLower() == "exit") break;
            try { ExecuteSqlQuery(sqlQuery); }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        }
    }

    public static void ExecuteSqlQuery(string sqlQuery)
    {
        using var db = new AppDbContext();
        var result = db.Database.ExecuteSqlRaw(sqlQuery);
        Console.WriteLine($"Запрос выполнен успешно.");
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\antip\\source\\repos\\AvgElbrusEnjoyer\\AvgElbrusEnjoyer\\SampleDatabase.mdf;Integrated Security=True");
}

// Основая реалтзация

/*using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class Program
{
    public static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Сервер запущен.");

        // Поток для SQL-запросов
        var sqlThread = new Thread(() => StartSqlConsole());
        sqlThread.Start();

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Подключение установлено.");
            var clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        while (true)
        {
            var command = reader.ReadLine();
            if (command == null) break;

            Console.WriteLine($"Получена команда: {command}");

            switch (command)
            {
                case "REGISTER":
                    Register(writer, reader);
                    break;
                case "LOGIN":
                    Login(writer, reader);
                    break;
                case "LOGOUT":
                    writer.WriteLine("Выход из системы выполнен.");
                    writer.Flush();
                    client.Close();
                    Environment.Exit(0); // Закрывает всё, и тесты до завершения //
                    break;
                case "GET_CATALOG":
                    SendCatalog(writer);
                    break;
                case "GET_USER_ID":
                    SendUserId(writer, reader);
                    break;
                case "CREATE_ORDER":
                    CreateOrder(writer, reader);
                    break;
                case "GET_ORDERS":
                    GetOrders(writer);
                    break;
                default:
                    writer.WriteLine("Неверная команда.");
                    break;
            }
        }
        client.Close();
    }

    static void CreateOrder(StreamWriter writer, StreamReader reader)
    {
        int userId = int.Parse(reader.ReadLine());
        int catalogId = int.Parse(reader.ReadLine());
        decimal totalPrice = decimal.Parse(reader.ReadLine());

        using var db = new AppDbContext();
        var order = new Order
        {
            ClientId = userId,
            CatalogId = catalogId,
            TotalPrice = totalPrice
        };

        db.Orders.Add(order);
        db.SaveChanges(); // ???

        writer.WriteLine("Заказ создан.");
        writer.Flush();
    }

    static void GetOrders(StreamWriter writer)
    {
        using var db = new AppDbContext();

        var orders = db.Orders
            .Join(db.Clients, o => o.ClientId, c => c.Id, (o, c) => new { o, c.Name })
            .Join(db.Catalogs, oc => oc.o.CatalogId, cat => cat.Id, (oc, cat) => new
            {
                oc.o.Id,
                ClientName = oc.Name,
                CatalogName = cat.Name,
                oc.o.TotalPrice
            })
            .ToList();

        writer.WriteLine(orders.Count);

        foreach (var order in orders)
        {
            writer.WriteLine($"{order.Id}|{order.ClientName}|{order.CatalogName}|{order.TotalPrice:F2}");
        }

        Console.WriteLine("Данные заказов отправлены.");
    }

    static void Register(StreamWriter writer, StreamReader reader)
    {
        using var db = new AppDbContext();

        string name = reader.ReadLine();
        string email = reader.ReadLine();
        string phone = reader.ReadLine();
        string address = reader.ReadLine();
        string password = reader.ReadLine();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Хеш при регистрации: {hashedPassword}");

        if (db.Clients.Any(c => c.Email == email))
        {
            writer.WriteLine("Пользователь с таким email уже существует.");
            return;
        }

        var newClient = new Client { Name = name, Email = email, Phone = phone, Address = address, Password = hashedPassword };
        db.Clients.Add(newClient);
        db.SaveChanges();

        writer.WriteLine($"Пользователь {name} зарегистрирован.");
    }

    static void Login(StreamWriter writer, StreamReader reader)
    {
        using var db = new AppDbContext();

        string email = reader.ReadLine();
        Console.WriteLine($"Email введен для входа: {email}");
        string password = reader.ReadLine();
        Console.WriteLine($"Пароль введен для входа: {password}");

        var client = db.Clients.FirstOrDefault(c => c.Email == email);
        if (client == null || !BCrypt.Net.BCrypt.Verify(password, client.Password))
        {
            writer.WriteLine("Неверный email или пароль.");
            return;
        }

        string userName = client.Name;
        writer.WriteLine($"Добро пожаловать, {client.Name}");
    }

    static void SendCatalog(StreamWriter writer)
    {
        using var db = new AppDbContext();
        var catalogs = db.Catalogs.ToList();

        writer.WriteLine(catalogs.Count);

        foreach (var item in catalogs)
        {
            writer.WriteLine($"{item.Id}|{item.Name}|{item.Category}|{item.Price}");
        }
        Console.WriteLine("Данные каталога отправлены.");
    }

    static void SendUserId(StreamWriter writer, StreamReader reader)
    {
        string email = reader.ReadLine();

        using var db = new AppDbContext();
        var client = db.Clients.FirstOrDefault(c => c.Email == email);

        if (client != null)
        {
            writer.WriteLine(client.Id);
            Console.WriteLine($"Отправлен ID клиента: {client.Id}");
        }
        else
        {
            writer.WriteLine("Пользователь не найден.");
        }
    }

    public static void StartSqlConsole()
    {
        while (true)
        {
            Console.WriteLine("Введите SQL-запрос или 'exit' для выхода:");
            var sqlQuery = Console.ReadLine();

            if (sqlQuery.ToLower() == "exit")
            {
                break;
            }

            try
            {
                ExecuteSqlQuery(sqlQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
            }
        }
    }

    public static void ExecuteSqlQuery(string sqlQuery)
    {
        using var db = new AppDbContext();

        // Выполнение SQL-запроса
        var result = db.Database.ExecuteSqlRaw(sqlQuery);

        Console.WriteLine($"Запрос выполнен успешно.");
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\antip\\source\\repos\\AvgElbrusEnjoyer\\AvgElbrusEnjoyer\\SampleDatabase.mdf;Integrated Security=True");
}*/
