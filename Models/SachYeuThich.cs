using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("sach_yeu_thich", Schema = "quan_ly_ban_sach")]
    public class SachYeuThich
    {
        [Key]
        [Column("ma_yeu_thich")]
        public int MaYeuThich { get; set; }

        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Column("ma_sach")]
        public int MaSach { get; set; }

        [Column("ngay_them")]
        public DateTime NgayThem { get; set; } = DateTime.UtcNow;
    }
}