using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KhoThuocChienKhoi.Models
{
    public class ChiTietPhieuNhapViewModel
    {

        [Required]
        public int MaThuoc { get; set; }
        public string? TenThuoc { get; set; }

        public string? SoLo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? HanSuDung { get; set; }

        [Required]
        public int SoLuongNhap { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuongNhap * DonGia;

        public string? DonViTinh { get; set; }

        public int? HuHong { get; set; }



    }
}