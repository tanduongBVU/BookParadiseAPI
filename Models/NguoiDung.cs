using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("nguoi_dung", Schema = "quan_ly_ban_sach")]
    public class NguoiDung
    {
        [Key]
        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Column("ho_ten")]
        public string HoTen { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("mat_khau")]
        public string MatKhau { get; set; } = string.Empty;

        // Thêm đoạn này vào dưới cột HienThi
        [Column("vai_tro")]
        public string VaiTro { get; set; } = "Khach_Hang";

        [Column("trang_thai_ai")]
        public string TrangThaiAI { get; set; } = "Cho_Kiem_Duyet";

        [Column("hien_thi")]
        public bool HienThi { get; set; } = true;
    }
}