using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm thư viện này

namespace BookParadise.Models
{
    [Table("loai_sach", Schema = "quan_ly_ban_sach")]
    public class LoaiSach
    {
        [Key]
        [Column("ma_loai")]
        public int MaLoai { get; set; }

        [Column("ten_loai")]
        public string TenLoai { get; set; }

        // Thêm JsonIgnore để Backend không bắt lỗi thiếu dữ liệu và tránh vòng lặp JSON
        [JsonIgnore]
        public ICollection<Sach>? CacSach { get; set; }
    }
}