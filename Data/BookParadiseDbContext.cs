using System;
using System.ComponentModel.DataAnnotations.Schema; // Cần dòng này để dùng [Column]
using BookParadise.Models;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Data
{
    // Ánh xạ chính xác tên cột chữ thường trong PostgreSQL
    public class MvDoanhThuNgay
    {
        [Column("ngay")]
        public DateTime Ngay { get; set; }

        [Column("tong_doanh_thu")]
        public decimal TongDoanhThu { get; set; }
    }

    public class ThongKeLog
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("lan_cap_nhat_cuoi")]
        public DateTime LanCapNhatCuoi { get; set; }
    }

    public class BookParadiseDbContext : DbContext
    {
        public BookParadiseDbContext(DbContextOptions<BookParadiseDbContext> options)
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
        public DbSet<SecurityEvent> SecurityEvents { get; set; }

        // Bổ sung DbSet cho thống kê
        public DbSet<MvDoanhThuNgay> MvDoanhThuNgays { get; set; }
        public DbSet<ThongKeLog> ThongKeLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình không tạo khóa chính cho Materialized View
            modelBuilder.Entity<MvDoanhThuNgay>()
                .HasNoKey()
                .ToView("mv_doanh_thu_ngay", "quan_ly_ban_sach");

            // Cấu hình bảng Log
            modelBuilder.Entity<ThongKeLog>()
                .ToTable("thong_ke_log", "quan_ly_ban_sach");
        }
    }
}