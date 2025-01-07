using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhoThuocChienKhoi.Models;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class LoThuocHetHanController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public LoThuocHetHanController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("Dashboard/LoThuocHetHan")]
        public IActionResult Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var loHetHan = _context.ChiTietPhieuNhaps
                .Where(c => c.HanSuDung.HasValue && c.HanSuDung.Value < today)
                .Select(nhap => new
                {
                    nhap.SoLo,
                    nhap.SoPhieu,
                    nhap.HanSuDung,
                    TenThuoc = _context.Thuocs.FirstOrDefault(m => m.MaThuoc == nhap.MaThuoc).TenThuoc,
                    SoLuongNhap = nhap.SoLuongNhap,
                    SoLuongXuat = _context.ChiTietPhieuXuats
                        .Where(x => x.MaThuoc == nhap.MaThuoc && x.SoLo == nhap.SoLo)
                        .Sum(x => x.SoLuongXuat)
                })
                .Select(g => new
                {
                    g.SoLo,
                    g.SoPhieu,
                    g.HanSuDung,
                    g.TenThuoc,
                    SoLuongHienTai = g.SoLuongNhap - g.SoLuongXuat
                })
                .Where(g => g.SoLuongHienTai > 0)
                .ToList();

            var loSapHetHan = _context.ChiTietPhieuNhaps
                .Where(c => c.HanSuDung.HasValue && c.HanSuDung.Value >= today && c.HanSuDung.Value <= today.AddDays(5))
                .Select(nhap => new
                {
                    nhap.SoLo,
                    nhap.SoPhieu,
                    nhap.HanSuDung,
                    TenThuoc = _context.Thuocs.FirstOrDefault(m => m.MaThuoc == nhap.MaThuoc).TenThuoc,
                    SoLuongNhap = nhap.SoLuongNhap,
                    SoLuongXuat = _context.ChiTietPhieuXuats
                        .Where(x => x.MaThuoc == nhap.MaThuoc && x.SoLo == nhap.SoLo)
                        .Sum(x => x.SoLuongXuat),
                    DaysUntilExpiry = (nhap.HanSuDung.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days
                })
                .Select(g => new
                {
                    g.SoLo,
                    g.SoPhieu,
                    g.HanSuDung,
                    g.TenThuoc,
                    SoLuongHienTai = g.SoLuongNhap - g.SoLuongXuat,
                    g.DaysUntilExpiry
                })
                .Where(g => g.SoLuongHienTai > 0)
                .ToList();

            ViewBag.loHetHan = loHetHan;
            ViewBag.loSapHetHan = loSapHetHan;

            return View();
        }




    }
}
