use QuanLyPhongKham
go
IF DB_ID('QuanLyPhongMach') IS NOT NULL
    DROP DATABASE QuanLyPhongMach;
GO

CREATE DATABASE QuanLyPhongKham;
GO
USE QuanLyPhongKham;
GO

/*========================================================
  1. TABLES
========================================================*/

-- 1.1 KHOA
CREATE TABLE Khoa
(
    sttKhoa INT IDENTITY(1,1) PRIMARY KEY,
    TenKhoa NVARCHAR(100) NOT NULL
);
GO

-- 1.2 BÁC SĨ
CREATE TABLE BacSi
(
    sttBS    INT IDENTITY(1,1) PRIMARY KEY,
    tenBS    NVARCHAR(100) NOT NULL,
    username VARCHAR(50)   NOT NULL UNIQUE,
    [password] VARCHAR(50) NOT NULL,
    DiaChi   NVARCHAR(200) NULL,
    DienThoai VARCHAR(20)  NULL,
    sttKhoa  INT NULL
        CONSTRAINT FK_BacSi_Khoa REFERENCES Khoa(sttKhoa)
);
GO

-- 1.3 BỆNH NHÂN
CREATE TABLE BenhNhan
(
    sttBN      INT PRIMARY KEY,          -- sinh bằng hàm fn_NextBenhNhanId
    hoten      NVARCHAR(50) NOT NULL,
    CMND       INT         NOT NULL,
    diachi     NVARCHAR(100) NULL,
    gioitinh   NVARCHAR(3)  NULL,
    ngaykham   DATETIME     NOT NULL,
    BSphutrach INT          NOT NULL
        CONSTRAINT FK_BenhNhan_BacSi REFERENCES BacSi(sttBS),
    sttKhoa    INT NULL
        CONSTRAINT FK_BenhNhan_Khoa REFERENCES Khoa(sttKhoa)
);
GO

-- 1.4 BỆNH NHÂN THƯỜNG
CREATE TABLE BenhNhanThuong
(
    sttBNT INT IDENTITY(1,1) PRIMARY KEY,
    sttBN  INT NOT NULL
        CONSTRAINT FK_BenhNhanThuong_BenhNhan REFERENCES BenhNhan(sttBN)
);
GO

-- 1.5 BỆNH NHÂN NẰM GIƯỜNG CHƯA XẾP CHỖ
CREATE TABLE BenhNhanNamGiuong_ChuaXepCho
(
    sttBNNG_CXC INT IDENTITY(1,1) PRIMARY KEY,
    sttBN       INT NOT NULL
        CONSTRAINT FK_BNNG_CXC_BenhNhan REFERENCES BenhNhan(sttBN)
);
GO
--Loại thuốc---
CREATE TABLE dbo.LoaiThuoc
(
    MaLoaiThuoc  INT IDENTITY(1,1) CONSTRAINT PK_LoaiThuoc PRIMARY KEY,
    TenLoai NVARCHAR(200) NOT NULL,
    MoTa    NVARCHAR(500) NULL,
    DonGia  INT NOT NULL
);
GO

-- 1.6 GIƯỜNG BỆNH
CREATE TABLE GiuongBenh
(
    sttGB     INT IDENTITY(1,1) PRIMARY KEY,
    TenGiuong NVARCHAR(50) NOT NULL,
    tinhtrang NVARCHAR(20) NOT NULL,
    sttKhoa   INT NULL
        CONSTRAINT FK_GiuongBenh_Khoa REFERENCES Khoa(sttKhoa),
    CONSTRAINT UQ_GiuongBenh_TenGiuong UNIQUE (TenGiuong)
);
GO

-- 1.7 BỆNH NHÂN NẰM GIƯỜNG ĐÃ XẾP CHỖ
CREATE TABLE BenhNhanNamGiuong_DaXepCho
(
    sttBNNG_DXC INT IDENTITY(1,1) PRIMARY KEY,
    sttBN       INT NOT NULL
        CONSTRAINT FK_BNNG_DXC_BenhNhan REFERENCES BenhNhan(sttBN),
    sttGB       INT NOT NULL
        CONSTRAINT FK_BNNG_DXC_GiuongBenh REFERENCES GiuongBenh(sttGB),
    ngayxep     DATETIME NOT NULL
);
GO

-- 1.8 ĐƠN THUỐC (TỰ TĂNG sttDT)
CREATE TABLE DonThuoc
(
    sttDT        INT IDENTITY(1,1) PRIMARY KEY,
    TenThuoc     NVARCHAR(300) NOT NULL,
    NgayCapThuoc DATETIME      NOT NULL DEFAULT GETDATE(),
    TienThuoc    INT           NOT NULL
);
GO

-- 1.9 ĐƠN THUỐC – BỆNH NHÂN THƯỜNG
CREATE TABLE DonThuoc_BenhNhanThuong
(
    sttDT_BNT        INT IDENTITY(1,1) PRIMARY KEY,
    sttDT            INT NOT NULL
        CONSTRAINT FK_DT_BNT_DonThuoc REFERENCES DonThuoc(sttDT),
    MaBenhNhanThuong INT NOT NULL
        CONSTRAINT FK_DT_BNT_BenhNhanThuong REFERENCES BenhNhanThuong(sttBNT)
);
GO

-- 1.10 ĐƠN THUỐC – BỆNH NHÂN NẰM GIƯỜNG ĐÃ XẾP CHỖ
CREATE TABLE DonThuoc_BenhNhanNamGiuong_DaXepCho
(
    sttDT_BNNG_DXC INT IDENTITY(1,1) PRIMARY KEY,
    sttDT          INT NOT NULL
        CONSTRAINT FK_DT_BNNG_DonThuoc REFERENCES DonThuoc(sttDT),
    MaBenhNhanNamGiuong_DaXepCho INT NOT NULL
        CONSTRAINT FK_DT_BNNG_BNNG_DXC REFERENCES BenhNhanNamGiuong_DaXepCho(sttBNNG_DXC),
    TinhTrang NVARCHAR(20) NOT NULL DEFAULT N'Chưa thanh toán'
);
GO

-- 1.11 THAM SỐ
CREATE TABLE ThamSo
(
    sttTS            INT IDENTITY(1,1) PRIMARY KEY,
    Tien1Ngay1Giuong FLOAT NOT NULL
);
GO

-- 1.12 HÓA ĐƠN
CREATE TABLE HoaDon
(
    sttHD       INT IDENTITY(1,1) PRIMARY KEY,
    sttBNNG_DXC INT NOT NULL
        CONSTRAINT FK_HoaDon_BNNG_DXC REFERENCES BenhNhanNamGiuong_DaXepCho(sttBNNG_DXC),
    ThanhTien   INT NOT NULL
);
GO

/*========================================================
  2. FUNCTIONS
========================================================*/

-- Hàm sinh mã bệnh nhân: YYYYxx (vd 202501, 202502,...)
CREATE OR ALTER FUNCTION dbo.fn_NextBenhNhanId()
RETURNS INT
AS
BEGIN
    DECLARE @year   INT = YEAR(GETDATE());
    DECLARE @prefix INT = @year * 100;      -- 202500
    DECLARE @maxId  INT;

    SELECT @maxId = MAX(sttBN)
    FROM BenhNhan
    WHERE sttBN BETWEEN @prefix + 1 AND @prefix + 99;

    IF @maxId IS NULL SET @maxId = @prefix;

    RETURN @maxId + 1;
END;
GO

/*========================================================
  3. STORED PROCEDURES
========================================================*/

-------------------------
-- 3.1 BÁC SĨ
-------------------------
CREATE OR ALTER PROC sp_InsertBacSi
    @tenBS    NVARCHAR(100),
    @username VARCHAR(50),
    @password VARCHAR(50),
    @DiaChi   NVARCHAR(200),
    @DienThoai VARCHAR(20),
    @sttKhoa  INT = NULL
