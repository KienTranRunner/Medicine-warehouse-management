using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class ChiTietPhieuXuat
{
    public int SoPhieu { get; set; }

    public int MaThuoc { get; set; }

    public string? SoLo { get; set; }

    public int? SoLuongXuat { get; set; }

    public decimal? DonGia { get; set; }

    public virtual Thuoc MaThuocNavigation { get; set; } = null!;

    public virtual PhieuXuat SoPhieuNavigation { get; set; } = null!;
}
