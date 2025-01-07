# Medicine warehouse management
 ASP.NET Core MVC
 
Script Sql Server:


CREATE TABLE NhomThuoc
(
    MaNhom INT IDENTITY(1,1) PRIMARY KEY,
    TenNhom NVARCHAR(100) NOT NULL
);

CREATE TABLE Thuoc
(
    MaThuoc INT IDENTITY(1,1) PRIMARY KEY,
    TenThuoc NVARCHAR(100) NOT NULL,
    HamLuong NVARCHAR(100),
    HoatChat NVARCHAR(100),
    DonViTinh NVARCHAR(50),
    Gia DECIMAL(18, 2),
    MaNhom INT FOREIGN KEY REFERENCES NhomThuoc(MaNhom)
);

CREATE TABLE NhaCungCap
(
    MaNhaCungCap INT IDENTITY(1,1) PRIMARY KEY,
    TenNhaCungCap NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(15)
);

CREATE TABLE TaiKhoanNhanVien
(
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    MaNhanVien INT NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100),
    VaiTro NVARCHAR(50),
);

CREATE TABLE KhachHang
(
    MaKhachHang INT IDENTITY(1,1) PRIMARY KEY,
    TenKhachHang NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(15)
);

CREATE TABLE PhieuNhap
(
    SoPhieu INT IDENTITY(1,1) PRIMARY KEY,
    SoChungTu NVARCHAR(50) NOT NULL,
    NgayNhap DATE NOT NULL,
    LyDoNhap NVARCHAR(255),
    MaNhaCungCap INT NOT NULL FOREIGN KEY REFERENCES NhaCungCap(MaNhaCungCap),
    MaTaiKhoan INT FOREIGN KEY REFERENCES TaiKhoanNhanVien(MaTaiKhoan)
);

CREATE TABLE ChiTietPhieuNhap
(
    SoPhieu INT NOT NULL FOREIGN KEY REFERENCES PhieuNhap(SoPhieu),
    MaThuoc INT NOT NULL FOREIGN KEY REFERENCES Thuoc(MaThuoc),
    SoLo NVARCHAR(50),
    HanSuDung DATE,
    SoLuongNhap INT,
    DonGia DECIMAL(18, 2),
    PRIMARY KEY (SoPhieu, MaThuoc)
);

CREATE TABLE PhieuXuat
(
    SoPhieu INT IDENTITY(1,1) PRIMARY KEY,
    SoChungTu NVARCHAR(50) NOT NULL,
    NgayXuat DATE NOT NULL,
    LyDoXuat NVARCHAR(255),
    MaKhachHang INT FOREIGN KEY REFERENCES KhachHang(MaKhachHang),
    MaTaiKhoan INT FOREIGN KEY REFERENCES TaiKhoanNhanVien(MaTaiKhoan)
);

CREATE TABLE ChiTietPhieuXuat
(
    SoPhieu INT NOT NULL FOREIGN KEY REFERENCES PhieuXuat(SoPhieu),
    MaThuoc INT NOT NULL FOREIGN KEY REFERENCES Thuoc(MaThuoc),
    SoLo NVARCHAR(50),
    SoLuongXuat INT,
    DonGia DECIMAL(18, 2),
    PRIMARY KEY (SoPhieu, MaThuoc)
);

CREATE TABLE TonKho
(
    MaTon INT IDENTITY(1,1) PRIMARY KEY,
    Thang INT NOT NULL,
    Nam INT NOT NULL,
    TonDau INT,
    TongNhap INT,
    TongXuat INT,
    HuHong INT,
    TonCuoi INT,
    MaThuoc INT NOT NULL FOREIGN KEY REFERENCES Thuoc(MaThuoc)
);
ALTER TABLE ChiTietPhieuNhap
ADD SoLuongXuat INT DEFAULT 0;
ALTER TABLE ChiTietPhieuNhap
ADD HuHong INT DEFAULT 0;
ALTER TABLE TonKho
ADD SoLuongThucTe INT NULL,  
    ChenhLech INT NULL;


Class Diagram:
![image](https://github.com/user-attachments/assets/e0dc465c-90f7-4443-bec7-5e5ed0005020)


Any Image in project

Dashboard:
![image](https://github.com/user-attachments/assets/13cdeb0c-5bde-419e-a586-b1d1ea2973dc)


Login Form:
![image](https://github.com/user-attachments/assets/16049af2-bab2-4b81-a32d-f17af4b956af)




