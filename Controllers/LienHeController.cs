using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BookParadise.Controllers
{
    [ApiController]
    public class LienHeController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        private readonly IConfiguration _config;

        public LienHeController(BookParadiseDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 1. API CHO KHÁCH HÀNG: Gửi form liên hệ (Giao diện Cửa hàng gọi)
        [HttpPost("api/lienhe")]
        public async Task<IActionResult> SubmitContact([FromBody] LienHe request)
        {
            request.NgayGui = DateTime.Now;
            request.DaTraLoi = false;
            _context.LienHes.Add(request);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Gửi liên hệ thành công!" });
        }

        // 2. API CHO ADMIN: Lấy danh sách liên hệ
        [HttpGet("api/admin/lienhe")]
        public async Task<IActionResult> GetAllContacts()
        {
            var list = await _context.LienHes.OrderByDescending(x => x.NgayGui).ToListAsync();
            return Ok(list);
        }

        // 3. API CHO ADMIN: Trả lời qua Email và cập nhật trạng thái
        [HttpPost("api/admin/lienhe/{id}/reply")]
        public async Task<IActionResult> ReplyContact(int id, [FromBody] ReplyDto dto)
        {
            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe == null) return NotFound("Không tìm thấy liên hệ");

            try
            {
                // Logic gửi Email qua MailKit
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:SenderEmail"]));
                emailMessage.To.Add(new MailboxAddress(lienHe.HoTen, lienHe.Email));
                emailMessage.Subject = "Phản hồi từ BookParadise";
                emailMessage.Body = new TextPart("plain") { Text = dto.NoiDungTraLoi };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:Password"]);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                // Đánh dấu đã trả lời
                lienHe.DaTraLoi = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gửi email phản hồi thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
            }
        }
    }

    // DTO để nhận dữ liệu phản hồi từ Frontend
    public class ReplyDto
    {
        public string NoiDungTraLoi { get; set; }
    }
}