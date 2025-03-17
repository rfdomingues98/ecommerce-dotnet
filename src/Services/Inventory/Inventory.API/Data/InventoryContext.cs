using Inventory.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Data;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>().HasKey(i => i.Id);
        modelBuilder.Entity<InventoryItem>().HasIndex(i => i.ProductId).IsUnique();
        modelBuilder.Entity<InventoryItem>().HasIndex(i => i.Sku).IsUnique();
        modelBuilder.Entity<InventoryMovement>().HasKey(m => m.Id);
        modelBuilder.Entity<InventoryMovement>().HasOne(m => m.InventoryItem).WithMany(i => i.Movements).HasForeignKey(m => m.InventoryItemId);
    }
}
