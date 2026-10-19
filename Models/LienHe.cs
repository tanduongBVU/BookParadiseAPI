using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("lien_he", Schema = "quan_ly_ban_sach")]
    public class LienHe
    {
        [Key]
        [Column("ma_lien_he")]
        public int MaLienHe { get; set; }

        [Column("ho_ten")]
        public string HoTen { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("noi_dung")]
        public string NoiDung { get; set; }

        [Column("ngay_gui")]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Column("da_tra_loi")]
        public bool DaTraLoi { get; set; } = false;
    }
}