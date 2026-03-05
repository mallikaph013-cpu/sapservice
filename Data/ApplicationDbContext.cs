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
        public DbSet<LicensePermissionItem> LicensePermissionItems { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Plant> Plants { get; set; } = null!;
        public DbSet<MasterDataCombination> MasterDataCombinations { get; set; } = null!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        public DbSet<DocumentRouting> DocumentRoutings { get; set; } = null!;
        public DbSet<NewsArticle> NewsArticles { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

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

            builder.Entity<RequestItem>()
                .HasMany(r => r.LicensePermissions)
                .WithOne(lp => lp.RequestItem)
                .HasForeignKey(lp => lp.RequestItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentRouting>()
                .HasOne(dr => dr.DocumentType)
                .WithMany(dt => dt.DocumentRoutings)
                .HasForeignKey(dr => dr.DocumentTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DocumentRouting>()
                .HasOne(dr => dr.Department)
                .WithMany()
                .HasForeignKey(dr => dr.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DocumentRouting>()
                .HasOne(dr => dr.Section)
                .WithMany()
                .HasForeignKey(dr => dr.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DocumentRouting>()
                .HasOne(dr => dr.Plant)
                .WithMany()
                .HasForeignKey(dr => dr.PlantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AuditLog>()
                .HasIndex(a => a.PerformedAt);
        }
    }
}
