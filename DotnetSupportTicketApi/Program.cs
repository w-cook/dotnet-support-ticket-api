using DotnetSupportTicketApi.Data;
using DotnetSupportTicketApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSeeding((context, _) =>
    {
        if (!context.Set<AppUser>().Any())
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

            context.Set<AppUser>().AddRange(alice, bob);
            context.SaveChanges();
        }

        if (!context.Set<Ticket>().Any())
        {
            var alice = context.Set<AppUser>().First(u => u.Email == "alice@example.com");
            var bob = context.Set<AppUser>().First(u => u.Email == "bob@example.com");

            context.Set<Ticket>().Add(new Ticket
            {
                Title = "Sample support issue",
                Description = "Example seeded ticket for development.",
                Priority = "Medium",
                Status = "Open",
                CreatedByUserId = alice.Id,
                AssignedToUserId = bob.Id
            });

            context.SaveChanges();
        }
    })
    .UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        if (!await context.Set<AppUser>().AnyAsync(cancellationToken))
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

            context.Set<AppUser>().AddRange(alice, bob);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Ticket>().AnyAsync(cancellationToken))
        {
            var alice = context.Set<AppUser>().First(u => u.Email == "alice@example.com");
            var bob = context.Set<AppUser>().First(u => u.Email == "bob@example.com");

            context.Set<Ticket>().Add(new Ticket
            {
                Title = "Sample support issue",
                Description = "Example seeded ticket for development.",
                Priority = "Medium",
                Status = "Open",
                CreatedByUserId = alice.Id,
                AssignedToUserId = bob.Id
            });

            await context.SaveChangesAsync(cancellationToken);
        }
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Support Ticket API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}