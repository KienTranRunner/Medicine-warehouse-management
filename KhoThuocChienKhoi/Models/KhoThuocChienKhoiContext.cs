using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Models;

public partial class KhoThuocChienKhoiContext : DbContext
{
    public KhoThuocChienKhoiContext()
    {
    }

    public KhoThuocChienKhoiContext(DbContextOptions<KhoThuocChienKhoiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }

    public virtual DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhomThuoc> NhomThuocs { get; set; }

    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }

    public virtual DbSet<PhieuXuat> PhieuXuats { get; set; }

    public virtual DbSet<TaiKhoanNhanVien> TaiKhoanNhanViens { get; set; }

    public virtual DbSet<Thuoc> Thuocs { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=KhoThuocChienKhoi;User Id=SA;Password=MyStrongPass123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => new { e.SoPhieu, e.MaThuoc }).HasName("PK__ChiTietP__82B1B18065E2D69C");

            entity.ToTable("ChiTietPhieuNhap");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HuHong).HasDefaultValue(0);
            entity.Property(e => e.SoLo).HasMaxLength(50);
            entity.Property(e => e.SoLuongXuat).HasDefaultValue(0);

            entity.HasOne(d => d.MaThuocNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaThuoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPh__MaThu__46E78A0C");

            entity.HasOne(d => d.SoPhieuNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.SoPhieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPh__SoPhi__45F365D3");
        });

        modelBuilder.Entity<ChiTietPhieuXuat>(entity =>
        {
            entity.HasKey(e => new { e.SoPhieu, e.MaThuoc }).HasName("PK__ChiTietP__82B1B180F959F72A");

            entity.ToTable("ChiTietPhieuXuat");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLo).HasMaxLength(50);

            entity.HasOne(d => d.MaThuocNavigation).WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(d => d.MaThuoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPh__MaThu__4E88ABD4");

            entity.HasOne(d => d.SoPhieuNavigation).WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(d => d.SoPhieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPh__SoPhi__4D94879B");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E5C98DBC88");

            entity.ToTable("KhachHang");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenKhachHang).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA9205F8D6C4DF");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenNhaCungCap).HasMaxLength(100);
        });

        modelBuilder.Entity<NhomThuoc>(entity =>
        {
            entity.HasKey(e => e.MaNhom).HasName("PK__NhomThuo__234F91CD0BE97EA1");

            entity.ToTable("NhomThuoc");

            entity.Property(e => e.TenNhom).HasMaxLength(100);
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.SoPhieu).HasName("PK__PhieuNha__960AAEE2E713322C");

            entity.ToTable("PhieuNhap");

            entity.Property(e => e.LyDoNhap).HasMaxLength(255);
            entity.Property(e => e.SoChungTu).HasMaxLength(50);

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.MaNhaCungCap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuNhap__MaNha__4222D4EF");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__PhieuNhap__MaTai__4316F928");
        });

        modelBuilder.Entity<PhieuXuat>(entity =>
        {
            entity.HasKey(e => e.SoPhieu).HasName("PK__PhieuXua__960AAEE2EAF1FED1");

            entity.ToTable("PhieuXuat");

            entity.Property(e => e.LyDoXuat).HasMaxLength(255);
            entity.Property(e => e.SoChungTu).HasMaxLength(50);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.PhieuXuats)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__PhieuXuat__MaKha__49C3F6B7");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.PhieuXuats)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__PhieuXuat__MaTai__4AB81AF0");
        });

        modelBuilder.Entity<TaiKhoanNhanVien>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C652927AD51E0");

            entity.ToTable("TaiKhoanNhanVien");

            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<Thuoc>(entity =>
        {
            entity.HasKey(e => e.MaThuoc).HasName("PK__Thuoc__4BB1F6204D4C5EFA");

            entity.ToTable("Thuoc");

            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HamLuong).HasMaxLength(100);
            entity.Property(e => e.HoatChat).HasMaxLength(100);
            entity.Property(e => e.TenThuoc).HasMaxLength(100);

            entity.HasOne(d => d.MaNhomNavigation).WithMany(p => p.Thuocs)
                .HasForeignKey(d => d.MaNhom)
                .HasConstraintName("FK__Thuoc__MaNhom__398D8EEE");
        });

        modelBuilder.Entity<TonKho>(entity =>
        {
            entity.HasKey(e => e.MaTon).HasName("PK__TonKho__31493479AA99AB5D");

            entity.ToTable("TonKho");

            entity.HasOne(d => d.MaThuocNavigation).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.MaThuoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TonKho__MaThuoc__5165187F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
