using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class XuatKhoController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public XuatKhoController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [Route("Dashboard/XuatKho")]
        [HttpGet]
        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            // Lấy danh sách thuốc và khách hàng để hiển thị trong View (nếu cần)
            ViewBag.Thuocs = _context.Thuocs.Select(t => new
            {
                t.MaThuoc,
                t.TenThuoc,
                t.DonViTinh
            }).ToList();

            ViewBag.KhachHangs = _context.KhachHangs.ToList();

            // Truy vấn dữ liệu từ cơ sở dữ liệu
            var phieuXuatQuery = _context.PhieuXuats
                .Join(_context.ChiTietPhieuXuats, px => px.SoPhieu, ctpx => ctpx.SoPhieu, (px, ctpx) => new { px, ctpx })
                .Join(_context.Thuocs, joinResult => joinResult.ctpx.MaThuoc, t => t.MaThuoc, (joinResult, t) => new { joinResult.px, joinResult.ctpx, t })
                .Join(_context.TaiKhoanNhanViens, finalJoin => finalJoin.px.MaTaiKhoan, tk => tk.MaTaiKhoan, (finalJoin, tk) => new
                {
                    SoPhieu = finalJoin.px.SoPhieu,
                    SoChungTu = finalJoin.px.SoChungTu,
                    NgayXuat = finalJoin.px.NgayXuat,
                    MaNVLapPhieu = tk.MaNhanVien,
                    DonViTinh = finalJoin.t.DonViTinh,
                    LyDoXuat = finalJoin.px.LyDoXuat,
                    DonGia = finalJoin.ctpx.DonGia,
                    SoLuongXuat = finalJoin.ctpx.SoLuongXuat,
                    ThanhTien = finalJoin.ctpx.SoLuongXuat * finalJoin.ctpx.DonGia
                });

            // Áp dụng bộ lọc theo ngày nếu có giá trị
            if (startDate.HasValue && endDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);

                phieuXuatQuery = phieuXuatQuery
                    .Where(p => p.NgayXuat >= startDateOnly && p.NgayXuat <= endDateOnly);
            }


            // Nhóm dữ liệu và tính tổng thành tiền
            var phieuXuat = phieuXuatQuery
                .GroupBy(item => new
                {
                    item.SoPhieu,
                    item.SoChungTu,
                    item.NgayXuat,
                    item.MaNVLapPhieu,
                    item.LyDoXuat
                })
                .Select(group => new
                {
                    SoPhieu = group.Key.SoPhieu,
                    SoChungTu = group.Key.SoChungTu,
                    NgayXuat = group.Key.NgayXuat,
                    MaNVLapPhieu = group.Key.MaNVLapPhieu,
                    LyDoXuat = group.Key.LyDoXuat,
                    ThanhTien = group.Sum(x => x.ThanhTien)
                })
                .ToList();

            // Trả về View với danh sách phiếu xuất đã lọc
            return View(phieuXuat);
        }



        [HttpGet]
        [Route("Dashboard/XuatKho/chiTietLo/{maThuoc}")]
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

        [HttpPost]
        [Route("Dashboard/XuatKho/Create")]
        public async Task<IActionResult> Create(PhieuXuatViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.PhieuXuats.Any(pn => pn.SoChungTu == model.SoChungTu))
                {
                    TempData["ErrorMessage"] = "Số chứng từ này đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }


                var phieuXuat = new PhieuXuat
                {
                    SoChungTu = model.SoChungTu,
                    NgayXuat = DateOnly.FromDateTime(model.NgayXuat),
                    LyDoXuat = model.LyDoXuat,
                    MaTaiKhoan = model.MaTaiKhoan,
                    MaKhachHang = model.MaKhachHang
                };

                _context.PhieuXuats.Add(phieuXuat);
                await _context.SaveChangesAsync();

                // Loop through each medicine in the PhieuXuat and update TonKho accordingly
                foreach (var medicine in model.Medicines)
                {
                    var chiTietPhieuXuat = new ChiTietPhieuXuat
                    {
                        SoPhieu = phieuXuat.SoPhieu,
                        MaThuoc = medicine.MaThuoc,
                        SoLo = medicine.SoLo,
                        DonGia = medicine.DonGia,
                        SoLuongXuat = medicine.SoLuongXuat
                    };

                    _context.ChiTietPhieuXuats.Add(chiTietPhieuXuat);

                    // Update the inventory in TonKho for the given MaThuoc
                    var month = model.NgayXuat.Month;
                    var year = model.NgayXuat.Year;
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.MaThuoc == medicine.MaThuoc && t.Thang == month && t.Nam == year);

                    if (tonKho == null)
                    {
                        tonKho = new TonKho
                        {
                            Thang = month,
                            Nam = year,
                            MaThuoc = medicine.MaThuoc,
                            TonDau = await GetLastMonthTonCuoi(medicine.MaThuoc, month, year),
                            TongNhap = 0,
                            TongXuat = medicine.SoLuongXuat,
                            HuHong = 0,
                            TonCuoi = 0
                        };

                        _context.TonKhos.Add(tonKho);
                    }
                    else
                    {
                        tonKho.TongXuat += medicine.SoLuongXuat;
                    }

                    tonKho.TonCuoi = (tonKho.TonDau ?? 0) + tonKho.TongNhap - tonKho.TongXuat - tonKho.HuHong;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        private async Task<int> GetLastMonthTonCuoi(int maThuoc, int currentMonth, int currentYear)
        {
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var lastYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var lastTonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.MaThuoc == maThuoc && t.Thang == lastMonth && t.Nam == lastYear);

            return lastTonKho?.TonCuoi ?? 0;
        }





        // Lấy thông tin 
        [HttpGet]
        [Route("Dashboard/XuatKho/Edit/{soPhieu}")]
        public async Task<IActionResult> Edit(int soPhieu)
        {
            var phieuXuat = await _context.PhieuXuats
                .Where(px => px.SoPhieu == soPhieu)
                .Select(px => new
                {
                    px.SoPhieu,
                    px.SoChungTu,
                    NgayXuat = px.NgayXuat,
                    px.LyDoXuat,
                    px.MaKhachHang,
                    Medicines = px.ChiTietPhieuXuats.Select(ct => new
                    {
                        ct.MaThuoc,
                        TenThuoc = _context.Thuocs
                            .Where(t => t.MaThuoc == ct.MaThuoc)
                            .Select(t => t.TenThuoc)
                            .FirstOrDefault(),
                        ct.SoLo,
                        ct.SoLuongXuat,
                        ct.DonGia,
                        DonViTinh = _context.Thuocs
                            .Where(t => t.MaThuoc == ct.MaThuoc)
                            .Select(t => t.DonViTinh)
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (phieuXuat == null)
            {
                return NotFound();
            }


            return Json(phieuXuat);
        }
        [HttpPost]
        [Route("Dashboard/XuatKho/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PhieuXuatViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
          


            var phieuXuat = await _context.PhieuXuats
                .Include(px => px.ChiTietPhieuXuats)
                .FirstOrDefaultAsync(px => px.SoPhieu == model.SoPhieu);

            if (phieuXuat == null)
            {
                return NotFound();
            }

            var oldChiTietPhieuXuats = phieuXuat.ChiTietPhieuXuats.ToList();

            phieuXuat.SoChungTu = model.SoChungTu;
            phieuXuat.NgayXuat = DateOnly.FromDateTime(model.NgayXuat);
            phieuXuat.LyDoXuat = model.LyDoXuat;
            phieuXuat.MaKhachHang = model.MaKhachHang;

            phieuXuat.ChiTietPhieuXuats.Clear();
            foreach (var medicine in model.Medicines)
            {
                phieuXuat.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
                {
                    MaThuoc = medicine.MaThuoc,
                    SoLo = medicine.SoLo,
                    SoLuongXuat = medicine.SoLuongXuat,
                    DonGia = medicine.DonGia
                });
            }

            var month = model.NgayXuat.Month;
            var year = model.NgayXuat.Year;

            foreach (var oldDetail in oldChiTietPhieuXuats)
            {
                var oldTonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.MaThuoc == oldDetail.MaThuoc && t.Thang == month && t.Nam == year);

                if (oldTonKho != null)
                {
                    oldTonKho.TongXuat -= oldDetail.SoLuongXuat;
                    oldTonKho.TonCuoi = (oldTonKho.TonDau ?? 0) + oldTonKho.TongNhap - oldTonKho.TongXuat - oldTonKho.HuHong;
                }
            }

            foreach (var newDetail in model.Medicines)
            {
                var newTonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.MaThuoc == newDetail.MaThuoc && t.Thang == month && t.Nam == year);

                if (newTonKho == null)
                {
                    newTonKho = new TonKho
                    {
                        Thang = month,
                        Nam = year,
                        MaThuoc = newDetail.MaThuoc,
                        TonDau = await GetLastMonthTonCuoi(newDetail.MaThuoc, month, year),
                        TongNhap = 0,
                        TongXuat = newDetail.SoLuongXuat,
                        HuHong = 0,
                        TonCuoi = 0
                    };

                    _context.TonKhos.Add(newTonKho);
                }
                else
                {
                    newTonKho.TongXuat += newDetail.SoLuongXuat;
                }

                // Cập nhật tồn cuối cho tháng hiện tại
                newTonKho.TonCuoi = (newTonKho.TonDau ?? 0) + newTonKho.TongNhap - newTonKho.TongXuat - newTonKho.HuHong;
            }


            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }


        [HttpPost]
        [Route("Dashboard/XuatKho/Delete/{soPhieu}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int soPhieu)
        {
            // Lấy thông tin phiếu xuất bao gồm các chi tiết phiếu xuất
            var phieuXuat = await _context.PhieuXuats
                .Include(px => px.ChiTietPhieuXuats)
                .FirstOrDefaultAsync(px => px.SoPhieu == soPhieu);

            if (phieuXuat == null)
            {
                return Json(new { success = false, message = "Phiếu xuất không tồn tại." });
            }

            try
            {
                // Cập nhật tồn kho trước khi xóa phiếu xuất
                foreach (var chiTietPhieuXuat in phieuXuat.ChiTietPhieuXuats)
                {
                    var maThuoc = chiTietPhieuXuat.MaThuoc;
                    var soLuongXuat = chiTietPhieuXuat.SoLuongXuat;
                    var thangXuat = phieuXuat.NgayXuat.Month;
                    var namXuat = phieuXuat.NgayXuat.Year;

                    // Lấy bản ghi tồn kho của thuốc tương ứng theo tháng và năm của phiếu xuất
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(tk => tk.MaThuoc == maThuoc && tk.Thang == thangXuat && tk.Nam == namXuat);

                    if (tonKho != null)
                    {
                        // Cập nhật lại số lượng xuất và tồn cuối
                        tonKho.TongXuat -= soLuongXuat;

                        // Đảm bảo số lượng không bị âm
                        if (tonKho.TongXuat < 0)
                        {
                            tonKho.TongXuat = 0;
                        }

                        // Tính lại tồn cuối
                        tonKho.TonCuoi = (tonKho.TonDau ?? 0) + (tonKho.TongNhap ?? 0) - (tonKho.TongXuat ?? 0) - (tonKho.HuHong ?? 0);

                        // Đảm bảo tồn cuối không bị âm
                        if (tonKho.TonCuoi < 0)
                        {
                            tonKho.TonCuoi = 0;
                        }

                        _context.TonKhos.Update(tonKho);
                    }
                }

                // Xóa các chi tiết phiếu xuất
                _context.ChiTietPhieuXuats.RemoveRange(phieuXuat.ChiTietPhieuXuats);

                // Xóa phiếu xuất
                _context.PhieuXuats.Remove(phieuXuat);

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa phiếu xuất và điều chỉnh tồn kho thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phiếu xuất." });
            }
        }
        [HttpGet]
        [Route("Dashboard/XuatKho/ExportExcel")]
        public IActionResult ExportExcel()
        {
            var phieuXuats = _context.PhieuXuats
                .Select(px => new
                {
                    px.SoPhieu,
                    px.SoChungTu,
                    px.NgayXuat,
                    Thang = px.NgayXuat.Month,
                    Nam = px.NgayXuat.Year,
                    KhachHang = px.MaKhachHangNavigation.TenKhachHang,
                    DiaChiKH = px.MaKhachHangNavigation.DiaChi,
                    NguoiXuat = px.MaTaiKhoanNavigation.HoTen,
                    ChiTiet = px.ChiTietPhieuXuats.Select(ct => new
                    {
                        ct.MaThuoc,
                        Thuoc = ct.MaThuocNavigation.TenThuoc,
                        ct.SoLo,
                        ct.SoLuongXuat,
                        ct.DonGia,
                        ThanhTien = ct.SoLuongXuat * ct.DonGia
                    }).ToList()
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("XuatKho");

                // Tạo header
                worksheet.Cell(1, 1).Value = "Tháng";
                worksheet.Cell(1, 2).Value = "Năm";
                worksheet.Cell(1, 3).Value = "Số Phiếu";
                worksheet.Cell(1, 4).Value = "Số Chứng Từ";
                worksheet.Cell(1, 5).Value = "Ngày Xuất";
                worksheet.Cell(1, 6).Value = "Khách Hàng";
                worksheet.Cell(1, 7).Value = "Địa Chỉ KH";
                worksheet.Cell(1, 8).Value = "Người Xuất";
                worksheet.Cell(1, 9).Value = "Mã Thuốc";
                worksheet.Cell(1, 10).Value = "Tên Thuốc";
                worksheet.Cell(1, 11).Value = "Số Lô";
                worksheet.Cell(1, 12).Value = "Số Lượng Xuất";
                worksheet.Cell(1, 13).Value = "Đơn Giá";
                worksheet.Cell(1, 14).Value = "Thành Tiền";

                int row = 2;

                var groupedPhieuXuats = phieuXuats
                    .GroupBy(px => new { px.Thang, px.Nam })
                    .OrderBy(g => g.Key.Nam)
                    .ThenBy(g => g.Key.Thang);

                foreach (var group in groupedPhieuXuats)
                {
                    foreach (var px in group)
                    {
                        foreach (var ct in px.ChiTiet)
                        {
                            worksheet.Cell(row, 1).Value = group.Key.Thang;
                            worksheet.Cell(row, 2).Value = group.Key.Nam;
                            worksheet.Cell(row, 3).Value = px.SoPhieu;
                            worksheet.Cell(row, 4).Value = px.SoChungTu;
                            worksheet.Cell(row, 5).Value = px.NgayXuat.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 6).Value = px.KhachHang;
                            worksheet.Cell(row, 7).Value = px.DiaChiKH;
                            worksheet.Cell(row, 8).Value = px.NguoiXuat;
                            worksheet.Cell(row, 9).Value = ct.MaThuoc;
                            worksheet.Cell(row, 10).Value = ct.Thuoc;
                            worksheet.Cell(row, 11).Value = ct.SoLo;
                            worksheet.Cell(row, 12).Value = ct.SoLuongXuat;
                            worksheet.Cell(row, 13).Value = ct.DonGia?.ToString("C0", new CultureInfo("vi-VN"));
                            worksheet.Cell(row, 14).Value = ct.ThanhTien?.ToString("C0", new CultureInfo("vi-VN"));
                            row++;
                        }
                    }
                }

                // Thiết lập định dạng cho bảng Excel
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoXuatKhoTheoThang.xlsx");
                }
            }
        }



    }
}


