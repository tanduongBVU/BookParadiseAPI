using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string MoTa { get; set; }

        [Column("gia")]
        public decimal Gia { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; }

        [Column("hien_thi")]
        public bool HienThi { get; set; }
    }
}