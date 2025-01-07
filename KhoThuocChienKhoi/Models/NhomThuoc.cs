using System;
using System.Collections.Generic;

namespace KhoThuocChienKhoi.Models;

public partial class NhomThuoc
{
    public int MaNhom { get; set; }

    public string TenNhom { get; set; } = null!;

    public virtual ICollection<Thuoc> Thuocs { get; set; } = new List<Thuoc>();
}
