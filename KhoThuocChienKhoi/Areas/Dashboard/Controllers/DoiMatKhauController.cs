using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhoThuocChienKhoi.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class DoiMatKhauController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public DoiMatKhauController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            // Lấy mã tài khoản hiện tại từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.TaiKhoanNhanViens.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            // Kiểm tra mật khẩu hiện tại
            if (!VerifyPasswordHash(model.CurrentPassword, user.MatKhau))
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới không khớp." });
            }

            // Cập nhật mật khẩu mới
            user.MatKhau = CreatePasswordHash(model.NewPassword);
            _context.TaiKhoanNhanViens.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        private string CreatePasswordHash(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("KhoThuocChienKhoiSecretSalt");
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            string hashed = CreatePasswordHash(password);
            return hashed == storedHash;
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
