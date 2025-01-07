using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    [Authorize(Roles = "Admin")]
    public class NhaCungCapController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public NhaCungCapController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [Route("Dashboard/NhaCungCap")]
        [HttpGet]
        public IActionResult Index()
        {
            var nhaCungCaps = _context.NhaCungCaps
                .Select(ncc => new NhaCungCap
                {
                    MaNhaCungCap = ncc.MaNhaCungCap,
                    TenNhaCungCap = ncc.TenNhaCungCap,
                    DiaChi = ncc.DiaChi,
                    SoDienThoai = ncc.SoDienThoai
                }).ToList();

            return View(nhaCungCaps);
        }

        [Route("Dashboard/NhaCungCap/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateThanhVien(NhaCungCap model)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.NhaCungCaps.Add(model);
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



        [Route("Dashboard/NhaCungCap/Edit/{id}")]
        [HttpGet]
        public IActionResult EditNhaCungCap(int id)
        {
            var nhacungcap = _context.NhaCungCaps
                .Where(ncc => ncc.MaNhaCungCap == id)
                .Select(ncc => new
                {
                    MaNhaCungCap = ncc.MaNhaCungCap,
                    TenNhaCungCap = ncc.TenNhaCungCap,
                    DiaChi = ncc.DiaChi,
                    SoDienThoai = ncc.SoDienThoai
                })
                .FirstOrDefault();

            if (nhacungcap == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp." });
            }

            return Json(nhacungcap);
        }

        [Route("Dashboard/NhaCungCap/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNhaCungCap(int id, NhaCungCap model)
        {
            if (id != model.MaNhaCungCap)
            {
                return BadRequest(new { message = "Mã nhà cung cấp không đúng." });
            }

            try
            {
                var nhacungcap = _context.NhaCungCaps.Find(id);
                if (nhacungcap != null)
                {
                    nhacungcap.TenNhaCungCap = model.TenNhaCungCap;
                    nhacungcap.DiaChi = model.DiaChi;
                    nhacungcap.SoDienThoai = model.SoDienThoai;

                    _context.NhaCungCaps.Update(nhacungcap);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(new { message = "Nhà cung cấp không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }
        [Route("Dashboard/NhaCungCap/Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNhaCungCap(int id)
        {
            try
            {
                var ncc = _context.NhaCungCaps.Find(id);
                if (ncc == null)
                {
                    return Json(new { success = false, message = "Nhà cung cấp không tồn tại." });
                }

                // Kiểm tra xem nhà cung cấp có liên kết khóa ngoại hay không
                bool hasRelatedEntries = _context.PhieuNhaps.Any(p => p.MaNhaCungCap == id);


                if (hasRelatedEntries)
                {
                    return Json(new { success = false, message = "Không thể xóa nhà cung cấp vì đang có dữ liệu liên kết." });
                }

                // Thực hiện xóa nếu không có liên kết
                _context.NhaCungCaps.Remove(ncc);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xoá nhà cung cấp." });
            }
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _context.NhaCungCaps
                .Where(tk => tk.TenNhaCungCap.ToString().Contains(query) || tk.SoDienThoai.Contains(query))
                .Select(tk => new
                {
                    MaNhaCungCap = tk.MaNhaCungCap,
                    TenNhaCungCap = tk.TenNhaCungCap,
                    DiaChi = tk.DiaChi,
                    SoDienThoai = tk.SoDienThoai
                })
                .ToList();

            return PartialView("_NhaCungCapTableRows", results);
        }





    }
}