AS
BEGIN
    INSERT INTO BacSi(tenBS, username, [password], DiaChi, DienThoai, sttKhoa)
    VALUES (@tenBS, @username, @password, @DiaChi, @DienThoai, @sttKhoa);
END;
GO

CREATE OR ALTER PROC sp_SelectAllBacSi
AS
BEGIN
    SET NOCOUNT ON;
    SELECT bs.sttBS,
           bs.tenBS,
           bs.username,
           bs.[password],
           bs.DiaChi,
           bs.DienThoai,
           bs.sttKhoa,
           ISNULL(k.TenKhoa, N'') AS TenKhoa
    FROM BacSi bs
    LEFT JOIN Khoa k ON bs.sttKhoa = k.sttKhoa
    ORDER BY bs.tenBS;
END;
GO

CREATE OR ALTER PROC sp_SelectAllBacSiForTraCuu
AS
BEGIN
    SET NOCOUNT ON;
    SELECT sttBS, tenBS, DiaChi, DienThoai
    FROM BacSi;
END;
GO

CREATE OR ALTER PROC sp_SelectBacSiByUserNameandPassword
    @username VARCHAR(50),
    @password VARCHAR(50)
AS
BEGIN
    SELECT *
    FROM BacSi
    WHERE username=@username AND [password]=@password;
END;
GO

CREATE OR ALTER PROC sp_UpdatePassWord
    @sttBS    INT,
    @password VARCHAR(50)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BacSi WHERE sttBS=@sttBS) RETURN;
    UPDATE BacSi SET [password]=@password WHERE sttBS=@sttBS;
END;
GO

CREATE OR ALTER PROC sp_UpdateThongTinBacSi
    @sttBS    INT,
    @DienThoai VARCHAR(20),
    @DiaChi   NVARCHAR(200),
    @sttKhoa  INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BacSi WHERE sttBS=@sttBS) RETURN;

    UPDATE BacSi
    SET DienThoai = @DienThoai,
        DiaChi    = @DiaChi,
        sttKhoa   = @sttKhoa
    WHERE sttBS = @sttBS;
END;
GO

-------------------------
-- 3.2 KHOA
-------------------------
-- Grid: Mã khoa, Tên khoa, Số bác sĩ, Số bệnh nhân, Số giường
CREATE OR ALTER PROC sp_SelectAllKhoa
AS
BEGIN
    SET NOCOUNT ON;

    SELECT k.sttKhoa       AS N'Mã Khoa',
           k.TenKhoa       AS N'Tên Khoa',
           (SELECT COUNT(*) FROM BacSi     WHERE sttKhoa = k.sttKhoa) AS N'Số Bác sĩ',
           (SELECT COUNT(*) FROM BenhNhan  WHERE sttKhoa = k.sttKhoa) AS N'Số Bệnh nhân',
           (SELECT COUNT(*) FROM GiuongBenh WHERE sttKhoa = k.sttKhoa) AS N'Số Giường'
    FROM Khoa k
    ORDER BY k.sttKhoa;
END;
GO

-- DS bác sĩ / bệnh nhân / giường trong 1 khoa (cho FrmKhoa)
CREATE OR ALTER PROC sp_Khoa_SelectBacSiTrongKhoa
    @MaKhoa INT
AS
BEGIN
    SELECT sttBS, tenBS
    FROM BacSi
    WHERE sttKhoa = @MaKhoa
    ORDER BY tenBS;
END;
GO

CREATE OR ALTER PROC sp_Khoa_SelectBenhNhanTrongKhoa
    @MaKhoa INT
AS
BEGIN
    SELECT sttBN, hoten
    FROM BenhNhan
    WHERE sttKhoa = @MaKhoa
    ORDER BY hoten;
END;
GO

CREATE OR ALTER PROC sp_Khoa_SelectGiuongTrongKhoa
    @MaKhoa INT
AS
BEGIN
    SELECT sttGB, TenGiuong, tinhtrang
    FROM GiuongBenh
    WHERE sttKhoa = @MaKhoa
    ORDER BY TenGiuong;
END;
GO

-------------------------
-- 3.3 BỆNH NHÂN
-------------------------

-- Thêm BN thường: khoa tự lấy theo bác sĩ phụ trách
CREATE OR ALTER PROC sp_InsertBenhNhan_BenhNhanThuong
    @hoten      NVARCHAR(50),
    @CMND       INT,
    @diachi     NVARCHAR(100),
    @gioitinh   NVARCHAR(3),
    @ngaykham   DATETIME,
    @BSphutrach INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sttBN   INT = dbo.fn_NextBenhNhanId();
    DECLARE @sttKhoa INT;

    SELECT @sttKhoa = sttKhoa FROM BacSi WHERE sttBS = @BSphutrach;

    INSERT INTO BenhNhan(sttBN, hoten, CMND, diachi, gioitinh, ngaykham, BSphutrach, sttKhoa)
    VALUES (@sttBN, @hoten, @CMND, @diachi, @gioitinh, @ngaykham, @BSphutrach, @sttKhoa);

    INSERT INTO BenhNhanThuong(sttBN) VALUES(@sttBN);
END;
GO

-- Thêm BN nằm giường (chưa xếp chỗ)
CREATE OR ALTER PROC sp_InsertBenhNhan_BenhNhanNamGiuong_ChuaXepCho
    @hoten      NVARCHAR(50),
    @CMND       INT,
    @diachi     NVARCHAR(100),
    @gioitinh   NVARCHAR(3),
    @ngaykham   DATETIME,
    @BSphutrach INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sttBN   INT = dbo.fn_NextBenhNhanId();
    DECLARE @sttKhoa INT;

    SELECT @sttKhoa = sttKhoa FROM BacSi WHERE sttBS = @BSphutrach;

    INSERT INTO BenhNhan(sttBN, hoten, CMND, diachi, gioitinh, ngaykham, BSphutrach, sttKhoa)
    VALUES (@sttBN, @hoten, @CMND, @diachi, @gioitinh, @ngaykham, @BSphutrach, @sttKhoa);

    INSERT INTO BenhNhanNamGiuong_ChuaXepCho(sttBN) VALUES(@sttBN);
END;
GO

-- Cập nhật thông tin BN (khoa cũng tự lấy theo bác sĩ)
CREATE OR ALTER PROC sp_UpdateThongTinBenhNhan
    @sttBN      INT,
    @hoten      NVARCHAR(50),
    @CMND       INT,
    @diachi     NVARCHAR(100),
    @gioitinh   NVARCHAR(3),
    @ngaykham   DATETIME,
    @BSphutrach INT
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM BenhNhan WHERE sttBN=@sttBN) RETURN;

    DECLARE @sttKhoa INT;
    SELECT @sttKhoa = sttKhoa FROM BacSi WHERE sttBS = @BSphutrach;

    UPDATE BenhNhan
    SET hoten      = @hoten,
        CMND       = @CMND,
        diachi     = @diachi,
        gioitinh   = @gioitinh,
        ngaykham   = @ngaykham,
        BSphutrach = @BSphutrach,
        sttKhoa    = @sttKhoa
    WHERE sttBN = @sttBN;
END;
GO

-- BN nằm giường đã xếp chỗ (chuyển từ "chưa xếp chỗ")
CREATE OR ALTER PROC sp_InsertBenhNhanNamGiuong_DaXepCho
    @sttBN  INT,
    @sttGB  INT,
    @ngayxep DATETIME
AS
BEGIN
    INSERT INTO BenhNhanNamGiuong_DaXepCho(sttBN, sttGB, ngayxep)
    VALUES(@sttBN, @sttGB, @ngayxep);

    DELETE FROM BenhNhanNamGiuong_ChuaXepCho WHERE sttBN=@sttBN;

    UPDATE GiuongBenh SET tinhtrang = N'Có bệnh nhân' WHERE sttGB=@sttGB;
END;
GO

