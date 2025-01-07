using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    [Authorize(Roles = "Admin")]

    public class ThanhVienController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public ThanhVienController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [Route("Dashboard/ThanhVien")]
        [HttpGet]
        public IActionResult Index()
        {
            var taiKhoanNhanViens = _context.TaiKhoanNhanViens
                .Select(tk => new TaiKhoanNhanVien
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    MaNhanVien = tk.MaNhanVien,
                    HoTen = tk.HoTen,
                    VaiTro = tk.VaiTro
                }).ToList();

            return View(taiKhoanNhanViens);
        }
        [Route("Dashboard/ThanhVien/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateThanhVien(TaiKhoanNhanVien model)
        {
            try
            {
                if (_context.TaiKhoanNhanViens.Any(tv => tv.MaNhanVien == model.MaNhanVien))
                {
                    TempData["ErrorMessage"] = "Mã nhân viên này đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }
                if (ModelState.IsValid)
                {
                    _context.TaiKhoanNhanViens.Add(model);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index");
        }


        [Route("Dashboard/ThanhVien/Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteThanhVien(int id)
        {
            try
            {
                var tv = _context.TaiKhoanNhanViens.Find(id);
                if (tv == null)
                {
                    return Json(new { success = false, message = "Nhân viên không tồn tại." });
                }

                bool hasRelatedEntries = _context.PhieuXuats.Any(p => p.MaTaiKhoan == id)
                                                || _context.PhieuNhaps.Any(p => p.MaTaiKhoan == id);


                if (hasRelatedEntries)
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản này vì đang có dữ liệu liên kết." });
                }

                _context.TaiKhoanNhanViens.Remove(tv);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá nhân viênthành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xoá nhân viên" });
            }
        }

        [Route("Dashboard/ThanhVien/Edit/{id}")]
        [HttpGet]
        public IActionResult EditThanhVien(int id)
        {
            var taiKhoan = _context.TaiKhoanNhanViens
                .Where(tk => tk.MaTaiKhoan == id)
                .Select(tk => new
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    MaNhanVien = tk.MaNhanVien,
                    HoTen = tk.HoTen,
                    VaiTro = tk.VaiTro
                })
                .FirstOrDefault();

            if (taiKhoan == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản nhân viên." });
            }

            return Json(taiKhoan);
        }

        [Route("Dashboard/ThanhVien/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditThanhVien(int id, TaiKhoanNhanVien model)
        {
            if (id != model.MaTaiKhoan)
            {
                return BadRequest(new { message = "Mã tài khoản không khớp." });
            }

            try
            {
                var taiKhoan = _context.TaiKhoanNhanViens.Find(id);
                if (taiKhoan != null)
                {
                    taiKhoan.MaNhanVien = model.MaNhanVien;
                    taiKhoan.HoTen = model.HoTen;
                    taiKhoan.VaiTro = model.VaiTro;

                    _context.TaiKhoanNhanViens.Update(taiKhoan);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(new { message = "Tài khoản không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _context.TaiKhoanNhanViens
                .Where(tk => tk.MaNhanVien.ToString().Contains(query) || tk.HoTen.Contains(query))
                .Select(tk => new
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    MaNhanVien = tk.MaNhanVien,
                    HoTen = tk.HoTen,
                    VaiTro = tk.VaiTro
                })
                .ToList();

            return PartialView("_ThanhVienTableRows", results);
        }

        [Route("Dashboard/ThanhVien/UpdatePassword/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePassword(int id, [FromBody] string newPassword)
        {
            try
            {
                var taiKhoan = _context.TaiKhoanNhanViens.Find(id);
                if (taiKhoan == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại." });
                }

                // Mã hóa mật khẩu (nếu cần thiết)
                taiKhoan.MatKhau = newPassword;

                _context.TaiKhoanNhanViens.Update(taiKhoan);
                _context.SaveChanges();

                return Json(new { success = true, message = "Cập nhật mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật mật khẩu." });
            }
        }




    }


}
