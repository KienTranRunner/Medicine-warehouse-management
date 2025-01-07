using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using KhoThuocChienKhoi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace KhoThuocChienKhoi.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class KhachHangController : Controller
    {

        private readonly KhoThuocChienKhoiContext _context;

        public KhachHangController(KhoThuocChienKhoiContext context)
        {
            _context = context;
        }

        [Route("Dashboard/KhachHang")]
        [HttpGet]
        public IActionResult Index()
        {
            var khachHangs = _context.KhachHangs
        .Select(kh => new
        {
            kh.MaKhachHang,
            kh.TenKhachHang,
            kh.DiaChi,
            kh.SoDienThoai,
            SoTienKhachMua = _context.PhieuXuats
                .Where(px => px.MaKhachHang == kh.MaKhachHang)
                .SelectMany(px => px.ChiTietPhieuXuats)
                .Sum(ctpx => (ctpx.SoLuongXuat ?? 0) * (ctpx.DonGia ?? 0))
        })
        .ToList();

            return View(khachHangs);
        }


        [Route("Dashboard/KhachHang/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateKhachHang(KhachHang model)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.KhachHangs.Add(model);
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

        [Route("Dashboard/KhachHang/Edit/{id}")]
        [HttpGet]
        public IActionResult EditKhachHang(int id)
        {
            var khachhang = _context.KhachHangs
                .Where(kh => kh.MaKhachHang == id)
                .Select(kh => new
                {
                    MaKhachHang = kh.MaKhachHang,
                    TenKhachHang = kh.TenKhachHang,
                    DiaChi = kh.DiaChi,
                    SoDienThoai = kh.SoDienThoai
                })
                .FirstOrDefault();

            if (khachhang == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp." });
            }

            return Json(khachhang);
        }

        [Route("Dashboard/KhachHang/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditKhachHang(int id, KhachHang model)
        {
            if (id != model.MaKhachHang)
            {
                return BadRequest(new { message = "Mã nhóm thuốc không khớp." });
            }
            try
            {
                var khachhang = _context.KhachHangs.Find(id);
                if (khachhang != null)
                {
                    khachhang.MaKhachHang = model.MaKhachHang;
                    khachhang.TenKhachHang = model.TenKhachHang;
                    khachhang.DiaChi = model.DiaChi;
                    khachhang.SoDienThoai = model.SoDienThoai;
                    _context.KhachHangs.Update(khachhang);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(new { message = "Khách hàng không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [Route("Dashboard/KhachHang/Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteKhachHang(int id)
        {
            try
            {
                var kh = _context.KhachHangs.Find(id);
                if (kh == null)
                {
                    return Json(new { success = false, message = "Nhà cung cấp không tồn tại." });
                }

                // Kiểm tra xem nhà cung cấp có liên kết khóa ngoại hay không
                bool hasRelatedEntries = _context.PhieuXuats.Any(p => p.MaKhachHang == id);


                if (hasRelatedEntries)
                {
                    return Json(new { success = false, message = "Không thể xóa khách hàng này vì đang có dữ liệu liên kết." });
                }

                // Thực hiện xóa nếu không có liên kết
                _context.KhachHangs.Remove(kh);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xoá khách hàng" });
            }
        }



        [HttpGet]
        [Route("Dashboard/KhachHang/ExportExcel")]
        public IActionResult ExportExcel()
        {
            var khachHangs = _context.KhachHangs
                .Select(kh => new
                {
                    kh.MaKhachHang,
                    kh.TenKhachHang,
                    kh.DiaChi,
                    kh.SoDienThoai,
                    SoTienKhachMua = _context.PhieuXuats
                        .Where(px => px.MaKhachHang == kh.MaKhachHang)
                        .SelectMany(px => px.ChiTietPhieuXuats)
                        .Sum(ctpx => (ctpx.SoLuongXuat ?? 0) * (ctpx.DonGia ?? 0))
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("KhachHang");

                // Tạo header
                worksheet.Cell(1, 1).Value = "Mã Khách Hàng";
                worksheet.Cell(1, 2).Value = "Tên Khách Hàng";
                worksheet.Cell(1, 3).Value = "Địa Chỉ";
                worksheet.Cell(1, 4).Value = "Số Điện Thoại";
                worksheet.Cell(1, 5).Value = "Số Tiền Khách Mua";

                // Thêm dữ liệu vào file
                for (int i = 0; i < khachHangs.Count; i++)
                {
                    var kh = khachHangs[i];
                    worksheet.Cell(i + 2, 1).Value = kh.MaKhachHang;
                    worksheet.Cell(i + 2, 2).Value = kh.TenKhachHang;
                    worksheet.Cell(i + 2, 3).Value = kh.DiaChi;
                    worksheet.Cell(i + 2, 4).Value = kh.SoDienThoai;
                    worksheet.Cell(i + 2, 5).Value = kh.SoTienKhachMua.ToString("C0", new CultureInfo("vi-VN"));
                }

                // Thiết lập định dạng cho bảng Excel
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachKhachHang.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _context.KhachHangs
                .Where(tk => tk.TenKhachHang.ToString().Contains(query) || tk.DiaChi.Contains(query) || tk.SoDienThoai.Contains(query))
                .Select(tk => new
                {
                    MaKhachHang = tk.MaKhachHang,
                    TenKhachHang = tk.TenKhachHang,
                    DiaChi = tk.DiaChi,
                    SoDienThoai = tk.SoDienThoai,
                    SoTienKhachMua = _context.PhieuXuats
                .Where(px => px.MaKhachHang == tk.MaKhachHang)
                .SelectMany(px => px.ChiTietPhieuXuats)
                .Sum(ctpx => (ctpx.SoLuongXuat ?? 0) * (ctpx.DonGia ?? 0))
                })
                .ToList();

            return PartialView("_KhachHangTableRows", results);
        }





    }
}