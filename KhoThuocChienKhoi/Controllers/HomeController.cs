using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using System.Linq;


public class HomeController : Controller
{
    private readonly KhoThuocChienKhoiContext _context;

    public HomeController(KhoThuocChienKhoiContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Dashboard" });

        }
        return View();

    }

    [HttpPost]
    public async Task<IActionResult> Login(int username, string password)
    {
        var user = _context.TaiKhoanNhanViens
            .FirstOrDefault(u => u.MaNhanVien == username && u.MatKhau == password);

        if (user != null)
        {
            var claims = new List<Claim>
        {
            // new Claim(ClaimTypes.Name, user.HoTen ?? "Unknown"),
            new Claim(ClaimTypes.Role, user.VaiTro),
            new Claim("MaNhanVien", user.MaNhanVien.ToString()),
            new Claim("HoTen", user.HoTen ?? "Unknown"),
            new Claim("VaiTro", user.VaiTro),
            new Claim("MaTaiKhoan", user.MaTaiKhoan.ToString() ?? "Unknown"),
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Dashboard");
        }
        else
        {
            ViewBag.Error = "Mã nhân viên hoặc mật khẩu không hợp lệ";
            return View("Index");
        }
    }


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
    {
        var maNhanVien = User.FindFirst("MaNhanVien")?.Value;
        if (string.IsNullOrEmpty(maNhanVien))
        {
            return Json(new { success = false, message = "Tài khoản không tồn tại." });
        }

        var user = _context.TaiKhoanNhanViens.FirstOrDefault(u => u.MaNhanVien.ToString() == maNhanVien);
        if (user == null)
        {
            return Json(new { success = false, message = "Người dùng không tồn tại." });
        }

        if (user.MatKhau != currentPassword)
        {
            return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
        }

        user.MatKhau = newPassword;
        _context.TaiKhoanNhanViens.Update(user);
        await _context.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Json(new { success = true });
    }


}
