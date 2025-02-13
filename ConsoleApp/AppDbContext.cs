using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp
{
    public class AppDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Finance> Finances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\antip\\source\\repos\\AvgElbrusEnjoyer\\AvgElbrusEnjoyer\\SampleDatabase.mdf;Integrated Security=True");
        }
    }
}
