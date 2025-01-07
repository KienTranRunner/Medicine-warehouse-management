using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public DashboardController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var doanhThuNhap = await _context.PhieuNhaps
                .SelectMany(pn => pn.ChiTietPhieuNhaps, (pn, ctpn) => new
                {
                    pn.NgayNhap,
                    DoanhThuNhap = ctpn.SoLuongNhap * ctpn.DonGia
                })
                .GroupBy(x => new { x.NgayNhap.Year, x.NgayNhap.Month })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThuNhap = g.Sum(x => x.DoanhThuNhap)
                })
                .ToListAsync();

            var doanhThuXuat = await _context.PhieuXuats
                .SelectMany(px => px.ChiTietPhieuXuats, (px, ctpx) => new
                {
                    px.NgayXuat,
                    DoanhThuXuat = ctpx.SoLuongXuat * ctpx.DonGia
                })
                .GroupBy(x => new { x.NgayXuat.Year, x.NgayXuat.Month })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThuXuat = g.Sum(x => x.DoanhThuXuat)
                })
                .ToListAsync();

            // Combine revenue data and calculate profit
            var doanhThuThang = (from n in doanhThuNhap
                                 join x in doanhThuXuat on new { n.Nam, n.Thang } equals new { x.Nam, x.Thang } into nx
                                 from x in nx.DefaultIfEmpty()
                                 select new
                                 {
                                     Year = n.Nam,
                                     Month = n.Thang,
                                     TotalExpense = n.DoanhThuNhap,
                                     TotalRevenue = x?.DoanhThuXuat ?? 0,
                                     Profit = (x?.DoanhThuXuat ?? 0) - n.DoanhThuNhap
                                 })
                                 .OrderBy(d => d.Year)
                                 .ThenBy(d => d.Month)
                                 .ToList();

            ViewBag.ChartData = doanhThuThang;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var thuocXuatNhieuThangNay = await _context.ChiTietPhieuXuats
               .Include(ctpx => ctpx.MaThuocNavigation)
               .Include(ctpx => ctpx.SoPhieuNavigation) 
               .Where(ctpx => ctpx.SoPhieuNavigation.NgayXuat.Month == currentMonth && ctpx.SoPhieuNavigation.NgayXuat.Year == currentYear)
               .GroupBy(ctpx => ctpx.MaThuoc)
               .Select(g => new
               {
                   TenThuoc = g.FirstOrDefault().MaThuocNavigation.TenThuoc,
                   SoLuongXuat = g.Sum(ctpx => ctpx.SoLuongXuat ?? 0)
               })
               .OrderByDescending(t => t.SoLuongXuat)
               .Take(5)
               .ToListAsync();

            ViewBag.ThuocXuatNhieu = thuocXuatNhieuThangNay;


            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;

            var totalExpenseThisMonth = await _context.PhieuNhaps
                .Where(pn => pn.NgayNhap.Month == currentMonth && pn.NgayNhap.Year == currentYear)
                .SelectMany(pn => pn.ChiTietPhieuNhaps)
                .SumAsync(ctpn => ctpn.SoLuongNhap * ctpn.DonGia ?? 0);

            // Tổng giá xuất tháng này
            var totalRevenueThisMonth = await _context.PhieuXuats
                .Where(px => px.NgayXuat.Month == currentMonth && px.NgayXuat.Year == currentYear)
                .SelectMany(px => px.ChiTietPhieuXuats)
                .SumAsync(ctpx => ctpx.SoLuongXuat * ctpx.DonGia ?? 0);

            // Các lô thuốc hết hạn tháng này
            var expiredLotsThisMonth = await _context.ChiTietPhieuNhaps
                .Where(ctpn => ctpn.HanSuDung.HasValue &&
                               ctpn.HanSuDung.Value.Month == currentMonth &&
                               ctpn.HanSuDung.Value.Year == currentYear)
                .ToListAsync();

            // Tổng phiếu nhập tháng này
            var totalPhieuNhapThisMonth = await _context.PhieuNhaps
                .CountAsync(pn => pn.NgayNhap.Month == currentMonth && pn.NgayNhap.Year == currentYear);

            // Tổng phiếu xuất tháng này
            var totalPhieuXuatThisMonth = await _context.PhieuXuats
                .CountAsync(px => px.NgayXuat.Month == currentMonth && px.NgayXuat.Year == currentYear);

            ViewBag.TotalExpenseThisMonth = totalExpenseThisMonth;
            ViewBag.TotalRevenueThisMonth = totalRevenueThisMonth;
            ViewBag.ExpiredLotsThisMonth = expiredLotsThisMonth.Count;
            ViewBag.TotalPhieuNhapThisMonth = totalPhieuNhapThisMonth;
            ViewBag.TotalPhieuXuatThisMonth = totalPhieuXuatThisMonth;


            return View();
        }

        


    }
}