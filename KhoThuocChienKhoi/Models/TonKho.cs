using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class TonKho
{
    public int MaTon { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public int? TonDau { get; set; }

    public int? TongNhap { get; set; }

    public int? TongXuat { get; set; }

    public int? HuHong { get; set; }

    public int? TonCuoi { get; set; }

    public int MaThuoc { get; set; }

    public int? SoLuongThucTe { get; set; }

    public int? ChenhLech { get; set; }

    public virtual Thuoc MaThuocNavigation { get; set; } = null!;
}
