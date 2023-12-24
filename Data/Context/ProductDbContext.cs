using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Context
{
    public class ProductDbContext : DbContext
    {
        
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
            //Baraye Ertebat Ba Laye Paeeintar Hatman Bayad In Sazandeh Bashad.
        }
        

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Images> Images { get; set; }

        /*
        //Agar Bekhahim Bedoone Ertebat Ba Laye Paeeintar, Database Ra Haminja Besazim.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.;Initial Catalog=ProductsDb;Integrated Security=True; MultipleActiveResultSets=True; Trust Server Certificate=True");
        }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // تعریف روابط میان Entity ها (برای Product و Category)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)    //Navigation Property "Category" in Product Class
                .WithMany(c => c.Products)  //Navigation Property "Products" in Category Class
                .HasForeignKey(p => p.CategoryId);  //ForeignKey Property in Product Class

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
            modelBuilder.Entity<Images>()
                .HasOne(i => i.Product)   //Navigation Property "Product" in Images Class
                .WithMany(p => p.Images)    //Navigation Property "Images" in Product Class
                .HasForeignKey(i => i.ProductId);   //ForeignKey Property in Images Class
            */



        }
    }
}
