using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KhoThuocChienKhoi.Models
{
    public class PhieuXuatViewModel
    {
        public int? SoPhieu { get; set; }

        [Required]
        public string SoChungTu { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayXuat { get; set; }

        public string? LyDoXuat { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        public int? MaTaiKhoan { get; set; }

        public List<ChiTietPhieuXuatViewModel> Medicines { get; set; } = new List<ChiTietPhieuXuatViewModel>();

    }
}