-- Danh sách BN nói chung (kèm khoa)
CREATE OR ALTER PROC sp_SelectAllBenhNhan
AS
BEGIN
    SET NOCOUNT ON;

    SELECT bn.sttBN   AS N'Mã Bệnh Nhân',
           bn.hoten   AS N'Họ Tên Bệnh Nhân',
           bn.CMND,
           bn.diachi  AS N'Địa Chỉ',
           bn.ngaykham AS N'Ngày Khám Bệnh',
           bn.BSphutrach AS N'Bác Sĩ Điều Trị',
           bn.gioitinh AS N'Giới Tính',
           bn.sttKhoa  AS N'Mã Khoa',
           ISNULL(k.TenKhoa, N'') AS N'Khoa'
    FROM BenhNhan bn
    LEFT JOIN Khoa k ON bn.sttKhoa = k.sttKhoa;
END;
GO

-- BN thường
CREATE OR ALTER PROC sp_SelectAllBenhNhanThuong
AS
BEGIN
    SELECT bnt.sttBNT,
           bnt.sttBN AS N'Mã Bệnh Nhân',
           bn.hoten  AS N'Họ Tên Bệnh Nhân',
           bn.CMND   AS N'Số Chứng Minh Nhân Dân',
           bn.diachi AS N'Địa Chỉ',
           bs.sttBS,
           bs.tenBS  AS N'Bác Sĩ Điều Trị'
    FROM BenhNhanThuong bnt
    JOIN BenhNhan bn ON bnt.sttBN = bn.sttBN
    JOIN BacSi bs    ON bn.BSphutrach = bs.sttBS;
END;
GO

-- BN nằm giường chưa xếp chỗ
CREATE OR ALTER PROC sp_selectAllBenhNhanNamGiuong_ChuaXepCho
AS
BEGIN
    SELECT bnng.sttBN          AS N'Mã Bệnh Nhân',
           bn.hoten            AS N'Họ Tên Bệnh Nhân',
           bn.CMND             AS N'Số Chứng Minh Nhân Dân',
           bn.diachi           AS N'Địa Chỉ',
           bs.sttBS,
           bs.tenBS            AS N'Bác Sĩ Điều Trị'
    FROM BenhNhanNamGiuong_ChuaXepCho bnng
    JOIN BenhNhan bn ON bnng.sttBN = bn.sttBN
    JOIN BacSi bs    ON bn.BSphutrach = bs.sttBS;
END;
GO

-- BN nằm giường đã xếp chỗ
CREATE OR ALTER PROC sp_selectAllBenhNhanNamGiuong_DaXepCho
AS
BEGIN
    SELECT bnng.sttBNNG_DXC,
           bnng.sttBN          AS N'Mã Bệnh Nhân',
           bn.hoten            AS N'Họ Tên Bệnh Nhân',
           bn.CMND             AS N'Số Chứng Minh Nhân Dân',
           bn.diachi           AS N'Địa Chỉ',
           bs.sttBS,
           bs.tenBS            AS N'Bác Sĩ Điều Trị',
           bnng.sttGB          AS N'Giường',
           bnng.ngayxep        AS N'Ngày xếp'
    FROM BenhNhanNamGiuong_DaXepCho bnng
    JOIN BenhNhan bn ON bnng.sttBN = bn.sttBN
    JOIN BacSi bs    ON bn.BSphutrach = bs.sttBS;
END;
GO

-- Xóa BN nằm giường đã xếp chỗ, trả giường về "Còn Trống"
CREATE OR ALTER PROC sp_DeleteBenhNhanNamGiuong_DaXepCho
    @sttBN INT,
    @sttGB INT
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM BenhNhanNamGiuong_DaXepCho WHERE sttBN=@sttBN) RETURN;

    DELETE FROM BenhNhanNamGiuong_DaXepCho WHERE sttBN=@sttBN;
    UPDATE GiuongBenh SET tinhtrang = N'Còn Trống' WHERE sttGB=@sttGB;
END;
GO

-- Một số SP lọc theo tên / bác sĩ / giới tính (giữ nguyên format cũ)
CREATE OR ALTER PROC sp_SelectAllBenhNhanByHoTen
    @hoten NVARCHAR(50)
AS
BEGIN
    SELECT sttBN AS N'Mã Bệnh Nhân',
           hoten AS N'Họ Tên Bệnh Nhân',
           CMND,
           diachi AS N'Địa Chỉ',
           ngaykham AS N'Ngày Khám Bệnh',
           BSphutrach AS N'Bác Sĩ Điều Trị',
           gioitinh AS N'Giới Tính'
    FROM BenhNhan
    WHERE hoten=@hoten;
END;
GO

CREATE OR ALTER PROC sp_SelectAllBenhNhanByBacSiDieuTri
    @sttBS INT
AS
BEGIN
    SELECT sttBN AS N'Mã Bệnh Nhân',
           hoten AS N'Họ Tên Bệnh Nhân',
           CMND,
           diachi AS N'Địa Chỉ',
           ngaykham AS N'Ngày Khám Bệnh',
           BSphutrach AS N'Bác Sĩ Điều Trị',
           gioitinh AS N'Giới Tính'
    FROM BenhNhan
    WHERE BSphutrach=@sttBS;
END;
GO

CREATE OR ALTER PROC sp_SelectAllBenhNhanByBacSiAndGioiTinh
    @sttBS INT,
    @gioitinh NVARCHAR(5)
AS
BEGIN
    SELECT sttBN AS N'Mã Bệnh Nhân',
           hoten AS N'Họ Tên Bệnh Nhân',
           CMND,
           diachi AS N'Địa Chỉ',
           ngaykham AS N'Ngày Khám Bệnh',
           BSphutrach AS N'Bác Sĩ Điều Trị',
           gioitinh AS N'Giới Tính'
    FROM BenhNhan
    WHERE BSphutrach=@sttBS AND gioitinh=@gioitinh;
END;
GO

-------------------------
-- 3.4 ĐƠN THUỐC & HÓA ĐƠN
-------------------------

-- Thêm đơn thuốc cho BN thường
CREATE OR ALTER PROC sp_InsertDonThuoc_DonThuocBenhNhanThuong
    @TenThuoc         NVARCHAR(300),
    @TienThuoc        INT,
    @MaBenhNhanThuong INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DonThuoc(TenThuoc, NgayCapThuoc, TienThuoc)
    VALUES(@TenThuoc, GETDATE(), @TienThuoc);

    DECLARE @sttDT INT = SCOPE_IDENTITY();

    INSERT INTO DonThuoc_BenhNhanThuong(sttDT, MaBenhNhanThuong)
    VALUES(@sttDT, @MaBenhNhanThuong);
END;
GO

-- Thêm đơn thuốc cho BN nằm giường đã xếp chỗ
CREATE OR ALTER PROC sp_InsertDonThuoc_DonThuocBenhNhanNamGiuong_DaXepCho
    @TenThuoc                      NVARCHAR(300),
    @TienThuoc                     INT,
    @MaBenhNhanNamGiuong_DaXepCho  INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DonThuoc(TenThuoc, NgayCapThuoc, TienThuoc)
    VALUES(@TenThuoc, GETDATE(), @TienThuoc);

    DECLARE @sttDT INT = SCOPE_IDENTITY();

    INSERT INTO DonThuoc_BenhNhanNamGiuong_DaXepCho(sttDT, MaBenhNhanNamGiuong_DaXepCho, TinhTrang)
    VALUES(@sttDT, @MaBenhNhanNamGiuong_DaXepCho, N'Chưa thanh toán');
END;
GO

-- DS đơn thuốc BN nằm giường đã xếp chỗ (chưa thanh toán)
CREATE OR ALTER PROC sp_selectAllDonThuoc_BenhNhanNamGiuong_DaXepChoByID
    @sttBNNG_DXC INT
