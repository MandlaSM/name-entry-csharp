using Microsoft.EntityFrameworkCore;
using NameEntryApp.Models;

namespace NameEntryApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People => Set<Person>();
    }
}
