using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KhoThuocChienKhoi.Models
{
    public class ChiTietPhieuXuatViewModel
    {
        [Required]
        public int MaThuoc { get; set; }

        public string? TenThuoc { get; set; }


        public string? SoLo { get; set; }

        [Required]
        public int SoLuongXuat { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuongXuat * DonGia;

        public string? DonViTinh { get; set; }

    }
}