AS
BEGIN
    SELECT dt_bn.sttDT         AS N'Mã Đơn Thuốc',
           dt_bn.MaBenhNhanNamGiuong_DaXepCho AS N'Mã Bệnh Nhân',
           dt.TenThuoc         AS N'Tên Thuốc',
           dt.TienThuoc        AS N'Tiền Thuốc',
           dt_bn.TinhTrang     AS N'Tình Trạng'
    FROM DonThuoc_BenhNhanNamGiuong_DaXepCho dt_bn
    JOIN DonThuoc dt ON dt.sttDT = dt_bn.sttDT
    WHERE dt_bn.MaBenhNhanNamGiuong_DaXepCho = @sttBNNG_DXC
      AND dt_bn.TinhTrang = N'Chưa thanh toán';
END;
GO

-- DS đơn thuốc BN thường
CREATE OR ALTER PROC sp_selectAllDonThuoc_BenhNhanThuongByID
    @sttBNT INT
AS
BEGIN
    SELECT dt_bn.sttDT_BNT        AS N'Mã Chi Tiết',
           dt_bn.sttDT            AS N'Mã Đơn Thuốc',
           dt_bn.MaBenhNhanThuong AS N'Mã BN Thường',
           dt.TenThuoc            AS N'Tên Thuốc',
           dt.TienThuoc           AS N'Tiền Thuốc',
           dt.NgayCapThuoc        AS N'Ngày Cấp'
    FROM DonThuoc_BenhNhanThuong dt_bn
    JOIN DonThuoc dt ON dt.sttDT = dt_bn.sttDT
    WHERE dt_bn.MaBenhNhanThuong = @sttBNT;
END;
GO

-- Lấy tham số
CREATE OR ALTER PROC sp_selectThamSo
AS
BEGIN
    SELECT * FROM ThamSo;
END;
GO

-- Thêm hóa đơn
CREATE OR ALTER PROC sp_InsertHoaDon
    @sttBNNG_DXC INT,
    @ThanhTien   INT
AS
BEGIN
    INSERT INTO HoaDon(sttBNNG_DXC, ThanhTien)
    VALUES(@sttBNNG_DXC, @ThanhTien);
END;
GO

-- (optional) Tính tổng tiền thuốc chưa thanh toán cho 1 bệnh nhân nằm giường
CREATE OR ALTER PROC sp_TinhTienThuoc_ChuaThanhToan
    @sttBNNG_DXC INT
AS
BEGIN
    SELECT ISNULL(SUM(dt.TienThuoc), 0) AS TongTienThuoc
    FROM DonThuoc_BenhNhanNamGiuong_DaXepCho dt_bn
    JOIN DonThuoc dt ON dt.sttDT = dt_bn.sttDT
    WHERE dt_bn.MaBenhNhanNamGiuong_DaXepCho = @sttBNNG_DXC
      AND dt_bn.TinhTrang = N'Chưa thanh toán';
END;
GO

/*========================================================
  4. DỮ LIỆU MẪU
========================================================*/

-- 4.1 Khoa mẫu

Insert into Khoa ( TenKhoa ) values (N'Khoa Dinh dưỡng')
Insert into Khoa ( TenKhoa ) values (N'Khoa Răng – hàm – mặt')
Insert into Khoa ( TenKhoa ) values (N'Khoa Nội tim mạch')
Insert into Khoa ( TenKhoa ) values (N'Khoa Nội tiêu hóa')
Insert into Khoa ( TenKhoa ) values (N'Khoa Nội tiết')
Insert into Khoa ( TenKhoa ) values (N'Khoa Truyền nhiễm')
Insert into Khoa ( TenKhoa ) values (N'Khoa Ngoại thận – tiết niệu')
Insert into Khoa ( TenKhoa ) values (N'Khoa Chấn thương chỉnh hình')
Insert into Khoa ( TenKhoa ) values (N'Khoa Vật lý trị liệu')
Insert into Khoa ( TenKhoa ) values (N'Khoa Sản')
Insert into Khoa ( TenKhoa ) values (N'Khoa Khoa Giải phẫu bệnh')
Insert into Khoa ( TenKhoa ) values (N'Khoa Ngoại tiêu hóa')
Insert into Khoa ( TenKhoa ) values (N'Khoa Tâm Lý')

GO


INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Paracetamol 500mg', N'Giảm đau, hạ sốt', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Ibuprofen 400mg', N'Chống viêm, giảm đau khớp', 3500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Amoxicillin 500mg', N'Kháng sinh điều trị nhiễm khuẩn', 4500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Azithromycin 500mg', N'Kháng sinh phổ rộng, trị viêm họng', 15000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Cefuroxime 500mg', N'Kháng sinh thế hệ 2, trị viêm phổi', 18000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Metronidazole 250mg', N'Trị nhiễm khuẩn đường ruột', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Ciprofloxacin 500mg', N'Kháng sinh trị nhiễm trùng tiết niệu', 7000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Levofloxacin 500mg', N'Kháng sinh mạnh trị viêm phổi, viêm xoang', 12000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Metformin 500mg', N'Điều trị tiểu đường tuýp 2', 2000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Gliclazide 80mg', N'Giảm đường huyết, điều trị tiểu đường', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Insulin glargine', N'Tiêm hạ đường huyết, tiểu đường type 1', 250000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Losartan 50mg', N'Hạ huyết áp, bảo vệ tim mạch', 5000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Amlodipine 5mg', N'Giãn mạch, điều trị tăng huyết áp', 4000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Perindopril 5mg', N'Điều trị suy tim, hạ áp', 8000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Enalapril 5mg', N'Hạ huyết áp, điều hòa tim mạch', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Bisoprolol 5mg', N'Điều trị cao huyết áp, rối loạn tim', 7000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Atorvastatin 20mg', N'Giảm mỡ máu, cholesterol cao', 10000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Rosuvastatin 10mg', N'Hạ cholesterol, bảo vệ tim mạch', 15000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Simvastatin 20mg', N'Điều trị rối loạn lipid máu', 9000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Omeprazole 20mg', N'Giảm acid dạ dày, trị viêm loét', 4000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Esomeprazole 40mg', N'Trị trào ngược dạ dày, viêm loét', 12000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Pantoprazole 40mg', N'Giảm tiết acid dạ dày, đau dạ dày', 10000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Ranitidine 150mg', N'Giảm đau dạ dày, đầy hơi', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Cetirizine 10mg', N'Giảm dị ứng, sổ mũi', 2000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Loratadine 10mg', N'Chống dị ứng, viêm mũi', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Fexofenadine 180mg', N'Giảm ngứa, mề đay, dị ứng da', 5000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Salbutamol inhaler', N'Hít giãn phế quản, trị hen suyễn', 60000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Budesonide inhaler', N'Giảm viêm phổi, hen mãn tính', 90000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Prednisone 5mg', N'Chống viêm, dị ứng, hen suyễn', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Dexamethasone 4mg', N'Giảm viêm mạnh, sốc phản vệ', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Diazepam 5mg', N'Giảm lo âu, mất ngủ', 4000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Alprazolam 0.5mg', N'An thần, giảm căng thẳng', 6000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Domperidone 10mg', N'Chống nôn, đầy hơi', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Metoclopramide 10mg', N'Kích thích tiêu hóa, giảm buồn nôn', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Loperamide 2mg', N'Chống tiêu chảy cấp', 2000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Oral rehydration salts', N'Bù nước điện giải khi tiêu chảy', 1000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Iron folic acid tablet', N'Bổ máu, cho phụ nữ mang thai', 2000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Calcium carbonate + Vitamin D3', N'Bổ sung canxi, xương khớp', 5000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Multivitamin tablet', N'Bổ sung vitamin tổng hợp', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Vitamin C 500mg', N'Tăng đề kháng, giảm cảm cúm', 1500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Ferrous sulfate 325mg', N'Bổ sung sắt, thiếu máu', 2000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Clopidogrel 75mg', N'Chống kết tập tiểu cầu', 12000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Aspirin 81mg', N'Giảm đau, chống đông máu nhẹ', 1000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Warfarin 5mg', N'Chống đông máu mạnh', 5000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Furosemide 40mg', N'Thuốc lợi tiểu, phù nề', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Spironolactone 25mg', N'Lợi tiểu giữ kali', 3000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Hydrochlorothiazide 25mg', N'Hạ áp, lợi tiểu nhẹ', 2500);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Nitroglycerin 0.5mg SL', N'Cắt cơn đau thắt ngực', 10000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Acetylcysteine 200mg', N'Thuốc long đờm, viêm phổi', 4000);
INSERT INTO LoaiThuoc (TenLoai, MoTa, DonGia) VALUES (N'Glucosamine sulfate 500mg', N'Hỗ trợ sụn khớp, thoái hóa khớp', 15000);
go

