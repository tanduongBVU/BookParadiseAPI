using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Thêm thư viện này nếu cần dùng JsonIgnore
using System.Text.Json.Serialization;

namespace BookParadise.Models
{
    [Table("sach", Schema = "quan_ly_ban_sach")]
    public class Sach
    {
        [Key]
        [Column("ma_sach")]
        public int MaSach { get; set; }

        [Column("ten_sach")]
        public string TenSach { get; set; }

        [Column("mo_ta")]
        public string? MoTa { get; set; } // Thêm ? vì form có thể gửi chuỗi rỗng

        [Column("gia")]
        public decimal Gia { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("trang_thai_ai")]
        public string? TrangThaiAI { get; set; } // Thêm ? 

        [Column("hien_thi")]
        public bool HienThi { get; set; }

        [Column("ma_loai")]
        public int? MaLoai { get; set; }

        [Column("ma_km")]
        public int? MaKM { get; set; }

        [Column("hien_thi_trang_sale")]
        public bool HienThiTrangSale { get; set; }

        // --- THÊM DẤU ? ĐỂ BỎ QUA KIỂM DUYỆT (VALIDATION) KHI THÊM MỚI ---
        [ForeignKey("MaLoai")]
        public LoaiSach? LoaiSach { get; set; }

        [ForeignKey("MaKM")]
        public KhuyenMai? KhuyenMai { get; set; }

        [Column("hinh_anh")]
        public string? HinhAnh { get; set; }

        public ICollection<BienThe>? CacBienThe { get; set; }
    }
}