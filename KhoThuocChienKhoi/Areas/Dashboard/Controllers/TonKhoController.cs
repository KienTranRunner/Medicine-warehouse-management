using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class TonKhoController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public TonKhoController(KhoThuocChienKhoiContext context)
        {
            _context = context;

        }



        [Route("Dashboard/TonKho")]
        [HttpGet]
        public IActionResult Index()
        {
            var thuocs = _context.Thuocs
                .Select(t => new Thuoc
                {
                    MaThuoc = t.MaThuoc,
                    TenThuoc = t.TenThuoc,
                    DonViTinh = t.DonViTinh,
                    HamLuong = t.HamLuong,
                    HoatChat = t.HoatChat,
                }).ToList();


            return View(thuocs);

        }

        [HttpGet]
        [Route("Dashboard/TonKho/chiTietLo/{maThuoc}")]
        public async Task<IActionResult> chiTietLo(int maThuoc)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var groupedData = await (
                from nhap in _context.ChiTietPhieuNhaps
                where nhap.MaThuoc == maThuoc && nhap.HanSuDung.HasValue && nhap.HanSuDung.Value >= today
                group nhap by new { nhap.MaThuoc, nhap.SoLo, nhap.HanSuDung } into grouped
                select new
                {
                    MaThuoc = grouped.Key.MaThuoc,
                    SoLo = grouped.Key.SoLo,
                    HanSuDung = grouped.Key.HanSuDung,
                    SoLuongNhap = grouped.Sum(g => g.SoLuongNhap),
                    SoLuongXuat = _context.ChiTietPhieuXuats
                        .Where(x => x.MaThuoc == grouped.Key.MaThuoc && x.SoLo == grouped.Key.SoLo)
                        .Sum(x => x.SoLuongXuat),
                    SoLuongHuHong = grouped.Sum(g => g.HuHong)
                }
            ).ToListAsync();

            var medicineDetails = groupedData
                .Select(g => new
                {
                    g.MaThuoc,
                    g.SoLo,
                    g.HanSuDung,
                    g.SoLuongNhap,
                    g.SoLuongXuat,
                    g.SoLuongHuHong,
                    SoLuongHienTai = g.SoLuongNhap - g.SoLuongXuat - g.SoLuongHuHong
                })
                .Where(g => g.SoLuongHienTai >= 0)
                .ToList();

            return Json(medicineDetails);
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

            return PartialView("_TonKhoTableRows", results);
        }
        [HttpGet]
        [Route("Dashboard/TonKho/ByMaThuoc/{maThuoc}")]
        public async Task<IActionResult> TonKhoByMaThuoc(int maThuoc, int? thang, int? nam)
        {
            var query = _context.TonKhos.Where(tk => tk.MaThuoc == maThuoc);

            // Lọc theo tháng nếu có giá trị
            if (thang.HasValue)
            {
                query = query.Where(tk => tk.Thang == thang.Value);
            }

            // Lọc theo năm nếu có giá trị
            if (nam.HasValue)
            {
                query = query.Where(tk => tk.Nam == nam.Value);
            }

            var tonKhoRecords = await query
                .Select(tk => new
                {
                    tk.Thang,
                    tk.Nam,
                    tk.TonDau,
                    tk.TongNhap,
                    tk.TongXuat,
                    tk.HuHong,
                    tk.TonCuoi
                })
                .ToListAsync();

            return Json(tonKhoRecords);
        }


        [HttpPost]
        [Route("Dashboard/TonKho/Update")]
        public async Task<IActionResult> UpdateTonKho()
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC usp_UpdateTonDauForNextMonth");

            return Ok(new { message = "Tồn kho đã được cập nhật cho tháng tiếp theo." });
        }


    }
}