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
            // Manual Configuration of the Table
            modelBuilder.Entity<DocumentItem>(entity =>
            {
                entity.ToTable("DocumentItems");  // Table name

                entity.HasKey(e => e.Id);  // Primary Key

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); // Ensure you get automatically generated ID back

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);  // Configures the "Name"-Columns
            });
            modelBuilder.Entity<DocumentItem>().HasData(new DocumentItem(1) { Name = "first", FileType = "application/pdf", FileSize="400kB" });
            base.OnModelCreating(modelBuilder);
        }
    }
}
