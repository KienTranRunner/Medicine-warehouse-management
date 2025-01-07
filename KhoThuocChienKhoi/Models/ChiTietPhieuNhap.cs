using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class ChiTietPhieuNhap
{
    public int SoPhieu { get; set; }

    public int MaThuoc { get; set; }

    public string? SoLo { get; set; }

    public DateOnly? HanSuDung { get; set; }

    public int? SoLuongNhap { get; set; }

    public decimal? DonGia { get; set; }

    public int? SoLuongXuat { get; set; }

    public int? HuHong { get; set; }

    public virtual Thuoc MaThuocNavigation { get; set; } = null!;

    public virtual PhieuNhap SoPhieuNavigation { get; set; } = null!;
}
