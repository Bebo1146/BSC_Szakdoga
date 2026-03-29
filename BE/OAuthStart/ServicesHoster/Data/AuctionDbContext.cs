using Microsoft.EntityFrameworkCore;
using ServicesHoster.Data.Entities;
using ServicesHoster.Services;

namespace ServicesHoster.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products => Set<ProductEntity>();
        public DbSet<BidEntity> Bids => Set<BidEntity>();
        public DbSet<ProductBidderEntity> ProductBidders => Set<ProductBidderEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Tell Npgsql to accept DateTime with Kind=Unspecified as UTC
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductEntity>(e =>
            {
                e.ToTable("products");
                e.HasKey(p => p.Id);
                e.Property(p => p.Status).HasConversion<string>();
                e.Property(p => p.TransactionStatus).HasConversion<string>();
            });

            modelBuilder.Entity<BidEntity>(e =>
            {
                e.ToTable("bids");
                e.HasKey(b => b.Id);
                e.HasOne(b => b.Product)
                 .WithMany(p => p.Bids)
                 .HasForeignKey(b => b.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductBidderEntity>(e =>
            {
                e.ToTable("product_bidders");
                e.HasKey(pb => pb.Id);
                e.HasIndex(pb => new { pb.ProductId, pb.BidderId }).IsUnique();
                e.HasOne(pb => pb.Product)
                 .WithMany(p => p.Bidders)
                 .HasForeignKey(pb => pb.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}