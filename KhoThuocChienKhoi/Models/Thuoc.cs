using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class Thuoc
{
    public int MaThuoc { get; set; }

    public string TenThuoc { get; set; } = null!;

    public string? HamLuong { get; set; }

    public string? HoatChat { get; set; }

    public string? DonViTinh { get; set; }

    public decimal? Gia { get; set; }

    public int? MaNhom { get; set; }

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual NhomThuoc? MaNhomNavigation { get; set; }

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
