using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("binh_luan", Schema = "quan_ly_ban_sach")]
    public class BinhLuan
    {
        [Key]
        [Column("ma_binh_luan")]
        public int MaBinhLuan { get; set; }

        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Column("ma_sach")]
        public int MaSach { get; set; }

        [Column("noi_dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Column("ngay_binh_luan")]
        public DateTime NgayBinhLuan { get; set; }

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; } = "Cho_Kiem_Duyet";

        [Column("hien_thi")]
        public bool HienThi { get; set; } = true;

        // Thêm đoạn này để nối với bảng NguoiDung
        [ForeignKey("MaNguoiDung")]
        public NguoiDung? NguoiDung { get; set; }
    }
}