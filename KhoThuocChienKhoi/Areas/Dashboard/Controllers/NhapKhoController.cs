using System;
using System.Collections.Generic;
using System.Data;
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
    public class NhapKhoController : Controller
    {
        private readonly KhoThuocChienKhoiContext _context;

        public NhapKhoController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }


        [Route("Dashboard/NhapKho")]
        [HttpGet]
        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            ViewBag.Thuocs = _context.Thuocs.Select(t => new
            {
                t.MaThuoc,
                t.TenThuoc,
                t.DonViTinh
            }).ToList();

            ViewBag.NhaCungCaps = _context.NhaCungCaps.ToList();

            var phieuNhapQuery = _context.PhieuNhaps
                .Join(_context.ChiTietPhieuNhaps, pn => pn.SoPhieu, ctpn => ctpn.SoPhieu, (pn, ctpn) => new { pn, ctpn })
                .Join(_context.Thuocs, joinResult => joinResult.ctpn.MaThuoc, t => t.MaThuoc, (joinResult, t) => new { joinResult.pn, joinResult.ctpn, t })
                .Join(_context.TaiKhoanNhanViens, finalJoin => finalJoin.pn.MaTaiKhoan, tk => tk.MaTaiKhoan, (finalJoin, tk) => new
                {
                    SoPhieu = finalJoin.pn.SoPhieu,
                    SoChungTu = finalJoin.pn.SoChungTu,
                    NgayNhap = finalJoin.pn.NgayNhap,
                    MaNVLapPhieu = tk.MaNhanVien,
                    LyDoNhap = finalJoin.pn.LyDoNhap,
                    DonGia = finalJoin.ctpn.DonGia,
                    SoLuongNhap = finalJoin.ctpn.SoLuongNhap,
                    HuHong = finalJoin.ctpn.HuHong,
                    ThanhTien = finalJoin.ctpn.SoLuongNhap * finalJoin.ctpn.DonGia
                })
                .GroupBy(item => new
                {
                    item.SoPhieu,
                    item.SoChungTu,
                    item.NgayNhap,
                    item.MaNVLapPhieu,
                    item.LyDoNhap
                })
                .Select(group => new
                {
                    SoPhieu = group.Key.SoPhieu,
                    SoChungTu = group.Key.SoChungTu,
                    NgayNhap = group.Key.NgayNhap,
                    MaNVLapPhieu = group.Key.MaNVLapPhieu,
                    LyDoNhap = group.Key.LyDoNhap,
                    ThanhTien = group.Sum(x => x.ThanhTien)
                })
                .AsEnumerable(); // Chuyển sang client-side evaluation để xử lý DateOnly

            // Lọc theo khoảng thời gian nếu startDate và endDate có giá trị
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date;
                phieuNhapQuery = phieuNhapQuery
                    .Where(p => p.NgayNhap >= DateOnly.FromDateTime(start) &&
                                p.NgayNhap <= DateOnly.FromDateTime(end));
            }

            // Chuyển kết quả truy vấn thành danh sách
            var phieuNhap = phieuNhapQuery.ToList();

            return View(phieuNhap);
        }


        [HttpPost]
        [Route("Dashboard/NhapKho/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuNhapViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (_context.PhieuNhaps.Any(pn => pn.SoChungTu == model.SoChungTu))
                {
                    TempData["ErrorMessage"] = "Số chứng từ này đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                if (model.Medicines.Any(m => _context.ChiTietPhieuNhaps.Any(ct => ct.SoLo == m.SoLo)))
                {
                    TempData["ErrorMessage"] = "Số lô này đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }




                var phieuNhap = new PhieuNhap
                {

                    SoChungTu = model.SoChungTu,
                    NgayNhap = DateOnly.FromDateTime(model.NgayNhap),
                    LyDoNhap = model.LyDoNhap,
                    MaNhaCungCap = model.MaNhaCungCap,
                    MaTaiKhoan = model.MaTaiKhoan
                };

                var chiTietPhieuNhaps = model.Medicines.Select(m => new ChiTietPhieuNhap
                {
                    MaThuoc = m.MaThuoc,
                    SoLo = m.SoLo,
                    HanSuDung = m.HanSuDung.HasValue ? DateOnly.FromDateTime(m.HanSuDung.Value) : (DateOnly?)null,
                    SoLuongNhap = m.SoLuongNhap,
                    DonGia = m.DonGia,
                    HuHong = 0
                }).ToList();

                phieuNhap.ChiTietPhieuNhaps = chiTietPhieuNhaps;

                try
                {
                    int currentMonth = model.NgayNhap.Month;
                    int currentYear = model.NgayNhap.Year;
                    int previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                    int previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                    var tonDauChuaCapNhat = await _context.TonKhos
                        .AnyAsync(t => t.Thang == currentMonth && t.Nam == currentYear && t.TonDau == null);

                    if (tonDauChuaCapNhat)
                    {
                        TempData["UpdateTonKhoWarning"] = "Tồn đầu của tháng này chưa được cập nhật. Hãy cập nhật tồn kho đầu tháng mới trước khi tiếp tục nhập kho.";
                        return RedirectToAction("Index", "TonKho");
                    }

                    _context.Add(phieuNhap);
                    await _context.SaveChangesAsync();

                    // Cập nhật tồn kho
                    foreach (var detail in chiTietPhieuNhaps)
                    {
                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(t => t.MaThuoc == detail.MaThuoc && t.Thang == currentMonth && t.Nam == currentYear);

                        if (tonKho != null)
                        {
                            tonKho.TongNhap += detail.SoLuongNhap;
                            tonKho.HuHong += detail.HuHong ?? 0;
                            tonKho.TonCuoi = (tonKho.TonDau ?? 0) + tonKho.TongNhap - (tonKho.TongXuat ?? 0) - tonKho.HuHong;
                        }
                        else
                        {
                            tonKho = new TonKho
                            {
                                MaThuoc = detail.MaThuoc,
                                Thang = currentMonth,
                                Nam = currentYear,
                                TonDau = 0,
                                TongNhap = detail.SoLuongNhap,
                                TongXuat = 0,
                                HuHong = detail.HuHong ?? 0,
                                TonCuoi = detail.SoLuongNhap - (detail.HuHong ?? 0)
                            };
                            _context.TonKhos.Add(tonKho);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo phiếu nhập.");
                }
            }

            ViewBag.Thuocs = _context.Thuocs.ToList();
            ViewBag.NhaCungCaps = _context.NhaCungCaps.ToList();
            return View(model);
        }



        [HttpGet]
        [Route("Dashboard/NhapKho/Edit/{soPhieu}")]
        public async Task<IActionResult> Edit(int soPhieu)
        {

            var phieuNhap = await _context.PhieuNhaps
                .Where(pn => pn.SoPhieu == soPhieu)
                .Select(pn => new
                {
                    pn.SoPhieu,
                    pn.SoChungTu,
                    NgayNhap = pn.NgayNhap,
                    pn.LyDoNhap,
                    pn.MaNhaCungCap,
                    Medicines = pn.ChiTietPhieuNhaps.Select(ct => new
                    {
                        ct.MaThuoc,
                        TenThuoc = _context.Thuocs
                            .Where(t => t.MaThuoc == ct.MaThuoc)
                            .Select(t => t.TenThuoc)
                            .FirstOrDefault(),
                        ct.SoLo,
                        HanSuDung = ct.HanSuDung,
                        ct.SoLuongNhap,
                        ct.DonGia,
                        ct.HuHong,
                        DonViTinh = _context.Thuocs
                            .Where(t => t.MaThuoc == ct.MaThuoc)
                            .Select(t => t.DonViTinh)
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (phieuNhap == null)
            {
                return NotFound();
            }

            ViewBag.Thuocs = await _context.Thuocs
                .Select(t => new { t.MaThuoc, t.TenThuoc, t.DonViTinh })
                .ToListAsync();

            return Json(phieuNhap);
        }
        [HttpGet]
        [Route("Dashboard/NhapKho/GetMedicines")]
        public async Task<IActionResult> GetMedicines()
        {
            var medicines = await _context.Thuocs
                .Select(t => new { t.MaThuoc, t.TenThuoc, t.DonViTinh })
                .ToListAsync();

            return Json(medicines);
        }



        [HttpPost]
        [Route("Dashboard/NhapKho/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PhieuNhapViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
           

           

            var phieuNhap = await _context.PhieuNhaps
                .Include(pn => pn.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(pn => pn.SoPhieu == model.SoPhieu);

            if (phieuNhap == null)
            {
                return NotFound();
            }

            var originalDetails = phieuNhap.ChiTietPhieuNhaps.ToList();

            phieuNhap.SoChungTu = model.SoChungTu;
            phieuNhap.NgayNhap = DateOnly.FromDateTime(model.NgayNhap);
            phieuNhap.LyDoNhap = model.LyDoNhap;
            phieuNhap.MaNhaCungCap = model.MaNhaCungCap;

            phieuNhap.ChiTietPhieuNhaps.Clear();
            foreach (var medicine in model.Medicines)
            {
                phieuNhap.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                {
                    MaThuoc = medicine.MaThuoc,
                    SoLo = medicine.SoLo,
                    HanSuDung = medicine.HanSuDung.HasValue ? DateOnly.FromDateTime(medicine.HanSuDung.Value) : (DateOnly?)null,
                    SoLuongNhap = medicine.SoLuongNhap,
                    DonGia = medicine.DonGia,
                    HuHong = 0
                });
            }

            try
            {
                int month = model.NgayNhap.Month;
                int year = model.NgayNhap.Year;

                foreach (var original in originalDetails)
                {
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.MaThuoc == original.MaThuoc && t.Thang == month && t.Nam == year);

                    if (tonKho != null)
                    {
                        tonKho.TongNhap -= original.SoLuongNhap;
                        tonKho.HuHong -= original.HuHong ?? 0;
                        tonKho.TonCuoi = (tonKho.TonDau ?? 0) + tonKho.TongNhap - (tonKho.TongXuat ?? 0) - tonKho.HuHong;
                    }
                }

                foreach (var medicine in model.Medicines)
                {
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.MaThuoc == medicine.MaThuoc && t.Thang == month && t.Nam == year);

                    if (tonKho != null)
                    {
                        tonKho.TongNhap += medicine.SoLuongNhap;
                        tonKho.HuHong += medicine.HuHong ?? 0;
                        tonKho.TonCuoi = (tonKho.TonDau ?? 0) + tonKho.TongNhap - (tonKho.TongXuat ?? 0) - tonKho.HuHong;
                    }
                    else
                    {
                        tonKho = new TonKho
                        {
                            MaThuoc = medicine.MaThuoc,
                            Thang = month,
                            Nam = year,
                            TonDau = 0,
                            TongNhap = medicine.SoLuongNhap,
                            TongXuat = 0,
                            HuHong = medicine.HuHong ?? 0,
                            TonCuoi = medicine.SoLuongNhap - (medicine.HuHong ?? 0)
                        };
                        _context.TonKhos.Add(tonKho);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }


        [HttpPost]
        [Route("Dashboard/NhapKho/Delete/{soPhieu}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int soPhieu)
        {
            var phieuNhap = await _context.PhieuNhaps
                .Include(pn => pn.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(pn => pn.SoPhieu == soPhieu);

            if (phieuNhap == null)
            {
                return Json(new { success = false, message = "Phiếu nhập không tồn tại." });
            }

            // Lấy danh sách các lô thuốc trong phiếu nhập
            var soLoList = phieuNhap.ChiTietPhieuNhaps.Select(ctpn => ctpn.SoLo).ToList();

            // Kiểm tra nếu có bất kỳ lô thuốc nào đã được xuất
            bool hasRelatedEntries = await _context.ChiTietPhieuXuats
                .AnyAsync(ctpx => soLoList.Contains(ctpx.SoLo));

            if (hasRelatedEntries)
            {
                return Json(new { success = false, message = "Không thể xóa phiếu nhập vì một số lô thuốc đã được xuất." });
            }

            try
            {
                // Cập nhật lại tồn kho trước khi xóa phiếu nhập
                foreach (var chiTietPhieuNhap in phieuNhap.ChiTietPhieuNhaps)
                {
                    var maThuoc = chiTietPhieuNhap.MaThuoc;
                    var soLuongNhap = chiTietPhieuNhap.SoLuongNhap;
                    var thangNhap = phieuNhap.NgayNhap.Month;
                    var namNhap = phieuNhap.NgayNhap.Year;

                    // Lấy bản ghi tồn kho của thuốc tương ứng theo tháng và năm của phiếu nhập
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(tk => tk.MaThuoc == maThuoc && tk.Thang == thangNhap && tk.Nam == namNhap);

                    if (tonKho != null)
                    {
                        // Cập nhật lại số lượng nhập và tồn cuối
                        tonKho.TongNhap -= soLuongNhap;

                        // Đảm bảo số lượng không bị âm
                        if (tonKho.TongNhap < 0)
                        {
                            tonKho.TongNhap = 0;
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

                // Xóa các chi tiết phiếu nhập
                _context.ChiTietPhieuNhaps.RemoveRange(phieuNhap.ChiTietPhieuNhaps);

                // Xóa phiếu nhập
                _context.PhieuNhaps.Remove(phieuNhap);

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa phiếu nhập và điều chỉnh tồn kho thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phiếu nhập." });
            }
        }


        [HttpGet]
        [Route("Dashboard/NhapKho/ExportExcel")]
        public IActionResult ExportExcel()
        {
            var phieuNhaps = _context.PhieuNhaps
                .Select(pn => new
                {
                    pn.SoPhieu,
                    pn.SoChungTu,
                    pn.NgayNhap,
                    Thang = pn.NgayNhap.Month,
                    Nam = pn.NgayNhap.Year,
                    NhaCungCap = pn.MaNhaCungCapNavigation.TenNhaCungCap,
                    DiaChiNCC = pn.MaNhaCungCapNavigation.DiaChi,
                    NguoiNhap = pn.MaTaiKhoanNavigation.HoTen,
                    ChiTiet = pn.ChiTietPhieuNhaps.Select(ct => new
                    {
                        ct.MaThuoc,
                        Thuoc = ct.MaThuocNavigation.TenThuoc,
                        ct.SoLo,
                        ct.HanSuDung,
                        ct.SoLuongNhap,
                        ct.DonGia,
                        ThanhTien = ct.SoLuongNhap * ct.DonGia
                    }).ToList()
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("NhapKho");

                // Tạo header
                worksheet.Cell(1, 1).Value = "Tháng";
                worksheet.Cell(1, 2).Value = "Năm";
                worksheet.Cell(1, 3).Value = "Số Phiếu";
                worksheet.Cell(1, 4).Value = "Số Chứng Từ";
                worksheet.Cell(1, 5).Value = "Ngày Nhập";
                worksheet.Cell(1, 6).Value = "Nhà Cung Cấp";
                worksheet.Cell(1, 7).Value = "Địa Chỉ NCC";
                worksheet.Cell(1, 8).Value = "Người Nhập";
                worksheet.Cell(1, 9).Value = "Mã Thuốc";
                worksheet.Cell(1, 10).Value = "Tên Thuốc";
                worksheet.Cell(1, 11).Value = "Số Lô";
                worksheet.Cell(1, 12).Value = "Hạn Sử Dụng";
                worksheet.Cell(1, 13).Value = "Số Lượng Nhập";
                worksheet.Cell(1, 14).Value = "Đơn Giá";
                worksheet.Cell(1, 15).Value = "Thành Tiền";

                int row = 2;

                // Nhóm dữ liệu theo tháng và năm
                var groupedPhieuNhaps = phieuNhaps
                    .GroupBy(pn => new { pn.Thang, pn.Nam })
                    .OrderBy(g => g.Key.Nam)
                    .ThenBy(g => g.Key.Thang);

                // Thêm dữ liệu vào file
                foreach (var group in groupedPhieuNhaps)
                {
                    foreach (var pn in group)
                    {
                        foreach (var ct in pn.ChiTiet)
                        {
                            worksheet.Cell(row, 1).Value = group.Key.Thang;
                            worksheet.Cell(row, 2).Value = group.Key.Nam;
                            worksheet.Cell(row, 3).Value = pn.SoPhieu;
                            worksheet.Cell(row, 4).Value = pn.SoChungTu;
                            worksheet.Cell(row, 5).Value = pn.NgayNhap.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 6).Value = pn.NhaCungCap;
                            worksheet.Cell(row, 7).Value = pn.DiaChiNCC;
                            worksheet.Cell(row, 8).Value = pn.NguoiNhap;
                            worksheet.Cell(row, 9).Value = ct.MaThuoc;
                            worksheet.Cell(row, 10).Value = ct.Thuoc;
                            worksheet.Cell(row, 11).Value = ct.SoLo;
                            worksheet.Cell(row, 12).Value = ct.HanSuDung?.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 13).Value = ct.SoLuongNhap;
                            worksheet.Cell(row, 14).Value = ct.DonGia?.ToString("C0", new CultureInfo("vi-VN"));
                            worksheet.Cell(row, 15).Value = ct.ThanhTien?.ToString("C0", new CultureInfo("vi-VN"));
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
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoNhapKhoTheoThang.xlsx");
                }
            }
        }





    }
}