using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WebProgramlama.Models;
using Zakuska_AI.Data;

namespace WebProgramlama.Data
{
    public class IdentityContext : IdentityDbContext<AppUser>
    {
        stringSQL strSQL = new stringSQL();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(strSQL.SQLString);
        }
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }
        public DbSet<User> Users {  get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Product>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.ProductOwnerID)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
