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

        public DbSet<NguoiDung> NguoiDungs { get; set; }

        public DbSet<DonHang> DonHangs { get; set; }

        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        public DbSet<BinhLuan> BinhLuans { get; set; }

        public DbSet<SachYeuThich> SachYeuThiches { get; set; }

        public DbSet<LoaiSach> LoaiSachs { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<BienThe> BienThes { get; set; }

        public DbSet<LienHe> LienHes { get; set; }

        public DbSet<MaGiamGia> MaGiamGias { get; set; }
    }
}