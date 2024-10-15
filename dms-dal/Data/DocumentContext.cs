using Microsoft.EntityFrameworkCore;
using dms_dal.Entities;

namespace dms_dal.Data
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

                entity.Property(e => e.IsComplete);  // Konfiguriert die "IsComplete"-Spalte
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
