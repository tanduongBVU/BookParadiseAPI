using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("don_hang", Schema = "quan_ly_ban_sach")]
    public class DonHang
    {
        [Key]
        [Column("ma_don_hang")]
        public int MaDonHang { get; set; }

        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Column("ngay_dat")]
        public DateTime NgayDat { get; set; }

        [Column("tong_tien")]
        public decimal TongTien { get; set; }

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; } = "Cho_Kiem_Duyet";

        [Column("hien_thi")]
        public bool HienThi { get; set; } = true;

        [Column("ma_code")]
        public string? MaCode { get; set; }

        [Column("so_tien_giam")]
        public decimal SoTienGiam { get; set; }

        // Thêm cột này để quản lý trạng thái giao hàng độc lập với AI
        [Column("trang_thai_don_hang")]
        public string TrangThaiDonHang { get; set; } = "Cho_Xac_Nhan";
    }
}