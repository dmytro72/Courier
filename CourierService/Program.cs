using CourierService.Model;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Couriers.Any())
    {
        context.Couriers.AddRange(
            new Courier {Id = 1, Name = "Alex", District = 1, },
            new Courier {Id = 2, Name = "Bob", District = 1, }
        );
        context.SaveChanges();
    }
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/couriers", async (AppDbContext db) => await db.Couriers.ToListAsync());

app.MapGet("/couriers/{id}", async (int id, AppDbContext db) =>
{
    var courier = await db.Couriers.FindAsync(id);
    return courier != null ? Results.Ok(courier) : Results.NotFound();
});

app.Run();

public partial class Program {}
