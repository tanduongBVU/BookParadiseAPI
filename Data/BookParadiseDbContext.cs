using BookParadise.Models;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Data
{
    public class BookParadiseDbContext : DbContext
    {
        public BookParadiseDbContext(
            DbContextOptions<BookParadiseDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sach> Sachs { get; set; }
    }
}