using System.Data.Common;
using DotnetSupportTicketApi.Data;
using DotnetSupportTicketApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSupportTicketApi.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly DbConnection _connection;

        public CustomWebApplicationFactory()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            _connection = connection;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbConnection));

                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                services.AddSingleton(_connection);

                services.AddDbContext<AppDbContext>((serviceProvider, options) =>
                {
                    var connection = serviceProvider.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });

                // Seed the SAME in-memory database the app will use.
                var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite((SqliteConnection)_connection)
                    .Options;

                using var db = new AppDbContext(dbOptions);

                db.Database.EnsureCreated();

                if (!db.AppUsers.Any())
                {
                    var alice = new AppUser
                    {
                        Name = "Alice Admin",
                        Email = "alice@example.com"
                    };

                    var bob = new AppUser
                    {
                        Name = "Bob Support",
                        Email = "bob@example.com"
                    };

                    db.AppUsers.AddRange(alice, bob);
                    db.SaveChanges();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}