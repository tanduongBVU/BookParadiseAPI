using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("chi_tiet_don_hang", Schema = "quan_ly_ban_sach")]
    public class ChiTietDonHang
    {
        [Key]
        [Column("ma_chi_tiet")]
        public int MaChiTiet { get; set; }

        [Column("ma_don_hang")]
        public int MaDonHang { get; set; }

        [Column("ma_sach")]
        public int MaSach { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("don_gia")]
        public decimal DonGia { get; set; }

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; } = "Cho_Kiem_Duyet";

        [Column("hien_thi")]
        public bool HienThi { get; set; } = true;
    }
}