using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/sale")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public SaleController(BookParadiseDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetSaleBooks()
        {
            // Chỉ yêu cầu điều kiện HienThiTrangSale == true
            var saleBooks = await _context.Sachs
                .Include(s => s.KhuyenMai)
                .Where(s => s.HienThiTrangSale == true)
                .ToListAsync();

            return Ok(saleBooks);
        }
    }
}