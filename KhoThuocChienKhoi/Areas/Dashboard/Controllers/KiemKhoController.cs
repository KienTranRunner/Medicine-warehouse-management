using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhoThuocChienKhoi.Models;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class KiemKhoController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public KiemKhoController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Dashboard/KiemKho")]
        public IActionResult Index(int? thang = null, int? nam = null, int? maNhom = null)
        {
            thang ??= DateTime.Now.Month;
            nam ??= DateTime.Now.Year;

            ViewBag.NhomThuocs = _context.NhomThuocs.ToList();
            ViewBag.SelectedNhomName = maNhom.HasValue
                ? _context.NhomThuocs.FirstOrDefault(n => n.MaNhom == maNhom.Value)?.TenNhom
                : "Tất cả";

            var today = DateOnly.FromDateTime(DateTime.Today);

            var tonKhoQuery = _context.TonKhos
                .Where(tk => tk.Thang == thang && tk.Nam == nam)
                .Join(_context.Thuocs, tk => tk.MaThuoc, t => t.MaThuoc, (tk, t) => new
                {
                    tk.MaTon,
                    tk.MaThuoc,
                    t.TenThuoc,
                    t.HamLuong,
                    t.HoatChat,
                    t.DonViTinh,
                    tk.TonCuoi,
                    tk.SoLuongThucTe,
                    tk.HuHong,
                    tk.ChenhLech,
                    t.MaNhom,
                    TongHuHong = _context.ChiTietPhieuNhaps
                        .Where(ct => ct.MaThuoc == tk.MaThuoc && ct.HanSuDung >= today)
                        .Sum(ct => (int?)ct.HuHong) ?? 0
                });

            if (maNhom.HasValue)
                tonKhoQuery = tonKhoQuery.Where(tk => tk.MaNhom == maNhom.Value);

            ViewBag.Thang = thang;
            ViewBag.Nam = nam;

            return View(tonKhoQuery.ToList());
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var results = _context.TonKhos
                .Where(tk => tk.Thang == currentMonth && tk.Nam == currentYear)
                .Join(_context.Thuocs, tk => tk.MaThuoc, t => t.MaThuoc, (tk, t) => new
                {
                    tk.MaTon,
                    tk.MaThuoc,
                    t.TenThuoc,
                    t.HamLuong,
                    t.HoatChat,
                    t.DonViTinh,
                    tk.TonCuoi,
                    tk.SoLuongThucTe,
                    tk.HuHong,
                    tk.ChenhLech,
                    t.MaNhom,
                    TongHuHong = _context.ChiTietPhieuNhaps
                        .Where(ct => ct.MaThuoc == tk.MaThuoc && ct.HanSuDung >= today)
                        .Sum(ct => (int?)ct.HuHong) ?? 0
                })
                .Where(t => t.TenThuoc.Contains(query))
                .ToList();

            return PartialView("_KiemKhoThuocTableRows", results);
        }

        [HttpGet]
        [Route("Dashboard/KiemKho/GetChiTietLo")]
        public IActionResult GetChiTietLo(int maThuoc)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var chiTietLo = _context.ChiTietPhieuNhaps
    .Where(ct => ct.MaThuoc == maThuoc && ct.HanSuDung >= today)
    .Select(ct => new
    {
        ct.SoPhieu,
        ct.MaThuoc,
        SoLo = ct.SoLo, // Luôn trả về dạng chuỗi
        ct.HanSuDung,
        ct.SoLuongNhap,
        SoLuongXuat = _context.ChiTietPhieuXuats
            .Where(px => px.SoLo == ct.SoLo) // So sánh chuỗi
            .Sum(px => (int?)px.SoLuongXuat) ?? 0,
        ct.HuHong
    })
    .ToList();



            return PartialView("_ChiTietLo", chiTietLo);
        }

        [HttpPost]
        public IActionResult CapNhatHuHong([FromBody] List<ChiTietHuHong> chiTietHuHong)
        {
            foreach (var item in chiTietHuHong)
            {
                var chiTiet = _context.ChiTietPhieuNhaps.FirstOrDefault(
                    ct => ct.SoPhieu == item.SoPhieu &&
                          ct.MaThuoc == item.MaThuoc &&
                          ct.SoLo == item.SoLo); // So sánh chuỗi

                if (chiTiet != null)
                    chiTiet.HuHong = item.HuHong;
            }


            _context.SaveChanges();
            return Json(new { success = true, message = "Cập nhật hư hỏng thành công!" });
        }

        [HttpPost]
        public IActionResult CapNhatKiemKho([FromBody] KiemKhoUpdateModel model)
        {
            var tonKho = _context.TonKhos.FirstOrDefault(t => t.MaTon == model.MaTon);

            if (tonKho == null)
                return Json(new { success = false, message = "Không tìm thấy tồn kho!" });

            var today = DateOnly.FromDateTime(DateTime.Today);
            var tongHuHongLoThuoc = _context.ChiTietPhieuNhaps
                .Where(ct => ct.MaThuoc == tonKho.MaThuoc && ct.HanSuDung >= today)
                .Sum(ct => (int?)ct.HuHong) ?? 0;

            tonKho.SoLuongThucTe = model.SoLuongThucTe;
            tonKho.HuHong = tongHuHongLoThuoc;
            tonKho.ChenhLech = (model.SoLuongThucTe + tongHuHongLoThuoc) - tonKho.TonCuoi;

            _context.SaveChanges();
            return Json(new { success = true, message = "Cập nhật kiểm kho thành công!" });
        }

        public class ChiTietHuHong
        {
            public int SoPhieu { get; set; }
            public int MaThuoc { get; set; }
            public string SoLo { get; set; }
            public int HuHong { get; set; }
        }

        public class KiemKhoUpdateModel
        {
            public int MaTon { get; set; }
            public int SoLuongThucTe { get; set; }
        }
    }
}
