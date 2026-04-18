using Microsoft.EntityFrameworkCore;
using NFe.Domain.Entities;

namespace NFe.Infrastructure.Data
{
    public class NfeDbContext : DbContext
    {
        public NfeDbContext(DbContextOptions<NfeDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<NfeDocument> NfeDocuments { get; set; }
        public DbSet<NfeEvent> NfeEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("nfe");

            // Force all table and column names to lowercase for PostgreSQL compatibility
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToLower());
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }

            modelBuilder.Entity<Company>()
                .ToTable("companies")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.Cnpj).IsUnique();

            modelBuilder.Entity<NfeDocument>()
                .ToTable("nfe_documents")
                .HasKey(d => d.Id);
            modelBuilder.Entity<NfeDocument>()
                .HasOne(d => d.Company)
                .WithMany(c => c.NfeDocuments)
                .HasForeignKey(d => d.CompanyId);
            modelBuilder.Entity<NfeDocument>()
                .HasIndex(d => new { d.CompanyId, d.NfeNumber, d.Series })
                .IsUnique();

            modelBuilder.Entity<NfeEvent>()
                .ToTable("nfe_events")
                .HasKey(e => e.Id);
            modelBuilder.Entity<NfeEvent>()
                .HasOne(e => e.NfeDocument)
                .WithMany(d => d.Events)
                .HasForeignKey(e => e.NfeId);

            modelBuilder.Entity<User>()
                .ToTable("users")
                .HasKey(u => u.id);
            modelBuilder.Entity<User>()
                .Ignore(u => u.create_at)
                .Ignore(u => u.update_at);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.username).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.email).IsUnique();
        }
    }
}