using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("ma_giam_gia", Schema = "quan_ly_ban_sach")]
    public class MaGiamGia
    {
        [Key]
        [Column("ma_giam_gia")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("phan_tram_giam")]
        public int PhanTramGiam { get; set; }

        [Column("tien_giam_truc_tiep")]
        public decimal TienGiamTrucTiep { get; set; }

        [Column("don_hang_toi_thieu")]
        public decimal DonHangToiThieu { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("ngay_het_han")]
        public DateTime NgayHetHan { get; set; }

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; } = "Da_Duyet";

        [Column("hien_thi")]
        public bool HienThi { get; set; } = true;
    }
}