-- 4.2 Bác sĩ mẫu
INSERT INTO BacSi(tenBS, username, [password], DiaChi, DienThoai, sttKhoa)
VALUES (N'Lê Tiến Đức',  'duc',  '123', N'TP HCM', '0909', 1),
       (N'Nguyễn Nhật Bình', 'binh', '123456', N'TP HCM', '0989', 2);
GO


-- 4.4 Tham số tiền giường
INSERT INTO ThamSo(Tien1Ngay1Giuong) VALUES (50000);
GO


-- ===== KHOA =====
CREATE OR ALTER PROC dbo.sp_SelectAllKhoa
AS
BEGIN
    SET NOCOUNT ON;

    SELECT sttKhoa, TenKhoa
    FROM   Khoa
    ORDER BY TenKhoa;
END;
GO

-- ===== BÁC SĨ CHO COMBOBOX TRA CỨU =====
CREATE OR ALTER PROC dbo.sp_SelectAllBacSiForTraCuu
AS
BEGIN
    SET NOCOUNT ON;

    SELECT bs.sttBS,
           bs.tenBS,
           bs.sttKhoa,
           ISNULL(k.TenKhoa, N'') AS TenKhoa,
           bs.tenBS
             + CASE WHEN k.TenKhoa IS NULL OR k.TenKhoa = N'' 
                    THEN N'' 
                    ELSE N' - ' + k.TenKhoa 
               END              AS TenHienThi
    FROM   BacSi bs
    LEFT JOIN Khoa k ON bs.sttKhoa = k.sttKhoa
    ORDER BY bs.tenBS;
END;
GO

--Update Giuong benh


/* 1. Cho phép TenGiuong tạm NULL và dọn dữ liệu cũ */
IF COL_LENGTH('dbo.GiuongBenh', 'TenGiuong') IS NOT NULL
BEGIN
    -- cho phép NULL để lệnh INSERT cũ không bị fail
    ALTER TABLE dbo.GiuongBenh
    ALTER COLUMN TenGiuong NVARCHAR(50) NULL;

    -- nếu từng tạo constraint unique cũ thì xoá
    IF EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.GiuongBenh')
          AND name = 'UQ_GiuongBenh_TenGiuong'
    )
    BEGIN
        ALTER TABLE dbo.GiuongBenh
        DROP CONSTRAINT UQ_GiuongBenh_TenGiuong;
    END

    -- đảm bảo tất cả giường hiện tại đều có tên
    UPDATE dbo.GiuongBenh
    SET TenGiuong = N'Giường ' + CAST(sttGB AS NVARCHAR(10))
    WHERE TenGiuong IS NULL OR TenGiuong = N'';
END
GO

/* 2. Trigger: mỗi lần thêm giường mới sẽ tự đặt TenGiuong = 'Giường ' + sttGB */
IF OBJECT_ID('dbo.trg_GiuongBenh_SetTenGiuong', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_GiuongBenh_SetTenGiuong;
GO

CREATE TRIGGER dbo.trg_GiuongBenh_SetTenGiuong
ON dbo.GiuongBenh
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE gb
    SET TenGiuong = N'Giường ' + CAST(i.sttGB AS NVARCHAR(10))
    FROM dbo.GiuongBenh gb
    JOIN inserted i ON gb.sttGB = i.sttGB
    WHERE i.TenGiuong IS NULL OR i.TenGiuong = N'';
END;
GO

/* 3. Tạo index unique đảm bảo tên giường không trùng nhau */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.GiuongBenh')
      AND name = 'IX_GiuongBenh_TenGiuong_Unique'
)
BEGIN
    CREATE UNIQUE INDEX IX_GiuongBenh_TenGiuong_Unique
    ON dbo.GiuongBenh(TenGiuong);
END;
GO


--Update BacSI
-- Cho phép username, password được NULL
ALTER TABLE BacSi
    ALTER COLUMN username VARCHAR(50) NULL;

ALTER TABLE dbo.BacSi
    ALTER COLUMN [password] VARCHAR(50) NULL;
GO
CREATE OR ALTER PROC sp_InsertBacSi
    @tenBS     NVARCHAR(100),
    @username  VARCHAR(50) = NULL,
    @password  VARCHAR(50) = NULL,
    @DiaChi    NVARCHAR(200) = NULL,
    @DienThoai VARCHAR(20)   = NULL,
    @sttKhoa   INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Nếu không nhập username/password thì để NULL => xem như chưa có tài khoản
    IF (ISNULL(LTRIM(RTRIM(@username)), '') = '')
        SET @username = NULL;

    IF (ISNULL(LTRIM(RTRIM(@password)), '') = '')
        SET @password = NULL;

    INSERT INTO BacSi(tenBS, username, [password], DiaChi, DienThoai, sttKhoa)
    VALUES(@tenBS, @username, @password, @DiaChi, @DienThoai, @sttKhoa);
END;
GO


