using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class PhieuNhap
{
    public int SoPhieu { get; set; }

    public string SoChungTu { get; set; } = null!;

    public DateOnly NgayNhap { get; set; }

    public string? LyDoNhap { get; set; }

    public int MaNhaCungCap { get; set; }

    public int? MaTaiKhoan { get; set; }

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual NhaCungCap MaNhaCungCapNavigation { get; set; } = null!;

    public virtual TaiKhoanNhanVien? MaTaiKhoanNavigation { get; set; }
}
