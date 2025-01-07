using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class PhieuXuat
{
    public int SoPhieu { get; set; }

    public string SoChungTu { get; set; } = null!;

    public DateOnly NgayXuat { get; set; }

    public string? LyDoXuat { get; set; }

    public int? MaKhachHang { get; set; }

    public int? MaTaiKhoan { get; set; }

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual TaiKhoanNhanVien? MaTaiKhoanNavigation { get; set; }
}
