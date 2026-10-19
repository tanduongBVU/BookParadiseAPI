using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm thư viện này

namespace BookParadise.Models
{
    [Table("bien_the", Schema = "quan_ly_ban_sach")]
    public class BienThe
    {
        [Key]
        [Column("ma_bien_the")]
        public int MaBienThe { get; set; }

        [Column("ma_sach")]
        public int MaSach { get; set; }

        [Column("ten_bien_the")]
        public string TenBienThe { get; set; }

        [Column("gia_cong_them")]
        public decimal GiaCongThem { get; set; }

        [Column("so_luong")]
        public int SoLuong { get; set; }

        // --- SỬA Ở ĐÂY: Thêm JsonIgnore và dấu ? ---
        [ForeignKey("MaSach")]
        [JsonIgnore]
        public Sach? Sach { get; set; }
    }
}