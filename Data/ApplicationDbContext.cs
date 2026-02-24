using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myapp.Models;

namespace myapp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RequestItem> RequestItems { get; set; } = null!;
        public DbSet<BomComponent> BomComponents { get; set; } = null!;
        public DbSet<Routing> Routings { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Plant> Plants { get; set; } = null!;
        public DbSet<MasterDataCombination> MasterDataCombinations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<RequestItem>()
                .HasMany(r => r.BomComponents)
                .WithOne(b => b.RequestItem)
                .HasForeignKey(b => b.RequestItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequestItem>()
                .HasMany(r => r.Routings)
                .WithOne(r => r.RequestItem)
                .HasForeignKey(r => r.RequestItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
