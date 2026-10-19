using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm thư viện này

namespace BookParadise.Models
{
    [Table("khuyen_mai", Schema = "quan_ly_ban_sach")]
    public class KhuyenMai
    {
        [Key]
        [Column("ma_km")]
        public int MaKM { get; set; }

        [Column("ten_km")]
        public string TenKM { get; set; }

        [Column("phan_tram_giam")]
        public decimal PhanTramGiam { get; set; }

        [Column("ngay_bat_dau")]
        public DateTime NgayBatDau { get; set; }

        [Column("ngay_ket_thuc")]
        public DateTime NgayKetThuc { get; set; }

        // Thêm JsonIgnore
        [JsonIgnore]
        public ICollection<Sach>? CacSachApDung { get; set; }
    }
}