-- Bảng tài khoản đăng nhập
IF OBJECT_ID('dbo.TaiKhoan', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan
    (
        MaTK     INT IDENTITY(1,1) PRIMARY KEY,
        Username VARCHAR(50) NOT NULL UNIQUE,
        [Password] VARCHAR(100) NOT NULL,     -- sau có thể đổi sang password hash
        VaiTro   NVARCHAR(50) NOT NULL,       -- Admin, BacSi, ThuNgan, LeTan,...
        sttBS    INT NULL,                    -- nếu là tài khoản bác sĩ thì link sang BacSi
        HoTen    NVARCHAR(100) NULL,          -- dùng cho user không phải bác sĩ
        IsActive BIT NOT NULL CONSTRAINT DF_TaiKhoan_IsActive DEFAULT(1),
        NgayTao  DATETIME NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT(GETDATE()),

        CONSTRAINT FK_TaiKhoan_BacSi 
            FOREIGN KEY(sttBS) REFERENCES dbo.BacSi(sttBS)
            ON DELETE SET NULL  -- xóa bác sĩ thì tài khoản vẫn giữ, chỉ bỏ link
    );
END
GO


ALTER TABLE dbo.BacSi
    ALTER COLUMN username VARCHAR(50) NULL;

ALTER TABLE dbo.BacSi
    ALTER COLUMN [password] VARCHAR(50) NULL;
GO


INSERT INTO TaiKhoan(Username,[Password],VaiTro,sttBS,IsActive)
VALUES 
('admin', '123', N'Admin', NULL, 1)
GO

SELECT tk.MaTK,
       tk.Username,
       tk.VaiTro,
       tk.sttBS,
       bs.tenBS
FROM   TaiKhoan tk
LEFT JOIN BacSi bs ON tk.sttBS = bs.sttBS
WHERE  tk.Username = 'Admin'
  AND  tk.[Password] = '123'   
  AND  tk.IsActive = 1;

  --Tạo bảng hóa đơn cho bệnh nhân thường 

  IF OBJECT_ID('dbo.HoaDon_BenhNhanThuong', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon_BenhNhanThuong
    (
        sttHD_BNT INT IDENTITY(1,1) PRIMARY KEY,
        sttPK     INT NOT NULL
            CONSTRAINT FK_HoaDonBNT_PhieuKham 
                REFERENCES dbo.PhieuKham(sttPK)
                ON DELETE CASCADE,

        NgayLap   DATETIME NOT NULL 
            CONSTRAINT DF_HoaDonBNT_NgayLap DEFAULT(GETDATE()),

        TienKham  INT NOT NULL 
            CONSTRAINT DF_HoaDonBNT_TienKham DEFAULT(0),

        TienThuoc INT NOT NULL 
            CONSTRAINT DF_HoaDonBNT_TienThuoc DEFAULT(0),

        TienDV    INT NOT NULL 
            CONSTRAINT DF_HoaDonBNT_TienDV DEFAULT(0),

        TongTien  INT NOT NULL,       -- = TienKham + TienThuoc + TienDV
        GhiChu    NVARCHAR(255) NULL
    );
END;
GO

  -- Sửa lại khóa ngoại Lịch hẹn

/* 1. Tìm và xóa mọi FOREIGN KEY từ PhieuKham -> LichHen */
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = STRING_AGG(
           N'ALTER TABLE dbo.PhieuKham DROP CONSTRAINT [' + fk.name + N']',
           N';'
       )
FROM sys.foreign_keys fk
JOIN sys.tables t   ON fk.parent_object_id     = t.object_id
JOIN sys.tables rt  ON fk.referenced_object_id = rt.object_id
WHERE t.name  = 'PhieuKham'
  AND rt.name = 'LichHen';

IF @sql <> N''
BEGIN
    PRINT N'Drop FK: ' + @sql;
    EXEC(@sql);
END
ELSE
BEGIN
    PRINT N'Không tìm thấy FK nào từ PhieuKham tới LichHen.';
END
GO

/* 2. Cho phép sttLH NULL để dùng ON DELETE SET NULL */
ALTER TABLE dbo.PhieuKham
    ALTER COLUMN sttLH INT NULL;
GO

/* 3. Tạo lại FK với ON DELETE SET NULL */
ALTER TABLE dbo.PhieuKham
ADD CONSTRAINT FK_PhieuKham_LichHen
    FOREIGN KEY (sttLH)
    REFERENCES dbo.LichHen(sttLH)
    ON DELETE SET NULL;
GO


--UPdate 26/11
IF OBJECT_ID('dbo.sp_PhieuKham_LapTuLichHen', 'P') IS NOT NULL
    DROP PROC dbo.sp_PhieuKham_LapTuLichHen;
GO

CREATE PROC dbo.sp_PhieuKham_LapTuLichHen
    @sttLH       INT,
    @TrieuChung  NVARCHAR(MAX) = NULL,
    @ChanDoan    NVARCHAR(MAX) = NULL,
    @GhiChu      NVARCHAR(MAX) = NULL,
    @TienKham    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sttBN   INT,
            @sttBS   INT,
            @NgayHen DATETIME;

    -- 1) Lấy thông tin từ lịch hẹn
    SELECT  @sttBN   = lh.sttBN,
            @sttBS   = lh.sttBS,
            @NgayHen = lh.NgayHen
    FROM    dbo.LichHen lh
    WHERE   lh.sttLH = @sttLH;

    IF @sttBN IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy lịch hẹn.', 16, 1);
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRAN;

        ------------------------------------------------
        -- 2) Lập PHIẾU KHÁM
        ------------------------------------------------
        INSERT INTO dbo.PhieuKham
        (
            sttBN, sttBS, NgayKham,
            TrieuChung, ChanDoan, GhiChu,
            TienKham, sttLH
        )
        VALUES
        (
            @sttBN, @sttBS, @NgayHen,
            @TrieuChung, @ChanDoan, @GhiChu,
            @TienKham, @sttLH
        );

        DECLARE @sttPK INT = SCOPE_IDENTITY();

        ------------------------------------------------
        -- 3) Tính tiền thuốc của BỆNH NHÂN THƯỜNG
        ------------------------------------------------
        DECLARE @TienThuoc INT;

        SELECT @TienThuoc = ISNULL(SUM(dt.TienThuoc), 0)
        FROM   dbo.DonThuoc_BenhNhanThuong dt_bnt
               INNER JOIN dbo.BenhNhanThuong bnt
                    ON dt_bnt.MaBenhNhanThuong = bnt.sttBNT
               INNER JOIN dbo.DonThuoc dt
                    ON dt_bnt.sttDT = dt.sttDT
        WHERE  bnt.sttBN = @sttBN;

        IF @TienThuoc IS NULL SET @TienThuoc = 0;

        DECLARE @TienDV   INT = 0;  -- sau này nếu có dịch vụ thì cộng thêm
        DECLARE @TongTien INT = @TienKham + @TienThuoc + @TienDV;

        ------------------------------------------------
        -- 4) Lập HÓA ĐƠN bệnh nhân thường
        ------------------------------------------------
        INSERT INTO dbo.HoaDon_BenhNhanThuong
        (
            sttPK, NgayLap,
            TienKham, TienThuoc, TienDV, TongTien,
            GhiChu
        )
        VALUES
        (
            @sttPK, GETDATE(),
            @TienKham, @TienThuoc, @TienDV, @TongTien,
            @GhiChu
        );

        ------------------------------------------------
        -- 5) XÓA ĐƠN THUỐC bệnh nhân thường (chỉ bảng link)
        ------------------------------------------------
        DELETE dt_bnt
        FROM   dbo.DonThuoc_BenhNhanThuong dt_bnt
               INNER JOIN dbo.BenhNhanThuong bnt
                    ON dt_bnt.MaBenhNhanThuong = bnt.sttBNT
        WHERE  bnt.sttBN = @sttBN;

        ------------------------------------------------
        -- 6) XÓA LỊCH HẸN sau khi đã khám xong
        ------------------------------------------------
        DELETE FROM dbo.LichHen
        WHERE  sttLH = @sttLH;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END;
GO

--upDATE 27/11
IF OBJECT_ID('dbo.sp_PhieuKham_LapTuLichHen', 'P') IS NOT NULL
    DROP PROC dbo.sp_PhieuKham_LapTuLichHen;
GO

CREATE PROC dbo.sp_PhieuKham_LapTuLichHen
    @sttLH       INT,
    @TrieuChung  NVARCHAR(MAX) = NULL,
    @ChanDoan    NVARCHAR(MAX) = NULL,
    @GhiChu      NVARCHAR(MAX) = NULL,
    @TienKham    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sttBN   INT,
            @sttBS   INT,
            @NgayHen DATETIME;

    -- 1) Lấy thông tin từ lịch hẹn
    SELECT  @sttBN   = lh.sttBN,
            @sttBS   = lh.sttBS,
            @NgayHen = lh.NgayHen
    FROM    dbo.LichHen lh
    WHERE   lh.sttLH = @sttLH;

    IF @sttBN IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy lịch hẹn.', 16, 1);
        RETURN;
    END;

    -- 2) Lập PHIẾU KHÁM (lịch sử khám)
    INSERT INTO dbo.PhieuKham
    (
        sttBN, sttBS, NgayKham,
        TrieuChung, ChanDoan, GhiChu,
        TienKham, sttLH
    )
    VALUES
    (
        @sttBN, @sttBS, @NgayHen,
        @TrieuChung, @ChanDoan, @GhiChu,
        @TienKham, @sttLH
    );

    -- 3) Cập nhật trạng thái lịch hẹn -> Đã khám
    UPDATE dbo.LichHen
    SET TrangThai = N'Đã khám'
    WHERE sttLH = @sttLH;
END;
GO

CREATE OR ALTER PROC dbo.sp_ThanhToan_BenhNhanThuong
    @sttBNT INT   -- khóa chính BenhNhanThuong
AS
BEGIN
    SET NOCOUNT ON;

    -- Xóa các đơn thuốc link với bệnh nhân thường
    DELETE dt_bnt
    FROM   DonThuoc_BenhNhanThuong dt_bnt
    WHERE  dt_bnt.MaBenhNhanThuong = @sttBNT;

    -- Xóa khỏi danh sách bệnh nhân thường (hàng khám trong ngày)
    DELETE FROM BenhNhanThuong
    WHERE  sttBNT = @sttBNT;

    -- KHÔNG xóa BenhNhan để còn lịch sử (phiếu khám, hóa đơn…)
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_LichHen_BenhNhan'
      AND parent_object_id = OBJECT_ID('dbo.LichHen')
)
BEGIN
    ALTER TABLE dbo.LichHen
        DROP CONSTRAINT FK_LichHen_BenhNhan;
