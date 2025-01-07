using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class NhomThuocController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public NhomThuocController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [Route("Dashboard/NhomThuoc")]
        [HttpGet]
        public IActionResult Index()
        {
            var nhomThuocs = _context.NhomThuocs
        .Select(nt => new
        {
            nt.MaNhom,
            nt.TenNhom
        })
        .ToList();

            return View(nhomThuocs);
        }

        [Route("Dashboard/NhomThuoc/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNhomThuoc(NhomThuoc model)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.NhomThuocs.Add(model);
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



        [Route("Dashboard/NhomThuoc/Edit/{id}")]
        [HttpGet]
        public IActionResult EditNhomThuoc(int id)
        {
            var nhomthuoc = _context.NhomThuocs
                .Where(nt => nt.MaNhom == id)
                .Select(nt => new
                {
                    MaNhom = nt.MaNhom,
                    TenNhom = nt.TenNhom
                })
                .FirstOrDefault();

            if (nhomthuoc == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp." });
            }

            return Json(nhomthuoc);
        }

        [Route("Dashboard/NhomThuoc/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNhomThuoc(int id, NhomThuoc model)
        {
            if (id != model.MaNhom)
            {
                return BadRequest(new { message = "Mã nhóm thuốc không khớp." });
            }
            try
            {
                var nhomThuoc = _context.NhomThuocs.Find(id);
                if (nhomThuoc != null)
                {
                    nhomThuoc.MaNhom = model.MaNhom;
                    nhomThuoc.TenNhom = model.TenNhom;
                    _context.NhomThuocs.Update(nhomThuoc);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(new { message = "Nhóm thuốc không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [Route("Dashboard/NhomThuoc/Delete/{id}")]
        [HttpPost]
        public IActionResult DeleteNhomThuoc(int id)
        {
            try
            {
                var nt = _context.NhomThuocs.Find(id);
                if (nt == null)
                {
                    return Json(new { success = false, message = "Nhóm thuốc không tồn tại." });
                }

                bool hasRelatedEntries = _context.Thuocs.Any(p => p.MaNhom == id);


                if (hasRelatedEntries)
                {
                    return Json(new { success = false, message = "Không thể xóa nhóm thuốc vì đang có dữ liệu liên kết." });
                }

                _context.NhomThuocs.Remove(nt);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá nhóm thuốc thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xoá nhóm thuốc" });
            }
        }



    }
}