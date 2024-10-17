using System;
using Microsoft.EntityFrameworkCore;

namespace CourierService.Model;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Courier> Couriers { get; set; }
}
