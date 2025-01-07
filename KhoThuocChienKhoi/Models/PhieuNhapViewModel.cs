using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KhoThuocChienKhoi.Models
{
    public class PhieuNhapViewModel
    {
        public int? SoPhieu { get; set; }

        [Required]
        public string SoChungTu { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayNhap { get; set; }

        public string? LyDoNhap { get; set; }

        [Required]
        public int MaNhaCungCap { get; set; }

        public int? MaTaiKhoan { get; set; }

        public List<ChiTietPhieuNhapViewModel> Medicines { get; set; } = new List<ChiTietPhieuNhapViewModel>();

    }
}