
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace RangrasiyaBusiness.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<WorkMeasurement> WorkMeasurements { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<QuotationCondition> QuotationConditions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<WorkExpense> WorkExpenses { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<WorkList> WorkLists { get; set; }

        public DbSet<WorkListMeasurement> WorkListMeasurements { get; set; }


        protected override void OnModelCreating(
    ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Item decimal precision
            modelBuilder.Entity<Item>()
                .Property(i => i.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Item>()
                .Property(i => i.GSTPercentage)
                .HasPrecision(5, 2);

            // Quotation decimal precision
            modelBuilder.Entity<Quotation>()
                .Property(q => q.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.GSTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.GrandTotal)
                .HasPrecision(18, 2);
            // QuotationItem decimal precision

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.GSTPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.GSTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationItem>()
                .Property(q => q.TotalAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                 .HasOne(i => i.Customer)
                 .WithMany()
                 .HasForeignKey(i => i.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Item)
                .WithMany()
                .HasForeignKey(ii => ii.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.GSTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.GrandTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.GSTPercentage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.GSTAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Work>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Work>()
                .Property(w => w.EstimatedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Work>()
                .Property(w => w.ActualAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<WorkExpense>()
                .HasOne(e => e.Work)
                .WithMany(w => w.WorkExpenses)
                .HasForeignKey(e => e.WorkId)
                .OnDelete(DeleteBehavior.Cascade);          
            modelBuilder.Entity<WorkExpense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);
        }
    }
}