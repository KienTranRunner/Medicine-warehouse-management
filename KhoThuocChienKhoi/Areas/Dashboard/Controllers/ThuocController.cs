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

    public class ThuocController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public ThuocController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }


        [Route("Dashboard/Thuoc")]
        [HttpGet]
        public IActionResult Index(int? maNhom)
        {
            ViewBag.NhomThuocs = _context.NhomThuocs.ToList();

            ViewBag.SelectedNhomName = maNhom.HasValue
                ? _context.NhomThuocs.FirstOrDefault(n => n.MaNhom == maNhom.Value)?.TenNhom
                : "Tất cả";

            var thuocsQuery = _context.Thuocs
                .Join(
                    _context.NhomThuocs,
                    thuoc => thuoc.MaNhom,
                    nhom => nhom.MaNhom,
                    (thuoc, nhom) => new
                    {
                        MaThuoc = thuoc.MaThuoc,
                        TenThuoc = thuoc.TenThuoc,
                        HamLuong = thuoc.HamLuong,
                        HoatChat = thuoc.HoatChat,
                        DonViTinh = thuoc.DonViTinh,
                        Gia = thuoc.Gia,
                        MaNhom = thuoc.MaNhom,
                        TenNhom = nhom.TenNhom
                    }
                );

            if (maNhom.HasValue)
            {
                thuocsQuery = thuocsQuery.Where(t => t.MaNhom == maNhom.Value);
            }

            var thuocs = thuocsQuery.ToList();
            return View(thuocs);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _context.Thuocs
                .Where(t => t.TenThuoc.Contains(query))
                .Join(
                    _context.NhomThuocs,
                    thuoc => thuoc.MaNhom,
                    nhom => nhom.MaNhom,
                    (thuoc, nhom) => new
                    {
                        MaThuoc = thuoc.MaThuoc,
                        TenThuoc = thuoc.TenThuoc,
                        HamLuong = thuoc.HamLuong,
                        HoatChat = thuoc.HoatChat,
                        DonViTinh = thuoc.DonViTinh,
                        Gia = thuoc.Gia,
                        TenNhom = nhom.TenNhom
                    }
                )
                .ToList();

            return PartialView("_ThuocTableRows", results);
        }

        [Route("Dashboard/Thuoc/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Thuoc newThuoc)
        {
            if (ModelState.IsValid)
            {
                _context.Thuocs.Add(newThuoc);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.NhomThuocs = _context.NhomThuocs.ToList();
            return View(newThuoc);
        }

        [Route("Dashboard/Thuoc/Edit/{id}")]
        [HttpGet]
        public IActionResult EditThuoc(int id)
        {
            var thuoc = _context.Thuocs
                .Where(t => t.MaThuoc == id)
                .Select(t => new
                {
                    MaThuoc = t.MaThuoc,
                    TenThuoc = t.TenThuoc,
                    HamLuong = t.HamLuong,
                    HoatChat = t.HoatChat,
                    DonViTinh = t.DonViTinh,
                    Gia = t.Gia,
                    MaNhom = t.MaNhom,
                })
                .FirstOrDefault();

            if (thuoc == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc nào." });
            }
                ViewBag.NhomThuocs = _context.NhomThuocs.ToList();


            return Json(thuoc);
        }


        [Route("Dashboard/Thuoc/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNhomThuoc(int id, Thuoc model)
        {
            if (id != model.MaThuoc)
            {
                return BadRequest(new { message = "Mã thuốc không khớp." });
            }
            try
            {
                var thuoc = _context.Thuocs.Find(id);
                if (thuoc != null)
                {
                    thuoc.MaNhom = model.MaThuoc;
                    thuoc.TenThuoc = model.TenThuoc;
                    thuoc.HamLuong = model.HamLuong;
                    thuoc.HoatChat = model.HoatChat;
                    thuoc.DonViTinh = model.DonViTinh;
                    thuoc.Gia = model.Gia;
                    thuoc.MaNhom = model.MaNhom;

                    _context.Thuocs.Update(thuoc);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(new { message = "Thuốc không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [Route("Dashboard/Thuoc/Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var t = _context.Thuocs.Find(id);
                if (t == null)
                {
                    return Json(new { success = false, message = "Thuốc không tồn tại." });
                }

                bool hasRelatedEntries = _context.ChiTietPhieuNhaps.Any(p => p.MaThuoc == id)
                                        || _context.ChiTietPhieuXuats.Any(t => t.MaThuoc == id);


                if (hasRelatedEntries)
                {
                    return Json(new { success = false, message = "Không thể xóa thuốc vì đang có dữ liệu liên kết." });
                }

                _context.Thuocs.Remove(t);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá thuốc thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xoá thuốc" });
            }
        }




    }
}