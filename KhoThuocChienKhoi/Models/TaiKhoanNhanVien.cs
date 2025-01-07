using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class TaiKhoanNhanVien
{
    public int MaTaiKhoan { get; set; }

    public int MaNhanVien { get; set; }

    public string MatKhau { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? VaiTro { get; set; }

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
}
