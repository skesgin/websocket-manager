using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace WebSocketsSample.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        //// Uncomment for dotnet ef commands.
        //public ConfigurationDbContext()
        //{

        //}

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //// Uncomment for dotnet ef commands.
            //optionsBuilder.UseSqlServer("Data Source=pitcdbserver.database.windows.net;Initial Catalog=PITC_DB;Integrated Security=False;User ID=PITC;Password=PI94tro22$;Connect Timeout=15;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tenant
            builder.Entity<Product>()
                .HasIndex(i => i.ProductNumber);
        }

    }
}
