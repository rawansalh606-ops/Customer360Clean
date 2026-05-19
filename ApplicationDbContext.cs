using Microsoft.EntityFrameworkCore;
using Customer360Clean.Models;

namespace Customer360Clean.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerActivity> Activities { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}
