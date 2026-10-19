using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaGiamGiaController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public MaGiamGiaController(BookParadiseDbContext context) => _context = context;

        // 1. DÀNH CHO ADMIN: Lấy danh sách, Thêm, Xóa
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.MaGiamGias.OrderByDescending(x => x.NgayHetHan).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(MaGiamGia voucher)
        {
            _context.MaGiamGias.Add(voucher);
            await _context.SaveChangesAsync();
            return Ok(voucher);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.MaGiamGias.FindAsync(id);
            if (voucher == null) return NotFound();
            _context.MaGiamGias.Remove(voucher);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }

        // 2. DÀNH CHO KHÁCH HÀNG: Áp dụng mã lúc thanh toán
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyVoucher([FromBody] VoucherApplyRequest req)
        {
            var voucher = await _context.MaGiamGias.FirstOrDefaultAsync(v => v.Code.ToUpper() == req.Code.ToUpper() && v.HienThi);

            if (voucher == null) return BadRequest(new { message = "Mã giảm giá không tồn tại!" });
            if (voucher.NgayHetHan < DateTime.Now) return BadRequest(new { message = "Mã giảm giá đã hết hạn!" });
            if (voucher.SoLuong <= 0) return BadRequest(new { message = "Mã giảm giá đã hết lượt sử dụng!" });
            if (req.TongTienDonHang < voucher.DonHangToiThieu)
                return BadRequest(new { message = $"Đơn hàng tối thiểu để áp dụng mã này là {voucher.DonHangToiThieu:N0}đ!" });

            decimal tienGiam = 0;
            if (voucher.PhanTramGiam > 0)
                tienGiam = (req.TongTienDonHang * voucher.PhanTramGiam) / 100;
            else if (voucher.TienGiamTrucTiep > 0)
                tienGiam = voucher.TienGiamTrucTiep;

            // Đảm bảo tiền giảm không vượt quá tổng tiền
            tienGiam = tienGiam > req.TongTienDonHang ? req.TongTienDonHang : tienGiam;

            return Ok(new { code = voucher.Code, soTienGiam = tienGiam });
        }
    }

    public class VoucherApplyRequest
    {
        public string Code { get; set; }
        public decimal TongTienDonHang { get; set; }
    }
}