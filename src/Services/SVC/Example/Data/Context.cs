using Microsoft.EntityFrameworkCore;
using SVC.Example.Model;

namespace SVC.Example.Data
{
    public class Context : DbContext
    {

        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderedProduct> OrderedProducts { get; set; }

        protected Context(DbContextOptions options) : base(options) { }
        public Context(DbContextOptions<Context> options) : this((DbContextOptions)options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

            builder.Entity<Location>(e => {
                e.HasKey(x => x.LocationId);
            });

            builder.Entity<Order>(e => {
                e.HasKey(o => o.OrderId);

                e.HasOne<Location>(o => o.Location)
                    .WithMany(l => l.Orders)
                    .HasForeignKey(o => o.LocationId)
                    .IsRequired(true);

                e.HasMany<OrderedProduct>(o => o.OrderedProducts)
                    .WithOne(op => op.Order)
                    .HasForeignKey(op => op.OrderId);
            });

            builder.Entity<Product>(e => {
                e.HasKey(x => x.ProductId);

                e.HasMany<OrderedProduct>(p => p.OrderedProducts)
                    .WithOne(op => op.Product)
                    .HasForeignKey(op => op.ProductId);

            });

            builder.Entity<OrderedProduct>(e => {
                e.HasKey(x => x.OrderedProductId);
            });
        }
        
    }
}