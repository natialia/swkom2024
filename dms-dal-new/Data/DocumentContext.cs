using Microsoft.EntityFrameworkCore;
using dms_dal_new.Entities;

namespace dms_dal_new.Data
{
    public sealed class DocumentContext : DbContext
    {
        public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) { }

        public DbSet<DocumentItem>? DocumentItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manuelle Konfiguration der Tabelle
            modelBuilder.Entity<DocumentItem>(entity =>
            {
                entity.ToTable("DocumentItems");  // Setzt den Tabellennamen

                entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);  // Konfiguriert den "Name"-Spalten
            });
            modelBuilder.Entity<DocumentItem>().HasData(new DocumentItem(1) { Name = "first" });
            base.OnModelCreating(modelBuilder);
        }
    }
}
