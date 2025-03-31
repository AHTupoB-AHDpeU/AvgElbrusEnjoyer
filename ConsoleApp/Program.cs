using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Threading;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключения...");

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
                    Environment.Exit(0);
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
        db.SaveChanges();

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

        Console.WriteLine("Данные заказов отправлены клиенту.");
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

        if (db.Clients.Any(c => c.Email == email))
        {
            writer.WriteLine("Пользователь с таким email уже существует.");
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
        if (client == null)
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
        Console.WriteLine("Данные каталога отправлены клиенту.");
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

    static void StartSqlConsole()
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

    static void ExecuteSqlQuery(string sqlQuery)
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
}
