using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключения...");

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
                    break;
                default:
                    writer.WriteLine("Неверная команда.");
                    break;
            }
        }
        client.Close();
    }

    static void Register(StreamWriter writer, StreamReader reader)
    {
        using var db = new AppDbContext();

        string name = reader.ReadLine();
        string email = reader.ReadLine();
        string password = reader.ReadLine();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        if (db.Clients.Any(c => c.Email == email))
        {
            writer.WriteLine("Пользователь с таким email уже существует.");
            return;
        }

        var newClient = new Client { Name = name, Email = email, Password = hashedPassword };
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

        writer.WriteLine($"Добро пожаловать, {client.Name}!");
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\antip\\source\\repos\\AvgElbrusEnjoyer\\AvgElbrusEnjoyer\\SampleDatabase.mdf;Integrated Security=True");
}
