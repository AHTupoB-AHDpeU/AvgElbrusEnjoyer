using System;
using System.Linq;
using ConsoleApp;
using BCrypt.Net;

class Program
{
    static bool isAuthenticated = false;
    static string userName = "";

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(isAuthenticated ? $"Добро пожаловать на сайт, {userName}!" : "Пожалуйста, авторизуйтесь на сайте!\n");
            using (var context = new AppDbContext())
            {
                Console.WriteLine("Доступные комплектующие:\n");

                var catalogs = context.Catalogs.ToList();
                foreach (var catalog in catalogs)
                {
                    Console.WriteLine($"{catalog.Id}. {catalog.Name} - {catalog.Category}, Цена: {catalog.Price} руб.");
                } nameof b  

                Console.WriteLine("\nКоманды:");
                Console.WriteLine("1) Войти");
                Console.WriteLine("2) Зарегистрироваться");
                Console.WriteLine("3) Начать выбор комплектующих");
                Console.WriteLine("4) Перейти в корзину");
                Console.WriteLine("5) Выйти из приложения\n");

                Console.WriteLine("Введите номер команды: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        Register();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод. Попробуйте снова.");
                        break;
                }
            }
        }
    }

    static void Register()
    {
        using (var db = new AppDbContext())
        {
            Console.Clear();
            Console.WriteLine("=== Регистрация ===");

            Console.Write("Имя: ");
            string name = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Телефон (необязательно): ");
            string phone = Console.ReadLine();

            Console.Write("Адрес (необязательно): ");
            string address = Console.ReadLine();

            Console.Write("Пароль: ");
            string password = Console.ReadLine();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new Client
            {
                Name = name,
                Email = email,
                Phone = string.IsNullOrEmpty(phone) ? null : phone,
                Address = string.IsNullOrEmpty(address) ? null : address,
                Password = hashedPassword
            };

            db.Clients.Add(newUser);
            db.SaveChanges();

            Console.WriteLine("Регистрация успешна! Нажмите Enter для продолжения.");
            Console.ReadLine();
        }
    }

    static void Login()
    {
        using (var db = new AppDbContext())
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Авторизация ===");
                Console.Write("Email: ");
                string email = Console.ReadLine();
                Console.Write("Пароль: ");
                string password = Console.ReadLine();

                var user = db.Clients.FirstOrDefault(u => u.Email == email);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    isAuthenticated = true;
                    userName = user.Name;
                    Console.Clear();
                    Console.WriteLine($"Добро пожаловать на сайт, {userName}!");
                    Console.WriteLine("Нажмите Enter для выхода в меню...");
                    Console.ReadLine();
                    break;
                }
                else
                {
                    Console.WriteLine("Неверный email или пароль. Попробуйте снова.");
                    Console.ReadLine();
                }
            }
        }
    }

}