END
GO

ALTER TABLE dbo.LichHen
ADD CONSTRAINT FK_LichHen_BenhNhan
    FOREIGN KEY (sttBN)
    REFERENCES dbo.BenhNhan(sttBN)
    ON DELETE CASCADE;   -- xóa bệnh nhân sẽ xóa luôn các lịch hẹn
GO



IF OBJECT_ID('dbo.LichSuKham', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LichSuKham
    (
        sttLSK     INT IDENTITY(1,1) PRIMARY KEY,
        sttBN      INT NOT NULL CONSTRAINT FK_LichSuKham_BenhNhan REFERENCES dbo.BenhNhan(sttBN),
        sttBS      INT NOT NULL CONSTRAINT FK_LichSuKham_BacSi REFERENCES dbo.BacSi(sttBS),
        NgayKham   DATETIME NOT NULL,
        TrieuChung NVARCHAR(MAX) NULL,
        ChanDoan   NVARCHAR(MAX) NULL,
        GhiChu     NVARCHAR(MAX) NULL,
        TienKham   INT NULL
    );
END;
GO

CREATE OR ALTER PROC dbo.sp_LichHen_HoanTatKham
    @sttLH       INT,
    @TrieuChung  NVARCHAR(MAX) = NULL,
    @ChanDoan    NVARCHAR(MAX) = NULL,
    @GhiChu      NVARCHAR(MAX) = NULL,
    @TienKham    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @sttBN   INT,
        @sttBS   INT,
        @NgayHen DATETIME,
        @Email   NVARCHAR(200),
        @TenBN   NVARCHAR(100),
        @TenBS   NVARCHAR(100);

    -- 1. Lấy thông tin từ lịch hẹn
    SELECT  @sttBN   = lh.sttBN,
            @sttBS   = lh.sttBS,
            @NgayHen = lh.NgayHen,
            @Email   = lh.email
    FROM    dbo.LichHen lh
    WHERE   lh.sttLH = @sttLH;

    IF @sttBN IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy lịch hẹn.', 16, 1);
        RETURN;
    END;

    SELECT @TenBN = hoten FROM dbo.BenhNhan WHERE sttBN = @sttBN;
    SELECT @TenBS = tenBS FROM dbo.BacSi WHERE sttBS = @sttBS;

    BEGIN TRY
        BEGIN TRAN;

        -- 2. Lưu vào bảng lịch sử khám
        INSERT INTO dbo.LichSuKham
        (
            sttBN, sttBS, NgayKham,
            TrieuChung, ChanDoan, GhiChu,
            TienKham
        )
        VALUES
        (
            @sttBN, @sttBS, @NgayHen,
            @TrieuChung, @ChanDoan, @GhiChu,
            @TienKham
        );

        -- 3. Xóa lịch hẹn
        DELETE FROM dbo.LichHen WHERE sttLH = @sttLH;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @msg NVARCHAR(4000);
        SET @msg = ERROR_MESSAGE();  
        RAISERROR(@msg, 16, 1);
        RETURN;
    END CATCH;

    -- 4. Trả dữ liệu ra cho ứng dụng gửi email
    SELECT
        @Email      AS Email,
        @TenBN      AS TenBenhNhan,
        @TenBS      AS TenBacSi,
        @NgayHen    AS NgayKham,
        @TrieuChung AS TrieuChung,
        @ChanDoan   AS ChanDoan,
        @GhiChu     AS GhiChu,
        @TienKham   AS TienKham;
END;
GO


IF OBJECT_ID('dbo.sp_ThanhToan_BenhNhanNamGiuong', 'P') IS NOT NULL
    DROP PROC dbo.sp_ThanhToan_BenhNhanNamGiuong;
GO

CREATE PROC dbo.sp_ThanhToan_BenhNhanNamGiuong
    @sttBNNG_DXC INT,          -- mã bệnh nhân nằm giường đã xếp chỗ
    @ThanhTien   INT = NULL,   -- tổng tiền (nếu app truyền vào)
    @TienGiuong  INT = NULL,   -- tùy app có truyền hay không
    @TienThuoc   INT = NULL,
    @TienDV      INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Nếu không truyền @ThanhTien thì tự cộng từ các thành phần
    IF @ThanhTien IS NULL
        SET @ThanhTien = ISNULL(@TienGiuong, 0)
                       + ISNULL(@TienThuoc, 0)
                       + ISNULL(@TienDV, 0);

    ----------------------------------------------------
    -- 1. LƯU HÓA ĐƠN
    ----------------------------------------------------
    INSERT INTO dbo.HoaDon(sttBNNG_DXC, ThanhTien)
    VALUES(@sttBNNG_DXC, ISNULL(@ThanhTien, 0));

    ----------------------------------------------------
    -- 2. ĐÁNH DẤU ĐƠN THUỐC ĐÃ THANH TOÁN
    ----------------------------------------------------
    UPDATE dbo.DonThuoc_BenhNhanNamGiuong_DaXepCho
    SET TinhTrang = N'Đã thanh toán'
    WHERE MaBenhNhanNamGiuong_DaXepCho = @sttBNNG_DXC;

    ----------------------------------------------------
    -- 3. TRẢ GIƯỜNG VỀ "Còn Trống"
    ----------------------------------------------------
    DECLARE @sttGB INT;

    SELECT @sttGB = sttGB
    FROM dbo.BenhNhanNamGiuong_DaXepCho
    WHERE sttBNNG_DXC = @sttBNNG_DXC;

    IF @sttGB IS NOT NULL
    BEGIN
        UPDATE dbo.GiuongBenh
        SET tinhtrang = N'Còn Trống'
        WHERE sttGB = @sttGB;
    END;
END;
GO


-- Nếu đã tồn tại thì drop trước cho sạch
IF OBJECT_ID('dbo.sp_LichHen_HoanTatVaLuuLichSu', 'P') IS NOT NULL
    DROP PROC dbo.sp_LichHen_HoanTatVaLuuLichSu;
GO

-- Tạo wrapper trùng tên cũ, forward sang proc mới
CREATE PROC dbo.sp_LichHen_HoanTatVaLuuLichSu
    @sttLH       INT,
    @TrieuChung  NVARCHAR(MAX) = NULL,
    @ChanDoan    NVARCHAR(MAX) = NULL,
    @GhiChu      NVARCHAR(MAX) = NULL,
    @TienKham    INT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_LichHen_HoanTatKham
        @sttLH       = @sttLH,
        @TrieuChung  = @TrieuChung,
        @ChanDoan    = @ChanDoan,
        @GhiChu      = @GhiChu,
        @TienKham    = @TienKham;
END;
GO

/*========================================================
= 1. BẢNG LỊCH SỬ KHÁM 
========================================================*/
IF OBJECT_ID('dbo.LichSuKham', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LichSuKham
    (
        sttLSK     INT IDENTITY(1,1) PRIMARY KEY,
        sttBN      INT NOT NULL
            CONSTRAINT FK_LichSuKham_BenhNhan REFERENCES dbo.BenhNhan(sttBN),
        sttBS      INT NOT NULL
            CONSTRAINT FK_LichSuKham_BacSi REFERENCES dbo.BacSi(sttBS),
        NgayKham   DATETIME NOT NULL,
        TrieuChung NVARCHAR(MAX) NULL,
        ChanDoan   NVARCHAR(MAX) NULL,
        GhiChu     NVARCHAR(MAX) NULL,
        TienKham   INT NULL
    );
END;
GO

/*========================================================
= 2. HOÀN TẤT KHÁM + TRẢ DỮ LIỆU GỬI EMAIL
   (dùng cho FrmLichHen – nút "Lập phiếu khám + gửi email")
========================================================*/
CREATE OR ALTER PROC dbo.sp_LichHen_HoanTatKham
    @sttLH       INT,
    @TrieuChung  NVARCHAR(MAX) = NULL,
    @ChanDoan    NVARCHAR(MAX) = NULL,
    @GhiChu      NVARCHAR(MAX) = NULL,
    @TienKham    INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @sttBN   INT,
        @sttBS   INT,
        @NgayHen DATETIME,
        @Email   NVARCHAR(200),
        @TenBN   NVARCHAR(100),
        @TenBS   NVARCHAR(100);

    -- 1. Lấy thông tin từ lịch hẹn
    SELECT  @sttBN   = lh.sttBN,
            @sttBS   = lh.sttBS,
            @NgayHen = lh.NgayHen,
            @Email   = lh.email
    FROM    dbo.LichHen lh
    WHERE   lh.sttLH = @sttLH;

    IF @sttBN IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy lịch hẹn.', 16, 1);
        RETURN;
    END;

    SELECT @TenBN = hoten FROM dbo.BenhNhan WHERE sttBN = @sttBN;
    SELECT @TenBS = tenBS FROM dbo.BacSi    WHERE sttBS = @sttBS;

    BEGIN TRY
        BEGIN TRAN;

        -- 2. Lưu vào lịch sử khám
        INSERT INTO dbo.LichSuKham
        (
            sttBN, sttBS, NgayKham,
            TrieuChung, ChanDoan, GhiChu,
            TienKham
        )
        VALUES
        (
            @sttBN, @sttBS, @NgayHen,
            @TrieuChung, @ChanDoan, @GhiChu,
            @TienKham
        );

        -- 3. Xoá lịch hẹn sau khi khám xong
        DELETE FROM dbo.LichHen WHERE sttLH = @sttLH;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @msg NVARCHAR(4000);
        SET @msg = ERROR_MESSAGE();
        RAISERROR(@msg, 16, 1);
        RETURN;
    END CATCH;

    -- 4. Trả data cho C# dùng để gửi email
    --  (tên cột giữ nguyên giống proc cũ để code không phải sửa)
    SELECT
        @Email      AS EmailBenhNhan,
        @TenBN      AS TenBenhNhan,
        @TenBS      AS TenBacSi,
        @NgayHen    AS NgayKham,
        @TrieuChung AS TrieuChung,
        @ChanDoan   AS ChanDoan,
        @GhiChu     AS GhiChu,
        @TienKham   AS TienKham;
END;
GO

/*========================================================
= 3. THANH TOÁN BỆNH NHÂN NẰM GIƯỜNG
   (dùng cho FrmHoaDon – bệnh nhân nằm giường)
========================================================*/
CREATE OR ALTER PROC dbo.sp_ThanhToan_BenhNhanNamGiuong
    @sttBNNG_DXC INT,        -- khoá chính BenhNhanNamGiuong_DaXepCho
    @NgayRaVien  DATETIME,   -- ngày ra viện (ngày thanh toán)
    @TienDV      INT = 0     -- tiền dịch vụ khác (nếu có)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Tien1Ngay INT,
        @NgayXep   DATETIME,
        @SoNgay    INT,
        @TienGiuong INT,
        @TienThuoc INT,
        @TongTien  INT,
        @sttGB     INT;

    -- Lấy tiền giường 1 ngày
    SELECT TOP 1 @Tien1Ngay = CAST(Tien1Ngay1Giuong AS INT)
    FROM dbo.ThamSo;

    -- Lấy ngày xếp giường + giường
    SELECT @NgayXep = ngayxep,
           @sttGB   = sttGB
    FROM   dbo.BenhNhanNamGiuong_DaXepCho
    WHERE  sttBNNG_DXC = @sttBNNG_DXC;

    IF @NgayXep IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy thông tin bệnh nhân nằm giường.', 16, 1);
        RETURN;
    END;

    -- Số ngày nằm = (ngày ra - ngày xếp) + 1, tối thiểu 1
    SET @SoNgay = DATEDIFF(DAY, CONVERT(date,@NgayXep), CONVERT(date,@NgayRaVien)) + 1;
    IF @SoNgay < 1 SET @SoNgay = 1;

    SET @TienGiuong = ISNULL(@Tien1Ngay,0) * @SoNgay;

    -- Tổng tiền thuốc chưa thanh toán
    SELECT @TienThuoc = ISNULL(SUM(dt.TienThuoc),0)
    FROM   dbo.DonThuoc_BenhNhanNamGiuong_DaXepCho link
           INNER JOIN dbo.DonThuoc dt
               ON link.sttDT = dt.sttDT
    WHERE  link.MaBenhNhanNamGiuong_DaXepCho = @sttBNNG_DXC
       AND link.TinhTrang = N'Chưa thanh toán';

    SET @TienDV      = ISNULL(@TienDV,0);
    SET @TienThuoc   = ISNULL(@TienThuoc,0);
    SET @TongTien    = @TienGiuong + @TienThuoc + @TienDV;

    BEGIN TRY
        BEGIN TRAN;

        -- 1. Lưu vào bảng HoaDon
        INSERT INTO dbo.HoaDon(sttBNNG_DXC, ThanhTien)
        VALUES(@sttBNNG_DXC, @TongTien);

        -- 2. Đánh dấu các đơn thuốc là đã thanh toán
        UPDATE dbo.DonThuoc_BenhNhanNamGiuong_DaXepCho
        SET    TinhTrang = N'Đã thanh toán'
        WHERE  MaBenhNhanNamGiuong_DaXepCho = @sttBNNG_DXC
           AND TinhTrang = N'Chưa thanh toán';

        -- 3. Trả giường về trạng thái "Còn trống"
        IF @sttGB IS NOT NULL
        BEGIN
            UPDATE dbo.GiuongBenh
            SET    tinhtrang = N'Còn trống'
            WHERE  sttGB = @sttGB;
        END;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH;
END;
GO
--Update 2/12
-----------------------------------------------------
-- BƯỚC 1: XÓA TẤT CẢ UNIQUE CONSTRAINT / INDEX TRÊN username
---------------------------------------------------------
DECLARE @sql nvarchar(max) = N'';

-- Xoá UNIQUE CONSTRAINT trên username (nếu có)
SELECT @sql = @sql +
    'ALTER TABLE dbo.BacSi DROP CONSTRAINT [' + kc.name + '];' + CHAR(13)
FROM sys.key_constraints kc
JOIN sys.index_columns ic
    ON ic.object_id = kc.parent_object_id
   AND ic.index_id  = kc.unique_index_id
JOIN sys.columns c
    ON c.object_id = kc.parent_object_id
   AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.BacSi')
  AND kc.[type] = 'UQ'
  AND c.[name] = 'username';

-- Xoá UNIQUE INDEX trên username (nếu còn)
SELECT @sql = @sql +
    'DROP INDEX [' + i.name + '] ON dbo.BacSi;' + CHAR(13)
FROM sys.indexes i
JOIN sys.index_columns ic
    ON ic.object_id = i.object_id
   AND ic.index_id  = i.index_id
JOIN sys.columns c
    ON c.object_id = i.object_id
   AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('dbo.BacSi')
  AND i.is_unique = 1
  AND c.[name] = 'username';

PRINT @sql;   -- để xem nó sẽ drop những gì
EXEC (@sql);
GO

---------------------------------------------------------
-- BƯỚC 2: CHO PHÉP NULL CHO username, password
---------------------------------------------------------
ALTER TABLE dbo.BacSi
ALTER COLUMN username  VARCHAR(50) NULL;
GO

ALTER TABLE dbo.BacSi
ALTER COLUMN [password] VARCHAR(50) NULL;
GO

---------------------------------------------------------
-- BƯỚC 3: TẠO UNIQUE INDEX CHỈ CHO username KHÁC NULL
---------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_BacSi_Username_NotNull'
      AND object_id = OBJECT_ID('dbo.BacSi')
)
BEGIN
    CREATE UNIQUE INDEX IX_BacSi_Username_NotNull
    ON dbo.BacSi(username)
    WHERE username IS NOT NULL;   -- nhiều NULL vẫn OK
END
GO

