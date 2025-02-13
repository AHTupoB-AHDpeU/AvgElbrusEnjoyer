using System;
using System.Linq;
using ConsoleApp;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();

        Console.WriteLine("Hello, World!");
        // Вывод всех клиентов
        var clients = db.Clients.ToList();
        foreach (var client in clients)
        {
            Console.WriteLine($"ID: {client.Id}, Name: {client.Name}, Email: {client.Email}");
        }
    }
}


