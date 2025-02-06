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

SignIn:
![image](https://github.com/user-attachments/assets/b26139ad-3c56-44b2-a025-84a39bc96648)



Dashboard:
![image](https://github.com/user-attachments/assets/13cdeb0c-5bde-419e-a586-b1d1ea2973dc)





User Management:
![image](https://github.com/user-attachments/assets/05f34bb6-f9ed-4023-b0e2-d840e997f2db)



Customer Management:
![image](https://github.com/user-attachments/assets/8e6e8c8b-6b96-42a9-8e89-baade8000f1c)




Medicine Management:
![image](https://github.com/user-attachments/assets/85185c7f-8091-4b07-927a-be3d9330738b)



Import Medicine Management:
![image](https://github.com/user-attachments/assets/d89d3f33-9714-42b8-873b-2d535563b4ce)




Detail Import Medicine:
![image](https://github.com/user-attachments/assets/bec0e607-e903-4743-8d8d-0d1da82f6d9e)




Export Medicine Management:
![image](https://github.com/user-attachments/assets/dac60ec9-4459-416e-9785-2af18a8ad485)



Detail Export Management:
![image](https://github.com/user-attachments/assets/a5aee3a4-cb19-4909-a007-6be2fbe79c22)





Check inventory in stock:
![image](https://github.com/user-attachments/assets/bac38c20-5978-4313-bbd6-d961d4dd134a)




Check medicine inventory:
![image](https://github.com/user-attachments/assets/987d9fe9-b3e7-4890-9034-ae0f827b56c5)



Manage expired medicine inventory:
<img width="1440" alt="Screenshot 2025-02-06 at 07 07 25" src="https://github.com/user-attachments/assets/0b769103-ffa1-4591-bf09-fc72cfbceb3a" />



