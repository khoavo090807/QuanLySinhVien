------------------------------------------------------------
-- Xóa DB cũ nếu tồn tại
------------------------------------------------------------
IF DB_ID('QLSV_DoAn_2') IS NOT NULL
BEGIN
    DROP DATABASE QLSV_DoAn_2;
END
GO

------------------------------------------------------------
-- Tạo DB
------------------------------------------------------------
CREATE DATABASE QLSV_DoAn_2
GO

USE QLSV_DoAn_2
GO

-- Bảng 1: HeDaoTao
CREATE TABLE HeDaoTao (
    MaHeDT NVARCHAR(10) PRIMARY KEY,
    TenHeDT NVARCHAR(100) NOT NULL
)

-- Bảng 2: Khoa
CREATE TABLE Khoa (
    MaKhoa NVARCHAR(10) PRIMARY KEY,
    TenKhoa NVARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15)
)

-- Bảng 3: ChucVu
CREATE TABLE ChucVu (
    MaChucVu NVARCHAR(10) PRIMARY KEY,
    TenChucVu NVARCHAR(100) NOT NULL
)

-- Bảng 4: GiangVien
CREATE TABLE GiangVien (
    MaGV NVARCHAR(10) PRIMARY KEY,
    HoTenGV NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(5) CHECK (GioiTinh IN (N'Nam', N'Nữ')),
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDT VARCHAR(15) UNIQUE,
    MaKhoa NVARCHAR(10),
    MaChucVu NVARCHAR(10),

    CONSTRAINT FK_GiangVien_Khoa FOREIGN KEY (MaKhoa) REFERENCES Khoa(MaKhoa),
    CONSTRAINT FK_GiangVien_ChucVu FOREIGN KEY (MaChucVu) REFERENCES ChucVu(MaChucVu)
)

-- Bảng 5: Lop
CREATE TABLE Lop (
    MaLop NVARCHAR(10) PRIMARY KEY,
    TenLop NVARCHAR(100) NOT NULL,
    SiSo INT DEFAULT 0,
    MaKhoa NVARCHAR(10),
    MaHeDT NVARCHAR(10),

    CONSTRAINT FK_Lop_Khoa FOREIGN KEY (MaKhoa) REFERENCES Khoa(MaKhoa),
    CONSTRAINT FK_Lop_HeDaoTao FOREIGN KEY (MaHeDT) REFERENCES HeDaoTao(MaHeDT)
)

-- Bảng 6: SinhVien
CREATE TABLE SinhVien (
    MaSV NVARCHAR(10) PRIMARY KEY,
    HoTenSV NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(5) CHECK (GioiTinh IN (N'Nam', N'Nữ')),
    DiaChi NVARCHAR(255),
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDT VARCHAR(15) UNIQUE,
    MaLop NVARCHAR(10),

    CONSTRAINT FK_SinhVien_Lop FOREIGN KEY (MaLop) REFERENCES Lop(MaLop)
)

-- Bảng 7: MonHoc
CREATE TABLE MonHoc (
    MaMH NVARCHAR(10) PRIMARY KEY,
    TenMH NVARCHAR(100) NOT NULL,
    SoTinChi INT NOT NULL CHECK (SoTinChi > 0)
)

CREATE TABLE HocKy (
    MaHK NVARCHAR(10) PRIMARY KEY,
    TenHK NVARCHAR(50) NOT NULL,
    NamHoc VARCHAR(20) NOT NULL,
    NgayBatDau DATE,
    NgayKetThuc DATE,
    UNIQUE(TenHK, NamHoc),
    CONSTRAINT CHK_HocKy_NgayThang CHECK (NgayKetThuc > NgayBatDau)
)

-- Bảng 9: LopHocPhan
CREATE TABLE LopHocPhan (
    MaLHP NVARCHAR(10) PRIMARY KEY,
    PhongHoc NVARCHAR(50),
    SoLuongToiDa INT DEFAULT 40,
    MaMH NVARCHAR(10),
    MaHK NVARCHAR(10),
    MaGV NVARCHAR(10),

    CONSTRAINT FK_LopHocPhan_MonHoc FOREIGN KEY (MaMH) REFERENCES MonHoc(MaMH),
    CONSTRAINT FK_LopHocPhan_HocKy FOREIGN KEY (MaHK) REFERENCES HocKy(MaHK),
    CONSTRAINT FK_LopHocPhan_GiangVien FOREIGN KEY (MaGV) REFERENCES GiangVien(MaGV)
)

-- Bảng 10: DangKyHocPhan
CREATE TABLE DangKyHocPhan (
    MaSV NVARCHAR(10),
    MaLHP NVARCHAR(10),
    
    NgayDangKy DATETIME DEFAULT GETDATE(),
    DiemChuyenCan FLOAT CHECK (DiemChuyenCan >= 0 AND DiemChuyenCan <= 10),
    DiemGiuaKy FLOAT CHECK (DiemGiuaKy >= 0 AND DiemGiuaKy <= 10),
    DiemCuoiKy FLOAT CHECK (DiemCuoiKy >= 0 AND DiemCuoiKy <= 10),
    DiemTongKet FLOAT,

    PRIMARY KEY (MaSV, MaLHP),
    CONSTRAINT FK_DangKyHocPhan_SinhVien FOREIGN KEY (MaSV) REFERENCES SinhVien(MaSV),
    CONSTRAINT FK_DangKyHocPhan_LopHocPhan FOREIGN KEY (MaLHP) REFERENCES LopHocPhan(MaLHP)
)
-- Bảng 11: MonHoc_TienQuyet
CREATE TABLE MonHoc_TienQuyet (
    MaMH_Chinh NVARCHAR(10),
    MaMH_TienQuyet NVARCHAR(10),
    PRIMARY KEY (MaMH_Chinh, MaMH_TienQuyet),
    CONSTRAINT FK_TienQuyet_MonHocChinh FOREIGN KEY (MaMH_Chinh) REFERENCES MonHoc(MaMH),
    CONSTRAINT FK_TienQuyet_MonHocTQ FOREIGN KEY (MaMH_TienQuyet) REFERENCES MonHoc(MaMH)
)
------------------------------------------------------------
-- DỮ LIỆU MẪU
------------------------------------------------------------
SET DATEFORMAT DMY;
GO
-- 1. HeDaoTao
INSERT INTO HeDaoTao (MaHeDT, TenHeDT) VALUES
(N'CQ', N'Chính quy'),
(N'CLC', N'Chất lượng cao'),
(N'LT', N'Liên thông'),
(N'TC', N'Tại chức')

-- 2. Khoa
INSERT INTO Khoa (MaKhoa, TenKhoa, SoDienThoai) VALUES
(N'CNTT', N'Công nghệ Thông tin', '028111222'),
(N'QTKD', N'Quản trị Kinh doanh', '028333444'),
(N'NN', N'Ngoại ngữ', '028555666'),
(N'DL', N'Du lịch & Lữ hành', '028777888'),
(N'CK', N'Cơ khí', '028999888')

-- 3. ChucVu
INSERT INTO ChucVu (MaChucVu, TenChucVu) VALUES
(N'GV', N'Giảng viên'),
(N'TK', N'Trưởng khoa'),
(N'PK', N'Phó khoa'),
(N'TG', N'Trợ giảng'),
(N'NV', N'Nhân viên')

-- 4. GiangVien
INSERT INTO GiangVien (MaGV, HoTenGV, NgaySinh, GioiTinh, Email, SoDT, MaKhoa, MaChucVu) VALUES
(N'GV001', N'Trần Văn Bảo', '1950-05-15', N'Nam', 'baotv@huit.edu.vn', '090123456', N'CNTT', N'TK'),
(N'GV002', N'Lê Thị Chi', '1985-11-20', N'Nữ', 'chilt@huit.edu.vn', '091234567', N'CNTT', N'GV'),
(N'GV003', N'Nguyễn Hùng Dũng', '1979-01-30', N'Nam', 'dungnh@huit.edu.vn', '092345678', N'QTKD', N'GV'),
(N'GV004', N'Phạm Hoài An', '1982-02-14', N'Nữ', 'anph@huit.edu.vn', '093456789', N'NN', N'GV'),
(N'GV005', N'Lý Minh Tuấn', '1975-07-25', N'Nam', 'tuanlm@huit.edu.vn', '094567890', N'DL', N'PK'),
(N'GV006', N'Võ Thanh Sơn', '1990-12-01', N'Nam', 'sonvt@huit.edu.vn', '095678901', N'CNTT', N'TG'),
(N'GV007', N'Đặng Thu Hà', '1988-06-20', N'Nữ', 'hadt@huit.edu.vn', '096789012', N'QTKD', N'GV')

-- 5. Lop
INSERT INTO Lop (MaLop, TenLop, MaKhoa, MaHeDT) VALUES
(N'14DHTH1', N'14 Đại học Tin học 1', N'CNTT', N'CQ'),
(N'15CDQT1', N'15 Cao đẳng Quản trị 1', N'QTKD', N'CQ'),
(N'14DHNN1', N'14 Đại học Ngôn ngữ 1', N'NN', N'CQ'),
(N'13CDDL1', N'13 Cao đẳng Du lịch 1', N'DL', N'LT'),
(N'15DHTH2', N'15 Đại học Tin học 2', N'CNTT', N'CLC')

-- 6. SinhVien
INSERT INTO SinhVien (MaSV, HoTenSV, NgaySinh, GioiTinh, DiaChi, Email, SoDT, MaLop) VALUES
(N'2001140001', N'Nguyễn Văn An', '2004-10-20', N'Nam', N'123 Lê Lợi, Q1, TPHCM', 'annv@huit.edu.vn', '0987654321', N'14DHTH1'),
(N'2001140002', N'Phạm Thị Em', '2004-03-15', N'Nữ', N'456 Nguyễn Trãi, Q5, TPHCM', 'empt@huit.edu.vn', '0987123456', N'14DHTH1'),
(N'2001150001', N'Hoàng Văn Phúc', '2003-07-07', N'Nam', N'789 CMT8, Q10, TPHCM', 'phuchv@huit.edu.vn', '0977123456', N'15CDQT1'),
(N'2001140003', N'Trần Minh Khang', '2004-08-30', N'Nam', N'111 Phan Đăng Lưu, Phú Nhuận, TPHCM', 'khangtm@huit.edu.vn', '0966554433', N'14DHTH1'),
(N'2001141001', N'Lê Nguyễn Bảo Châu', '2004-01-15', N'Nữ', N'222 Võ Văn Tần, Q3, TPHCM', 'chaulnb@huit.edu.vn', '0977889900', N'14DHNN1'),
(N'2001130001', N'Đỗ Anh Dũng', '2002-06-05', N'Nam', N'333 Lũy Bán Bích, Tân Phú, TPHCM', 'dungda@huit.edu.vn', '0911223344', N'13CDDL1'),
(N'2001150002', N'Mai Thị Lan Anh', '2003-11-10', N'Nữ', N'444 Cộng Hòa, Tân Bình, TPHCM', 'anhmtl@huit.edu.vn', '0922334455', N'15CDQT1'),
(N'2001150101', N'Vũ Hoàng Long', '2005-04-25', N'Nam', N'555 Nguyễn Kiệm, Gò Vấp, TPHCM', 'longvh@huit.edu.vn', '0933445566', N'15DHTH2')

-- 7. MonHoc
INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES
(N'CSDL', N'Cơ sở dữ liệu', 3),
(N'HDT', N'Hướng đối tượng', 3),
(N'MKT', N'Marketing căn bản', 2),
(N'LTW', N'Lập trình Web', 3),
(N'MMT', N'Mạng máy tính', 3),
(N'TA-C1', N'Tiếng Anh C1', 4),
(N'QTLH', N'Quản trị lữ hành', 2),
(N'KTCB', N'Kế toán căn bản', 2)

-- 8. HocKy
INSERT INTO HocKy (MaHK, TenHK, NamHoc, NgayBatDau, NgayKetThuc) VALUES
(N'HK1-2324', N'Học kỳ 1', '2023-2024', '05/09/2023', '10/01/2024'),
(N'HK2-2324', N'Học kỳ 2', '2023-2024', '20/01/2024', '01/06/2024'),
(N'HK1-2425', N'Học kỳ 1', '2024-2025', '05/09/2024', '10/01/2025'),
(N'HK2-2425', N'Học kỳ 2', '2024-2025', '20/01/2025', '01/06/2025')

-- 9. LopHocPhan
INSERT INTO LopHocPhan (MaLHP, MaMH, MaHK, MaGV, PhongHoc) VALUES
(N'LHP01', N'CSDL', N'HK1-2425', N'GV001', 'A101'),
(N'LHP02', N'HDT', N'HK1-2425', N'GV002', 'A102'),
(N'LHP03', N'MKT', N'HK1-2425', N'GV003', 'B201'),
(N'LHP04', N'LTW', N'HK1-2425', N'GV002', 'A103'),
(N'LHP05', N'TA-C1', N'HK1-2425', N'GV004', 'C201'),
(N'LHP06', N'QTLH', N'HK1-2425', N'GV005', 'D105'),
(N'LHP07', N'KTCB', N'HK1-2425', N'GV007', 'B202'),
(N'LHP08', N'CSDL', N'HK2-2324', N'GV001', 'A101')

-- 10. DangKyHocPhan
INSERT INTO DangKyHocPhan (MaSV, MaLHP, NgayDangKy, DiemChuyenCan, DiemGiuaKy, DiemCuoiKy) VALUES
(N'2001140001', N'LHP01', '15/08/2024 08:30:00', 9.0, 8.0, 7.5),
(N'2001140001', N'LHP02', '15/08/2024 08:31:00', 10.0, 8.5, 9.0),
(N'2001140002', N'LHP01', '15/08/2024 09:00:00', 9.5, 8.5, 8.0),
(N'2001150001', N'LHP03', '16/08/2024 10:00:00', 8.0, 7.0, 6.5),
(N'2001140003', N'LHP01', '15/08/2024 09:05:00', 8.0, 7.5, 7.0),
(N'2001140003', N'LHP04', '15/08/2024 09:06:00', 9.0, 9.0, 9.5),
(N'2001141001', N'LHP05', '17/08/2024 11:00:00', 10.0, 9.0, 8.5),
(N'2001130001', N'LHP06', '17/08/2024 11:30:00', 7.0, 8.0, 6.0),
(N'2001150002', N'LHP03', '16/08/2024 10:05:00', 8.5, 8.0, 7.5),
(N'2001150002', N'LHP07', '16/08/2024 10:06:00', 9.0, 7.0, 8.0),
(N'2001140001', N'LHP08', '15/01/2024 14:00:00', 8.5, 8.0, 9.0)
-- 11. MonHoc_TienQuyet
INSERT INTO MonHoc_TienQuyet (MaMH_Chinh, MaMH_TienQuyet) VALUES
(N'LTW', N'HDT'),  -- Lập trình Web yêu cầu Hướng đối tượng
(N'LTW', N'CSDL'), -- Lập trình Web cũng yêu cầu Cơ sở dữ liệu
(N'MMT', N'CSDL'), -- Mạng máy tính yêu cầu Cơ sở dữ liệu
(N'QTLH', N'MKT'); -- Quản trị lữ hành yêu cầu Marketing căn bản
PRINT N'Tạo CSDL QLSV_DoAn_2 thành công!'
GO

SELECT * FROM HeDaoTao
SELECT * FROM Khoa
SELECT * FROM ChucVu
SELECT * FROM GiangVien
SELECT * FROM Lop
SELECT * FROM SinhVien
SELECT * FROM MonHoc
SELECT * FROM HocKy
SELECT * FROM LopHocPhan
SELECT * FROM DangKyHocPhan
SELECT * FROM MonHoc_TienQuyet
---------------------------------//////vo anh khoa//////////---------------------
-- SP_CAPNHAT_CHUCVU_GV
-- Mục đích: Cập nhật chức vụ mới cho một giảng viên

CREATE PROCEDURE SP_CAPNHAT_CHUCVU_GV
    @MaGV NVARCHAR(10),
    @MaChucVuMoi NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra tồn tại Giảng viên
    IF NOT EXISTS (SELECT 1 FROM GiangVien WHERE MaGV = @MaGV)
    BEGIN
        RAISERROR(N'Lỗi: Mã Giảng viên không tồn tại.', 16, 1);
        RETURN;
    END

    -- Kiểm tra tồn tại Chức vụ mới
    IF NOT EXISTS (SELECT 1 FROM ChucVu WHERE MaChucVu = @MaChucVuMoi)
    BEGIN
        RAISERROR(N'Lỗi: Mã Chức vụ mới không hợp lệ.', 16, 1);
        RETURN;
    END

    -- Cập nhật
    UPDATE GiangVien
    SET MaChucVu = @MaChucVuMoi
    WHERE MaGV = @MaGV;

    PRINT N'Cập nhật chức vụ thành công cho Giảng viên ' + @MaGV;
END
GO


-- Ví dụ chạy SP: Chuyển GV003 từ Giảng viên (GV) thành Trưởng khoa (TK)
select* from GiangVien
EXEC SP_CAPNHAT_CHUCVU_GV N'GV003', N'TK';
SELECT MaGV, HoTenGV, MaChucVu FROM GiangVien WHERE MaGV = N'GV003';

-- FN_DEM_GV_THEO_CHUCVU
-- Mục đích: Trả về số lượng giảng viên giữ một chức vụ cụ thể
CREATE FUNCTION FN_DEM_GV_THEO_CHUCVU (@MaChucVu NVARCHAR(10))
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(MaGV)
    FROM GiangVien
    WHERE MaChucVu = @MaChucVu;

    RETURN @SoLuong;
END
GO

-- Ví dụ chạy Function: Đếm số lượng Giảng viên (GV)
select* from GiangVien
SELECT N'Số lượng Giảng viên (GV) hiện tại: ' + CAST(dbo.FN_DEM_GV_THEO_CHUCVU(N'GV') AS NVARCHAR) AS KetQua;


-- TR_NGAN XOA CHUCVU
CREATE TRIGGER TR_NGAN_CHAN_XOA_CHUCVU
ON ChucVu
INSTEAD OF DELETE -- Sử dụng INSTEAD OF để thay thế hành động DELETE
AS
BEGIN
    SET NOCOUNT ON;    -- Kiểm tra xem có Mã Chức vụ nào trong tập "deleted" (đang định xóa) mà vẫn tồn tại trong GiangVien không
    IF EXISTS (
        SELECT 1
        FROM deleted d
        INNER JOIN GiangVien g ON d.MaChucVu = g.MaChucVu
    )
    BEGIN
        -- Báo lỗi và ROLLBACK TRANSACTION
        RAISERROR(N'Lỗi: Không thể xóa Chức vụ. Vẫn còn Giảng viên được gán với Chức vụ này.', 16, 1);
        RETURN; -- Ngăn hành động xóa xảy ra
    END
    ELSE
    BEGIN
        -- Nếu không có giảng viên nào sử dụng, cho phép xóa
        DELETE FROM ChucVu
        WHERE MaChucVu IN (SELECT MaChucVu FROM deleted);
    END
END
GO



-- Ví dụ chạy Trigger:

-- Thử xóa chức vụ 'GV' (đang được sử dụng)

select* from GiangVien

DELETE FROM ChucVu WHERE MaChucVu = N'GV'; -- Sẽ báo lỗi
DELETE FROM ChucVu WHERE MaChucVu = N'HS';--- thành công

-- CS_THONG_BAO_NGHI_HUU
-- Mục đích: Duyệt qua danh sách giảng viên và thông báo nếu họ trên 60 tuổi

SELECT * FROM GiangVien WHERE MaGV='GV001'
DECLARE @MaGV NVARCHAR(10);
DECLARE @HoTenGV NVARCHAR(100);
DECLARE @NgaySinh DATE;
DECLARE @Tuoi INT;
DECLARE @NamHienTai INT = YEAR(GETDATE());

-- Khai báo Cursor: Chọn các Giảng viên có ngày sinh trước năm hiện tại - 60
DECLARE GV_CURSOR CURSOR FOR
    SELECT MaGV, HoTenGV, NgaySinh
    FROM GiangVien
    WHERE NgaySinh IS NOT NULL
    AND YEAR(NgaySinh) <= (@NamHienTai - 60);

-- Mở CursorOPEN GV_CURSOR;
OPEN GV_CURSOR;
-- Lấy dòng đầu tiên
FETCH NEXT FROM GV_CURSOR INTO @MaGV, @HoTenGV, @NgaySinh;

PRINT N'--- DANH SÁCH GV ĐÃ HOẶC SẮP ĐẾN TUỔI NGHỈ HƯU (>= 60 TUỔI) ---';

-- Bắt đầu vòng lặp
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Tính tuổi (ước tính)
    SET @Tuoi = DATEDIFF(year, @NgaySinh, GETDATE()) -
                CASE
                    WHEN (MONTH(@NgaySinh) > MONTH(GETDATE())) OR
                         (MONTH(@NgaySinh) = MONTH(GETDATE()) AND DAY(@NgaySinh) > DAY(GETDATE()))
                    THEN 1
                    ELSE 0
                END;

    PRINT N'Mã GV: ' + @MaGV + N' | Tên: ' + @HoTenGV + N' | Tuổi: ' + CAST(@Tuoi AS NVARCHAR) + N' | Cần xem xét hồ sơ nghỉ hưu.';

    -- Lấy dòng tiếp theo
    FETCH NEXT FROM GV_CURSOR INTO @MaGV, @HoTenGV, @NgaySinh;
END

-- Đóng và hủy Cursor
CLOSE GV_CURSOR;
DEALLOCATE GV_CURSOR;



GO



-- TX_THANG_CHUC_TRUONG_KHOA
-- Mục đích: Chuyển GV X thành Trưởng khoa (TK) và đảm bảo GV Trưởng khoa cũ (nếu có) trở lại thành Giảng viên (GV).
CREATE PROCEDURE SP_THANG_CHUC_TRUONG_KHOA
    @MaGVThangChuc NVARCHAR(10),
    @MaKhoa NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    -- Bắt đầu giao dịch
    BEGIN TRANSACTION;
    
    -- Biến lưu trữ kết quả
    DECLARE @ErrorState INT = 0;

    -- 1. Tìm Trưởng khoa cũ của khoa đó (nếu có)
    DECLARE @MaGVCu NVARCHAR(10);
    SELECT @MaGVCu = MaGV
    FROM GiangVien
    WHERE MaKhoa = @MaKhoa AND MaChucVu = N'TK';

    -- 2. Đảm bảo GV thăng chức thuộc Khoa đó và tồn tại
    IF NOT EXISTS (SELECT 1 FROM GiangVien WHERE MaGV = @MaGVThangChuc AND MaKhoa = @MaKhoa)
    BEGIN
        SET @ErrorState = 1;
    END

    -- 3. Hạ chức Trưởng khoa cũ thành Giảng viên thường (GV)
    IF @MaGVCu IS NOT NULL
    BEGIN
        UPDATE GiangVien
        SET MaChucVu = N'GV'
        WHERE MaGV = @MaGVCu;
    END

    -- 4. Thăng chức Giảng viên mới thành Trưởng khoa (TK)
    IF @ErrorState = 0
    BEGIN
        UPDATE GiangVien
        SET MaChucVu = N'TK'
        WHERE MaGV = @MaGVThangChuc;
    END

    -- 5. Xử lý kết quả giao dịch
    IF @ErrorState = 1 OR @@ERROR <> 0 -- Nếu có lỗi trong quá trình thực thi
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'Giao dịch Thăng chức không thành công. Mã GV mới không hợp lệ hoặc không thuộc Khoa được chỉ định.', 16, 1);
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        PRINT N'Thăng chức thành công! GV ' + @MaGVThangChuc + N' là Trưởng khoa mới của Khoa ' + @MaKhoa;
        
        IF @MaGVCu IS NOT NULL
        BEGIN
            PRINT N'GV ' + @MaGVCu + N' (Trưởng khoa cũ) đã được chuyển về chức vụ Giảng viên.';
        END
    END
END
GO



-- Ví dụ chạy Transaction: Thăng chức GV002 thành Trưởng khoa CNTT. (GV001 là TK cũ)
select * from GiangVien
EXEC SP_THANG_CHUC_TRUONG_KHOA N'GV002', N'CNTT';
SELECT MaGV, HoTenGV, MaChucVu FROM GiangVien WHERE MaKhoa = N'CNTT';

-------------------------------------------------------Nguyễn Gia Bảo-----------------------------

CREATE TABLE LoiHeThong (
    ID INT IDENTITY PRIMARY KEY,
    NoiDung NVARCHAR(255),
    ThoiGian DATETIME DEFAULT GETDATE()
)

GO
-- Procedure thêm khoa
CREATE PROCEDURE sp_ThemKhoa
    @MaKhoa NVARCHAR(10),
    @TenKhoa NVARCHAR(100),
    @SoDienThoai VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Khoa WHERE TenKhoa = @TenKhoa)
    BEGIN
        INSERT INTO LoiHeThong (NoiDung)
        VALUES (N'Trùng tên khoa: ' + @TenKhoa);
        PRINT N'Tên khoa đã tồn tại. Không thể thêm mới.';
        RETURN;
    END

    INSERT INTO Khoa (MaKhoa, TenKhoa, SoDienThoai)
    VALUES (@MaKhoa, @TenKhoa, @SoDienThoai);

    PRINT N'Thêm khoa thành công!';
END;
GO
-- Ví dụ 
select * from Khoa
EXEC sp_ThemKhoa 'CNTT', N'Công nghệ thông tin', '0912345678' --Tên khoa đã tồn tại không thể thêm

select * from Khoa
EXEC sp_ThemKhoa 'KT', N'Kế toán', '0987654321' --Thêm thành công
select * from Khoa

-- 2. Function: Đếm số lượng khoa có số điện thoại
-- Trả về số lượng khoa có thông tin SoDienThoai (không NULL)
CREATE FUNCTION fn_DemKhoaCoSoDienThoai()
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;
    SELECT @SoLuong = COUNT(*) FROM Khoa WHERE SoDienThoai IS NOT NULL;
    RETURN @SoLuong;
END
-- chạy thử
select * from Khoa
SELECT dbo.fn_DemKhoaCoSoDienThoai() AS SoKhoaCoSoDienThoai

-- 3. Trigger: Kiểm tra số điện thoại khi thêm hoặc cập nhật khoa
-- Khi thêm hoặc cập nhật Khoa, kiểm tra SoDienThoai phải đúng định dạng 10 số (bắt đầu bằng 0)
-- Nếu sai -> hủy thao tác.
CREATE TRIGGER trg_KiemTraSoDienThoai
ON Khoa
FOR INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT * FROM inserted
        WHERE SoDienThoai NOT LIKE '0%' OR LEN(SoDienThoai) <> 10
    )
    BEGIN
        RAISERROR(N'Số điện thoại không hợp lệ! Phải có 10 chữ số và bắt đầu bằng 0.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
-- ví dụ
select * from Khoa
INSERT INTO Khoa (MaKhoa, TenKhoa, SoDienThoai)
VALUES (N'TT', N'Thống kê', '0912345678'); -- thêm thành công
select * from Khoa


INSERT INTO Khoa (MaKhoa, TenKhoa, SoDienThoai)
VALUES (N'TM', N'Thương mại', '812345678'); -- không thêm được
select * from Khoa
--delete from Khoa
--where MaKhoa=N'TM'
-- 4. CURSOR Duyệt từng hệ đào tạo và hiển thị thông tin
CREATE PROCEDURE sp_DuyetTungHeDaoTao
AS
BEGIN
    DECLARE @MaHe NVARCHAR(10), @Ten NVARCHAR(100)

    DECLARE cur_HeDaoTao CURSOR FOR
    SELECT MaHeDT, TenHeDT FROM HeDaoTao

    OPEN cur_HeDaoTao
    FETCH NEXT FROM cur_HeDaoTao INTO @MaHe, @Ten

    WHILE @@FETCH_STATUS = 0
    BEGIN
		PRINT N'Mã: ' + @MaHe + N' | Tên hệ đào tạo: ' + @Ten
        FETCH NEXT FROM cur_HeDaoTao INTO @MaHe, @Ten
    END

    CLOSE cur_HeDaoTao
    DEALLOCATE cur_HeDaoTao
END;
-- chạy thử
SELECT * FROM HeDaoTao
EXEC sp_DuyetTungHeDaoTao;

-- 5. TRANSACTION – Cập nhật tên hệ đào tạo có kiểm soát lỗi
select* from HeDaoTao
BEGIN TRANSACTION;
BEGIN TRY
    -- Kiểm tra xem mã hệ đào tạo cần cập nhật có tồn tại không
    IF EXISTS (SELECT * FROM HeDaoTao WHERE MaHeDT = N'LT')
    BEGIN
        UPDATE HeDaoTao
        SET TenHeDT = N'Liên thông từ Cao đẳng'
        WHERE MaHeDT = N'LT';

        PRINT N'Cập nhật thành công!';
        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        PRINT N'Mã hệ đào tạo không tồn tại. Hủy giao dịch.';
        ROLLBACK TRANSACTION;
    END
END TRY

BEGIN CATCH
    PRINT N'Lỗi xảy ra, giao dịch bị hủy.';
    PRINT N'Thông tin lỗi: ' + ERROR_MESSAGE();
    ROLLBACK TRANSACTION;
END CATCH;
-- ví dụ
SELECT * FROM HeDaoTao;


-------------------------//////Nguyễn Viết An Bình/////---------

-- Procedure đăng ký học phần
CREATE OR ALTER PROCEDURE sp_DangKyHocPhan
    @MaSV NVARCHAR(10),
    @MaLHP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM LopHocPhan WHERE MaLHP = @MaLHP)
    BEGIN
        PRINT N'Lớp học phần không tồn tại!';
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP)
    BEGIN
        PRINT N'Sinh viên đã đăng ký lớp này!';
        RETURN;
    END;

    DECLARE @SL INT = (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = @MaLHP);
    DECLARE @ToiDa INT = (SELECT SoLuongToiDa FROM LopHocPhan WHERE MaLHP = @MaLHP);

    IF @SL >= @ToiDa
    BEGIN
        PRINT N'Lớp học phần đã đầy!';
        RETURN;
    END;

    INSERT INTO DangKyHocPhan (MaSV, MaLHP)
    VALUES (@MaSV, @MaLHP);

    PRINT N'Đăng ký thành công!';
END;
GO
-- 🎯 Ví dụ chạy procedure
select * from DangKyHocPhan
order by MaSV
select * from LopHocPhan
EXEC sp_DangKyHocPhan @MaSV = '2001130001', @MaLHP = 'LHP04';
SELECT * FROM DangKyHocPhan WHERE MaSV = '2001130001';

-- Trigger tự tính điểm tổng kết của 1 sv
CREATE OR ALTER TRIGGER trg_TinhDiemTongKet
ON DangKyHocPhan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE D
    SET D.DiemTongKet = ROUND(
        (ISNULL(D.DiemChuyenCan, 0)*0.1 +
         ISNULL(D.DiemGiuaKy, 0)*0.3 +
         ISNULL(D.DiemCuoiKy, 0)*0.6), 2)
    FROM DangKyHocPhan D
    JOIN inserted I
        ON D.MaSV = I.MaSV AND D.MaLHP = I.MaLHP;
END;
SELECT* FROM DangKyHocPhan
ORDER BY MaSV
UPDATE DangKyHocPhan
SET DiemChuyenCan = 5, DiemGiuaKy = 6, DiemCuoiKy = 7
WHERE MaSV = '2001150002' AND MaLHP = 'LHP03';

SELECT * FROM DangKyHocPhan WHERE MaSV = '2001150002';

-- Function tính điểm trung bình của tất cả sv

CREATE OR ALTER FUNCTION fn_TinhDiemTrungBinh (@MaSV NVARCHAR(10))
RETURNS FLOAT
AS
BEGIN
    DECLARE @DTB FLOAT;
    
    SELECT @DTB = AVG(DiemTongKet)
    FROM DangKyHocPhan
    WHERE MaSV = @MaSV
      -- Đây là điều kiện quan trọng: Chỉ tính trung bình các môn đã có điểm tổng kết (ĐÃ HOÀN THÀNH)
      AND DiemTongKet IS NOT NULL;
    
    RETURN @DTB;
END;
GO



UPDATE DangKyHocPhan
SET DiemTongKet = DiemTongKet  -- Cập nhật chính nó để kích hoạt Trigger
WHERE DiemChuyenCan IS NOT NULL 
  AND DiemGiuaKy IS NOT NULL 
  AND DiemCuoiKy IS NOT NULL
  AND DiemTongKet IS NULL;
GO
select * from DangKyHocPhan
order by MaSV
SELECT dbo.fn_TinhDiemTrungBinh('2001140003') AS DiemTrungBinh;


-- Cursor in điểm trung bình từng môn của sinh viên được cập nhật ở sp_dangkyhocphan
SELECT * FROM DangKyHocPhan
DECLARE @MaSV NVARCHAR(10);
DECLARE @DiemTB FLOAT;

DECLARE cur_SV CURSOR FOR
SELECT DISTINCT MaSV FROM DangKyHocPhan;

OPEN cur_SV;
FETCH NEXT FROM cur_SV INTO @MaSV;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @DiemTB = dbo.fn_TinhDiemTrungBinh(@MaSV);
    PRINT N'Sinh viên ' + @MaSV + N' có điểm trung bình: ' + CAST(@DiemTB AS NVARCHAR(10));
    FETCH NEXT FROM cur_SV INTO @MaSV;
END;

CLOSE cur_SV;
DEALLOCATE cur_SV;



-- Transaction gộp đăng ký + cập nhật điểm của 1 sv
BEGIN TRY
    BEGIN TRANSACTION;

    EXEC sp_DangKyHocPhan @MaSV = '2001140001', @MaLHP = 'LHP01';

    UPDATE DangKyHocPhan
    SET DiemChuyenCan = 9, DiemGiuaKy = 8, DiemCuoiKy = 9
    WHERE MaSV = '2001140001' AND MaLHP = 'LHP01';

    COMMIT TRANSACTION;
    PRINT N'Cập nhật thành công!';
END TRY
BEGIN CATCH
    PRINT N'Lỗi: ' + ERROR_MESSAGE();
    ROLLBACK TRANSACTION;
END CATCH;

select * from DangKyHocPhan
order by MaSV
SELECT * FROM DangKyHocPhan WHERE MaSV = '2001140001';
--------------------------//////// Mã Nhật Phong////////----------------


CREATE PROCEDURE sp_LayDanhSachSV_TheoLop
    @MaLop NVARCHAR(15)
AS
BEGIN
    SELECT 
        MaSV, 
        HoTenSV, 
        NgaySinh, 
        GioiTinh, 
        DiaChi, 
        Email, 
        SoDT
    FROM SinhVien
    WHERE MaLop = @MaLop
    ORDER BY HoTenSV;
END;
GO
SELECT* FROM SinhVien
ORDER BY MaLop
EXEC sp_LayDanhSachSV_TheoLop N'14DHTH1'
GO



/*
-----------------------------------------------------------------
-- PHẦN 4: FUNCTIONS (HÀM)
-----------------------------------------------------------------
*/

-- 1. Hàm vô hướng (Scalar Function) đếm số sinh viên trong một lớp
CREATE FUNCTION fn_DemSoSinhVien_TheoLop
(
    @MaLop NVARCHAR(15)
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;
    
    SELECT @SoLuong = COUNT(*)
    FROM SinhVien
    WHERE MaLop = @MaLop;
    
    RETURN @SoLuong;
END;
GO

SELECT * FROM SinhVien
ORDER BY MaLop
SELECT dbo.fn_DemSoSinhVien_TheoLop(N'15CDQT1') AS SiSo;
GO

-- 2. Hàm bảng (Table-Valued Function) trả về danh sách sinh viên của một Khoa
CREATE FUNCTION fn_LaySinhVien_TheoKhoa
(
    @MaKhoa NVARCHAR(10)
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        sv.MaSV, 
        sv.HoTenSV, 
        sv.NgaySinh, 
        sv.GioiTinh, 
        l.TenLop
    FROM SinhVien AS sv
    JOIN Lop AS l ON sv.MaLop = l.MaLop
    WHERE l.MaKhoa = @MaKhoa
);
GO

SELECT * FROM dbo.fn_LaySinhVien_TheoKhoa(N'CNTT');
GO

/*
-----------------------------------------------------------------
-- PHẦN 5: TRIGGERS (TRÌNH KÍCH HOẠT)
-----------------------------------------------------------------
*/




-- 1. Trigger ngăn không cho xóa Lớp nếu lớp đó vẫn còn sinh viên
CREATE TRIGGER trg_NganXoaLop_KhiConSinhVien
ON Lop
INSTEAD OF DELETE
AS
BEGIN
    DECLARE @MaLopBiXoa NVARCHAR(15);
    
    -- Lấy mã lớp từ bảng 'deleted' (bảng ảo chứa các dòng bị xóa)
    SELECT @MaLopBiXoa = MaLop FROM deleted;

    -- Kiểm tra xem lớp này còn sinh viên không
    IF EXISTS (SELECT 1 FROM SinhVien WHERE MaLop = @MaLopBiXoa)
    BEGIN
        -- Nếu còn SV, thông báo lỗi và không thực hiện xóa
        RAISERROR (N'Không thể xóa lớp này vì vẫn còn sinh viên. Vui lòng chuyển sinh viên sang lớp khác trước.', 16, 1);
    END
    ELSE
    BEGIN
        -- Nếu không còn SV, tiến hành xóa lớp
        DELETE FROM Lop
        WHERE MaLop = @MaLopBiXoa;
    END;
END;
GO
SELECT * FROM SinhVien
ORDER BY MaLop
DELETE FROM Lop WHERE MaLop = N'14DHTH1';
GO


/*
-----------------------------------------------------------------
-- PHẦN 6: CURSOR (CON TRỎ)
-- Ví dụ sử dụng Cursor để duyệt và in danh sách sinh viên
-----------------------------------------------------------------
*/

CREATE PROCEDURE sp_DuyetSinhVien_BangCursor
    @MaLop NVARCHAR(15)
AS
BEGIN
    -- 1. Khai báo biến để giữ dữ liệu từ cursor
    DECLARE @MaSV_Current NVARCHAR(15);
    DECLARE @HoTen_Current NVARCHAR(100);

    PRINT N'--- Danh sách sinh viên của lớp ' + @MaLop + ' ---';

    -- 2. Khai báo Cursor
    DECLARE sv_cursor CURSOR FOR
    SELECT MaSV, HoTenSV
    FROM SinhVien
    WHERE MaLop = @MaLop
    ORDER BY HoTenSV;

    -- 3. Mở Cursor
    OPEN sv_cursor;

    -- 4. Lấy dòng đầu tiên
    FETCH NEXT FROM sv_cursor INTO @MaSV_Current, @HoTen_Current;

    -- 5. Bắt đầu vòng lặp duyệt (khi @@FETCH_STATUS = 0 là còn dữ liệu)
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- In thông tin
        PRINT N'Mã SV: ' + @MaSV_Current + N' | Tên: ' + @HoTen_Current;

        -- 6. Lấy dòng tiếp theo
        FETCH NEXT FROM sv_cursor INTO @MaSV_Current, @HoTen_Current;
    END;
    
    PRINT N'--- Kết thúc danh sách ---';

    -- 7. Đóng và Hủy Cursor
    CLOSE sv_cursor;
    DEALLOCATE sv_cursor;
END;
GO
select * from SinhVien
order by MaSV
EXEC sp_DuyetSinhVien_BangCursor N'14DHTH1'
GO

/*
-----------------------------------------------------------------
-- PHẦN 7: TRANSACTION (GIAO DỊCH)
-- Ví dụ về Transaction: Chuyển một sinh viên sang lớp mới.
-- Phải đảm bảo việc chuyển thành công, nếu có lỗi thì hủy bỏ.
-----------------------------------------------------------------
*/

CREATE PROCEDURE sp_ChuyenLopChoSinhVien
    @MaSV NVARCHAR(15),
    @MaLopMoi NVARCHAR(15)
AS
BEGIN
    -- Bắt đầu một giao dịch
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Kiểm tra sinh viên có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM SinhVien WHERE MaSV = @MaSV)
        BEGIN
            RAISERROR(N'Không tìm thấy sinh viên.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        -- Kiểm tra lớp mới có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM Lop WHERE MaLop = @MaLopMoi)
        BEGIN
            RAISERROR(N'Không tìm thấy lớp mới.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        -- Cập nhật lớp mới cho sinh viên
        UPDATE SinhVien
        SET MaLop = @MaLopMoi
        WHERE MaSV = @MaSV;

        -- (Giả sử có bảng Lop và có cột SiSo, ta sẽ cập nhật lại sỉ số)
        -- UPDATE Lop SET SiSo = SiSo - 1 WHERE MaLop = @MaLopCu;
        -- UPDATE Lop SET SiSo = SiSo + 1 WHERE MaLop = @MaLopMoi;

        -- Nếu mọi thứ thành công, xác nhận giao dịch
        COMMIT TRANSACTION;
        PRINT N'Chuyển lớp cho sinh viên thành công.';

    END TRY
    BEGIN CATCH
        -- Nếu có lỗi xảy ra, hủy bỏ tất cả thay đổi
        ROLLBACK TRANSACTION;
        
        -- In thông báo lỗi
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(N'Đã xảy ra lỗi khi chuyển lớp: %s', 16, 1, @ErrorMessage);
    END CATCH;
END;
GO
-- Ví dụ cách gọi: 
SELECT * FROM SinhVien
ORDER BY MaSV
EXEC sp_ChuyenLopChoSinhVien N'2001150101', N'14DHTH1'; -- Chuyển 'Vũ Hoàng Long' sang lớp '14DHTH1'
SELECT * FROM SinhVien WHERE MaSV = N'2001150101';
GO

-----------------////Nguyễn Hửu Hoàng Thông////////////
/*
------------------------------------------------------------
* Tên hàm: sp_XoaMonHoc
------------------------------------------------------------
*/
CREATE PROCEDURE sp_XoaMonHoc @MaMH NVARCHAR(10)
AS
BEGIN
    -- 1. Kiểm tra xem môn này có đang được LỚP HỌC PHẦN nào sử dụng không?
    IF EXISTS (SELECT * FROM LopHocPhan WHERE MaMH = @MaMH)
    BEGIN
        PRINT N'LỖI: Không thể xóa môn [' + @MaMH + N']. Môn học này đã được mở lớp học phần.';
        RETURN;
    END
    -- 2. Kiểm tra xem môn này có đang là MÔN TIÊN QUYẾT cho môn khác không?
    IF EXISTS (SELECT * FROM MonHoc_TienQuyet WHERE MaMH_TienQuyet = @MaMH)
    BEGIN
        PRINT N'LỖI: Không thể xóa môn [' + @MaMH + N']. Môn học này đang là tiên quyết cho môn khác.';
        RETURN;
    END

    -- Nếu vượt qua cả 2 kiểm tra trên, môn học này có thể xóa

    -- 3. Xóa môn học khỏi bảng MonHoc_TienQuyet:
    DELETE FROM MonHoc_TienQuyet
    WHERE MaMH_Chinh = @MaMH;

    -- 4. Xóa môn học khỏi bảng chính (Bảng MonHoc)
    DELETE FROM MonHoc
    WHERE MaMH = @MaMH;
END
GO

-- Chạy thử sp_XoaMonHoc
EXEC sp_XoaMonHoc @MaMH = N'MMT'; -- Xóa thành công
-- Hiển thị kết quả sau sp_XoaMonHoc
SELECT * FROM MonHoc_TienQuyet
SELECT * FROM MonHoc

EXEC sp_XoaMonHoc @MaMH = N'HDT'; -- Báo lỗi vì nó là tiên quyết của LTW
EXEC sp_XoaMonHoc @MaMH = N'CSDL'; -- Báo lỗi vì đã được mở lớp LHP01, LHP08
GO

/*
------------------------------------------------------------
* Tên hàm: sp_ThemMonHoc
------------------------------------------------------------
*/
CREATE PROCEDURE sp_ThemMonHoc
    @MaMH NVARCHAR(10),
    @TenMH NVARCHAR(100),
    @SoTinChi INT,
    @MaMHTienQuyet NVARCHAR(10) = NULL
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Kiểm tra mã môn học đã tồn tại
        IF EXISTS (SELECT 1 FROM MonHoc WHERE MaMH = @MaMH)
            RAISERROR(N'Mã môn học [%s] đã tồn tại.', 16, 1, @MaMH);
        
        -- Kiểm tra số tín chỉ hợp lệ
        IF @SoTinChi < 1 OR @SoTinChi > 6
            RAISERROR(N'Số tín chỉ phải từ 1-6.', 16, 1);
        
        -- Kiểm tra tên môn học không rỗng
        IF @TenMH IS NULL OR LTRIM(RTRIM(@TenMH)) = ''
            RAISERROR(N'Tên môn học không được để trống.', 16, 1);
        
        -- Kiểm tra môn tiên quyết có tồn tại hay không
        IF @MaMHTienQuyet IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MonHoc WHERE MaMH = @MaMHTienQuyet)
            RAISERROR(N'Môn tiên quyết [%s] không tồn tại.', 16, 1, @MaMHTienQuyet);
        
        -- Thêm môn học vào bảng MonHoc
        INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES (@MaMH, @TenMH, @SoTinChi);
        
        -- Thêm môn tiên quyết nếu có
        IF @MaMHTienQuyet IS NOT NULL
            INSERT INTO MonHoc_TienQuyet (MaMH_Chinh, MaMH_TienQuyet) VALUES (@MaMH, @MaMHTienQuyet);
        
        COMMIT TRANSACTION;
        PRINT N'Thêm môn học thành công!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Chạy thử transaction
EXEC sp_ThemMonHoc @MaMH = N'CSDL', @TenMH = N'Cơ sở dữ liệu', @SoTinChi = 3; -- Lỗi: Mã đã tồn tại
EXEC sp_ThemMonHoc @MaMH = N'BLOCKCHAIN', @TenMH = N'Công nghệ Blockchain', @SoTinChi = 3, @MaMHTienQuyet = N'CSDL'; -- Thành công
EXEC sp_ThemMonHoc @MaMH = N'IOT', @TenMH = N'Internet of Things', @SoTinChi = 3, @MaMHTienQuyet = N'XYZ'; -- Lỗi: Môn tiên quyết không tồn tại
SELECT * FROM MonHoc
SELECT * FROM MonHoc_TienQuyet
GO

/*
------------------------------------------------------------
* Tên hàm: sp_CapNhatMonHoc
------------------------------------------------------------
*/
CREATE PROCEDURE sp_CapNhatMonHoc
    @MaMH NVARCHAR(10),
    @TenMH NVARCHAR(100),
    @SoTinChi INT
AS
BEGIN
    -- 1. Kiểm tra môn học có tồn tại không
    IF NOT EXISTS (SELECT * FROM MonHoc WHERE MaMH = @MaMH)
    BEGIN
        RAISERROR(N'Mã môn học [%s] không tồn tại trong hệ thống.', 16, 1, @MaMH);
        RETURN;
    END

    -- 2. Kiểm tra môn học có đang được sử dụng trong lớp học phần không
    IF EXISTS (SELECT * FROM LopHocPhan WHERE MaMH = @MaMH)
    BEGIN
        RAISERROR(N'Không thể cập nhật môn [%s]. Môn học đang được sử dụng trong lớp học phần.', 16, 1, @MaMH);
        RETURN;
    END

    -- 3. Kiểm tra số tín chỉ hợp lệ
    IF @SoTinChi < 1 OR @SoTinChi > 6
    BEGIN
        RAISERROR(N'Số tín chỉ phải nằm trong khoảng 1-6. Giá trị nhận được: %d', 16, 1, @SoTinChi);
        RETURN;
    END

    -- 4. Kiểm tra tên môn học không được rỗng
    IF @TenMH IS NULL OR LTRIM(RTRIM(@TenMH)) = ''
    BEGIN
        RAISERROR(N'Tên môn học không được để trống.', 16, 1);
        RETURN;
    END

    -- Nếu vượt qua tất cả kiểm tra, cập nhật môn học
    UPDATE MonHoc
    SET TenMH = @TenMH, SoTinChi = @SoTinChi
    WHERE MaMH = @MaMH;

    PRINT N'THÀNH CÔNG: Đã cập nhật môn học [' + @MaMH + N'] - ' + @TenMH + N' (' + CAST(@SoTinChi AS NVARCHAR) + N' tín chỉ).';
END
GO

-- Chạy thử sp_CapNhatMonHoc
EXEC sp_CapNhatMonHoc @MaMH = N'XYZ', @TenMH = N'Môn không tồn tại', @SoTinChi = 3; -- Báo lỗi: Mã không tồn tại
EXEC sp_CapNhatMonHoc @MaMH = N'CSDL', @TenMH = N'Cơ sở dữ liệu nâng cao', @SoTinChi = 4; -- Báo lỗi: Đang được dùng trong LHP

INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES
(N'AI', N'Artificial Intelligence', 2)
EXEC sp_CapNhatMonHoc @MaMH = N'AI', @TenMH = N'Trí tuệ nhân tạo', @SoTinChi = 4; -- Thành công
SELECT * FROM MonHoc
GO

/*
------------------------------------------------------------
* Tên hàm: fn_LayDanhSachMonTienQuyet
------------------------------------------------------------
*/
CREATE FUNCTION fn_LayDanhSachMonTienQuyet (@MaMH NVARCHAR(10))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        mt.MaMH_TienQuyet AS MaMH,
        mh.TenMH,
        mh.SoTinChi
    FROM MonHoc_TienQuyet mt
    INNER JOIN MonHoc mh ON mt.MaMH_TienQuyet = mh.MaMH
    WHERE mt.MaMH_Chinh = @MaMH
)
GO

-- Lấy danh sách tiên quyết của môn LTW (Lập trình Web)
SELECT * FROM dbo.fn_LayDanhSachMonTienQuyet('LTW');

-- Lấy danh sách tiên quyết của môn QTLH (Quản trị lữ hành)
SELECT * FROM dbo.fn_LayDanhSachMonTienQuyet('QTLH');

-- Lấy danh sách tiên quyết của môn CSDL (không có tiên quyết)
SELECT * FROM dbo.fn_LayDanhSachMonTienQuyet('CSDL');
GO

/*
------------------------------------------------------------
* Tên hàm: trg_NganXoaHocKy
------------------------------------------------------------
*/
CREATE TRIGGER trg_NganXoaHocKy
ON HocKy
INSTEAD OF DELETE
AS
BEGIN
    DECLARE @MaHK NVARCHAR(10);
    
    -- Lấy mã học kỳ từ bảng bảng ảo 'deleted'
    SELECT @MaHK = MaHK FROM deleted;
    
    -- Kiểm tra xem học kỳ này còn lớp học phần không
    IF EXISTS (
        SELECT 1 
        FROM deleted d
        INNER JOIN LopHocPhan lhp ON d.MaHK = lhp.MaHK
    )
    BEGIN
        -- Nếu còn lớp học phần, thông báo lỗi và không thực hiện xóa
        RAISERROR(N'Không thể xóa học kỳ [%s]. Vẫn còn lớp học phần trong học kỳ này.', 16, 1, @MaHK);
    END
    ELSE
    BEGIN
        -- Nếu không còn lớp học phần, tiến hành xóa học kỳ
        DELETE FROM HocKy
        WHERE MaHK = @MaHK;
        
        PRINT N'Đã xóa học kỳ [' + @MaHK + N'] thành công.';
    END
END
GO

-- Chạy thử trigger
DELETE FROM HocKy WHERE MaHK = N'HK1-2425'; -- Sẽ báo lỗi vì còn lớp học phần LHP01-LHP07
DELETE FROM HocKy WHERE MaHK = N'HK2-2425'; -- Thành công nếu không có lớp học phần nào
SELECT * FROM HocKy;
GO

/*
------------------------------------------------------------
* Cursor: sp_BaoCaoHocKy
------------------------------------------------------------
*/
CREATE PROCEDURE sp_BaoCaoHocKy @MaHK NVARCHAR(10)
AS
BEGIN
    PRINT N'--- BÁO CÁO SĨ SỐ HỌC KỲ ' + @MaHK + N' ---';

    -- 1. Khai báo các biến để giữ dữ liệu từ Cursor
    DECLARE @MaLHP_Current NVARCHAR(10);
    DECLARE @TenMH_Current NVARCHAR(100);
    DECLARE @SiSo_Current INT;

    -- 2. Khai báo Cursor
    -- (Lấy các lớp học phần thuộc học kỳ @MaHK)
    DECLARE cur_LopHocPhan CURSOR FOR
        SELECT
            lhp.MaLHP,
            mh.TenMH
        FROM LopHocPhan AS lhp
        JOIN MonHoc AS mh ON lhp.MaMH = mh.MaMH
        WHERE lhp.MaHK = @MaHK;

    -- 3. Mở Cursor để bắt đầu sử dụng
    OPEN cur_LopHocPhan;

    -- 4. Lấy dòng dữ liệu ĐẦU TIÊN
    FETCH NEXT FROM cur_LopHocPhan
    INTO @MaLHP_Current, @TenMH_Current;

    -- 5. Bắt đầu vòng lặp
    -- (@@FETCH_STATUS = 0 nghĩa là lấy dữ liệu thành công)
    WHILE (@@FETCH_STATUS = 0)
    BEGIN
        -- 6. Xử lý logic cho MỖI DÒNG

        -- Đếm sĩ số thực tế của lớp học phần này
        SELECT @SiSo_Current = COUNT(*)
        FROM DangKyHocPhan
        WHERE MaLHP = @MaLHP_Current;

        -- In kết quả của dòng này
        PRINT
            N' - Lớp ' + @MaLHP_Current +
            N' (' + @TenMH_Current + N'): ' +
            CAST(@SiSo_Current AS VARCHAR(10)) + N' sinh viên.';

        -- 7. Lấy dòng dữ liệu TIẾP THEO
        FETCH NEXT FROM cur_LopHocPhan
        INTO @MaLHP_Current, @TenMH_Current;
    END

    PRINT N'--- Kết thúc báo cáo ---';

    -- 8. Đóng và Hủy Cursor
    CLOSE cur_LopHocPhan;
    DEALLOCATE cur_LopHocPhan;
END
GO

-- Chạy báo cáo cho học kỳ 1 năm 2024-2025
SELECT
    LHP.MaHK, -- Hiển thị Mã học kỳ
    LHP.MaLHP,
    MH.TenMH AS TenMonHoc,
    SV.MaSV,
    SV.HoTenSV
FROM 
    LopHocPhan AS LHP
INNER JOIN 
    DangKyHocPhan AS DKHP ON LHP.MaLHP = DKHP.MaLHP
INNER JOIN
    MonHoc AS MH ON LHP.MaMH = MH.MaMH
INNER JOIN
    SinhVien AS SV ON DKHP.MaSV = SV.MaSV 
ORDER BY
    LHP.MaHK, -- Sắp xếp theo Học kỳ trước
    LHP.MaLHP, 
    SV.MaSV;
EXEC sp_BaoCaoHocKy @MaHK = N'HK1-2425';
-- ====================================================================

-- TRUY VẤN 2: Chạy Báo cáo Tổng hợp (Sĩ số của lớp)
SELECT
    LHP.MaHK, -- Hiển thị Mã học kỳ
    LHP.MaLHP,
    MH.TenMH AS TenMonHoc,
    SV.MaSV,
    SV.HoTenSV
FROM 
    LopHocPhan AS LHP
INNER JOIN 
    DangKyHocPhan AS DKHP ON LHP.MaLHP = DKHP.MaLHP
INNER JOIN
    MonHoc AS MH ON LHP.MaMH = MH.MaMH
INNER JOIN
    SinhVien AS SV ON DKHP.MaSV = SV.MaSV 
ORDER BY
    LHP.MaHK, -- Sắp xếp theo Học kỳ trước
    LHP.MaLHP, 
    SV.MaSV;

EXEC sp_BaoCaoHocKy @MaHK = N'HK2-2324';
GO

/*
------------------------------------------------------------
* Tên hàm: fn_KiemTraTienQuyet
------------------------------------------------------------
*/
CREATE FUNCTION fn_KiemTraTienQuyet
(
    @MaSV NVARCHAR(10),
    @MaMH_Chinh NVARCHAR(10)
)
RETURNS BIT
AS
BEGIN
    DECLARE @SoMonTienQuyet INT;
    DECLARE @SoMonDaDat INT;
    DECLARE @KetQua BIT = 0; -- Mặc định là KHÔNG đủ điều kiện

    -- 1. Đếm tổng số môn tiên quyết mà môn học chính yêu cầu
    SELECT @SoMonTienQuyet = COUNT(*)
    FROM MonHoc_TienQuyet
    WHERE MaMH_Chinh = @MaMH_Chinh;

    -- 2. Nếu môn học không yêu cầu môn tiên quyết (count = 0) --> Sinh viên đủ điều kiện
    IF @SoMonTienQuyet = 0
    BEGIN
        SET @KetQua = 1;
        RETURN @KetQua;
    END

    -- 3. Đếm số môn tiên quyết mà sinh viên đã HỌC VÀ ĐẠT (>= 4.0)
    -- (Dùng DISTINCT phòng trường hợp SV học cải thiện nhiều lần)
    SELECT @SoMonDaDat = COUNT(DISTINCT mhtq.MaMH_TienQuyet)
    FROM DangKyHocPhan AS dkhp
    JOIN LopHocPhan AS lhp ON dkhp.MaLHP = lhp.MaLHP
    -- Join với bảng tiên quyết để chỉ lấy những môn là tiên quyết của @MaMH_Chinh
    JOIN MonHoc_TienQuyet AS mhtq ON lhp.MaMH = mhtq.MaMH_TienQuyet
    WHERE
        dkhp.MaSV = @MaSV                     -- Đúng sinh viên
        AND mhtq.MaMH_Chinh = @MaMH_Chinh     -- Đúng môn học chính
        AND dkhp.DiemTongKet >= 4.0;         -- Điểm đạt (Giả sử 4.0 là điểm qua môn)

    -- 4. So sánh
    -- Nếu tổng số môn yêu cầu BẰNG tổng số môn đã đạt -> Đủ điều kiện
    IF @SoMonTienQuyet = @SoMonDaDat
    BEGIN
        SET @KetQua = 1;
    END

    -- Trả về kết quả (0 hoặc 1)
    RETURN @KetQua;
END
GO

-- Sinh viên '2001140001' đã học cả 'CSDL' (LHP01) và 'HDT' (LHP02)
-- nên ĐỦ điều kiện học 'LTW' (LHP04)
select * from DangKyHocPhan
order by MaSV
select * from LopHocPhan
select *  from MonHoc_TienQuyet

SELECT dbo.fn_KiemTraTienQuyet('2001140001', 'LTW') AS DuDieuKien_HetCacMonTienQuyet;

-- Sinh viên '2001140002' chỉ học 'CSDL' (LHP01) nhưng CHƯA học 'HDT'
-- nên KHÔNG đủ điều kiện học 'LTW' (LHP04) vì thiếu môn tiên quyết 'HDT'
select * from DangKyHocPhan
order by MaSV
select * from LopHocPhan
select *  from MonHoc_TienQuyet
SELECT dbo.fn_KiemTraTienQuyet('2001140002', 'LTW') AS KhongDuDieuKien_ThieuHDT;
GO

/*
------------------------------------------------------------
* Tên thủ tục: sp_DangKyMonHoc
------------------------------------------------------------
*/
CREATE PROCEDURE sp_DangKyMonHoc
    @MaSV NVARCHAR(10),
    @MaLHP NVARCHAR(10)
AS
BEGIN
    -- 1. Kiểm tra Lớp học phần có tồn tại không
    DECLARE @MaMH_Chinh NVARCHAR(10);
    SELECT @MaMH_Chinh = MaMH
    FROM LopHocPhan
    WHERE MaLHP = @MaLHP;

    IF @MaMH_Chinh IS NULL
    BEGIN
        PRINT N'LỖI: Mã lớp học phần ' + @MaLHP + N' không tồn tại.';
        RETURN;
    END

    -- 2. Kiểm tra sinh viên đã đăng ký lớp này chưa
    IF EXISTS (SELECT * FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP)
    BEGIN
        PRINT N'LỖI: Sinh viên ' + @MaSV + N' đã đăng ký lớp ' + @MaLHP + N' này rồi.';
        RETURN;
    END

    -- 3. Gọi hàm kiểm tra tiên quyết
    IF (dbo.fn_KiemTraTienQuyet(@MaSV, @MaMH_Chinh) = 0)
    BEGIN
        -- Nếu trả về 0 (Không đủ điều kiện) -> Báo lỗi và dừng
        PRINT N'LỖI: Sinh viên ' + @MaSV + N' không đủ điều kiện tiên quyết để đăng ký môn ' + @MaMH_Chinh;
        RETURN;
    END

    -- Nếu mọi thứ OK, tiến hành INSERT
    INSERT INTO DangKyHocPhan (MaSV, MaLHP)
    VALUES (@MaSV, @MaLHP);

    PRINT N'THÀNH CÔNG: Đã đăng ký lớp học phần ' + @MaLHP + N' cho sinh viên ' + @MaSV + N'.';
END
GO

-- Thử đăng ký cho SV '2001140002' học 'LTW' (LHP04) -> Sẽ báo lỗi do chua hoc mon hoc phan tuyen quyet
select * from DangKyHocPhan
order by MaSV
select * from LopHocPhan
select *  from MonHoc_TienQuyet
EXEC sp_DangKyMonHoc @MaSV = N'2001140002', @MaLHP = N'LHP04'; -- Sẽ báo lỗi
EXEC sp_DangKyMonHoc @MaSV = N'2001140001', @MaLHP = N'LHP04'; -- Sẽ thành công
GO

/*
------------------------------------------------------------
* Tên trigger: trg_KiemTraNgayDangKy
------------------------------------------------------------
*/
CREATE TRIGGER trg_KiemTraNgayDangKy
ON DangKyHocPhan
FOR INSERT
AS
BEGIN
    -- Kiểm tra xem có bất kỳ dòng nào trong các dòng vừa chèn (bảng 'inserted')
    -- vi phạm quy tắc ngày tháng hay không.
    IF EXISTS (
        SELECT *
        FROM inserted AS i
        JOIN LopHocPhan AS lhp ON i.MaLHP = lhp.MaLHP
        JOIN HocKy AS hk ON lhp.MaHK = hk.MaHK
		-- Kiểm tra ngày hiện tại CÓ nằm ngoài khoảng cho phép không
        WHERE GETDATE() NOT BETWEEN hk.NgayBatDau AND hk.NgayKetThuc
    )
    BEGIN
        -- Nếu tìm thấy vi phạm, báo lỗi và ROLLBACK
        PRINT N'LỖI: Đã hết hạn đăng ký hoặc học kỳ chưa bắt đầu.';
        ROLLBACK TRANSACTION;
    END
END
GO

-- Thử đăng ký vào một lớp của học kỳ CŨ (LHP08 - HK2-2324)
-- Trigger 'trg_KiemTraNgayDangKy' sẽ bắt lỗi và ROLLBACK

-- Thử đăng ký cho SV '2001140002' học 'LTW' (LHP04) -> Sẽ báo lỗi
select * from LopHocPhan
EXEC sp_DangKyMonHoc @MaSV = N'2001140002', @MaLHP = N'LHP08';
GO

--// ========== 3. DATABASE SCRIPTS ==========/

-- Tạo bảng Account
CREATE TABLE Account (
    MaTaiKhoan NVARCHAR(20) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(MAX) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    LoaiTaiKhoan NVARCHAR(20) CHECK (LoaiTaiKhoan IN ('Admin', 'Lecturer', 'Student')),
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE(),
    NgayCapNhat DATETIME,
    CONSTRAINT CHK_Email CHECK (Email LIKE '%@%')
);

-- Tạo bảng LoginLog
CREATE TABLE LoginLog (
    ID INT IDENTITY PRIMARY KEY,
    TenDangNhap NVARCHAR(50),
    NgayGio DATETIME DEFAULT GETDATE(),
    ThanhCong BIT,
    DiaChiIP VARCHAR(20),
    CONSTRAINT FK_LoginLog_Account FOREIGN KEY (TenDangNhap) REFERENCES Account(TenDangNhap)
);

-- Tạo bảng LogoutLog
CREATE TABLE LogoutLog (
    ID INT IDENTITY PRIMARY KEY,
    TenDangNhap NVARCHAR(50),
    NgayGio DATETIME DEFAULT GETDATE(),
    DiaChiIP VARCHAR(20),
    CONSTRAINT FK_LogoutLog_Account FOREIGN KEY (TenDangNhap) REFERENCES Account(TenDangNhap)
);

-- Dữ liệu mẫu cho bảng Account (mật khẩu được mã hóa SHA256)
INSERT INTO Account (MaTaiKhoan, TenDangNhap, MatKhau, Email, LoaiTaiKhoan, TrangThai, NgayTao)
VALUES 
('200120396', '2001230396', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@huit.edu.vn', 'Admin', 1, GETDATE());
INSERT INTO Account (MaTaiKhoan, TenDangNhap, MatKhau, Email, LoaiTaiKhoan, TrangThai, NgayTao)
VALUES 
('2001130001', '2001130001', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'khoavo090807@gmail.com', 'Student', 1, GETDATE());
INSERT INTO Account (MaTaiKhoan, TenDangNhap, MatKhau, Email, LoaiTaiKhoan, TrangThai, NgayTao)
VALUES 
(N'ACC001', N'admin', N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', N'admin@huit.edu.vn', N'Admin', 1, GETDATE()),
(N'ACC002', N'baotv', N'5e884898da28047151d0e56f8dc62927593d815b266dd1c23d936a6b151f4f2a', N'baotv@huit.edu.vn', N'Lecturer', 1, GETDATE()),
(N'ACC003', N'annv', N'5e884898da28047151d0e56f8dc62927593d815b266dd1c23d936a6b151f4f2a', N'annv@huit.edu.vn', N'Student', 1, GETDATE());

-- Tạo Stored Procedure: Lấy danh sách tài khoản
CREATE PROCEDURE sp_LayDanhSachAccount
AS
BEGIN
    SELECT MaTaiKhoan, TenDangNhap, Email, LoaiTaiKhoan, TrangThai, NgayTao
    FROM Account
    ORDER BY NgayTao DESC;
END
GO

-- Tạo Stored Procedure: Khoá/mở khoá tài khoản
CREATE PROCEDURE sp_KhoaTaiKhoan
    @MaTaiKhoan NVARCHAR(20),
    @TrangThai BIT
AS
BEGIN
    UPDATE Account
    SET TrangThai = @TrangThai, NgayCapNhat = GETDATE()
    WHERE MaTaiKhoan = @MaTaiKhoan;
    
    PRINT N'Cập nhật trạng thái tài khoản thành công!';
END
GO

-- Tạo Trigger: Kiểm tra mật khẩu khi INSERT Account
CREATE TRIGGER trg_KiemTraMatKhau_Account
ON Account
FOR INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT * FROM inserted
        WHERE LEN(MatKhau) < 40 -- SHA256 hash tối thiểu 40 ký tự
    )
    BEGIN
        RAISERROR(N'Lỗi: Mật khẩu phải được mã hóa!', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

-- Tạo Function: Kiểm tra tài khoản hoạt động
CREATE FUNCTION fn_KiemTraTaiKhoan (@TenDangNhap NVARCHAR(50))
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;
    
    IF EXISTS (SELECT 1 FROM Account WHERE TenDangNhap = @TenDangNhap AND TrangThai = 1)
        SET @KetQua = 1;
    
    RETURN @KetQua;
END
GO

-- Tạo View: Thống kê hoạt động đăng nhập
CREATE VIEW vw_ThongKeLoginActivity
AS
SELECT 
    a.TenDangNhap,
    a.LoaiTaiKhoan,
    COUNT(CASE WHEN ll.ThanhCong = 1 THEN 1 END) AS LanDangNhapThanhCong,
    COUNT(CASE WHEN ll.ThanhCong = 0 THEN 1 END) AS LanDangNhapThatBai,
    MAX(ll.NgayGio) AS LanDangNhapCuoi
FROM Account a
LEFT JOIN LoginLog ll ON a.TenDangNhap = ll.TenDangNhap
GROUP BY a.TenDangNhap, a.LoaiTaiKhoan;
GO



INSERT INTO Account (MaTaiKhoan, TenDangNhap, MatKhau, Email, LoaiTaiKhoan, TrangThai, NgayTao)
VALUES 
('200120397', 'KHOAVO', 
  '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 
  'khoavo090807@gmail.com', 'Admin', 1, GETDATE());

  
------------------- Chương 3 ---------------------------
--------- Tạo login ---------
  -- login cho sinh viên
CREATE LOGIN sinhvien
WITH PASSWORD = 'sinhvien@123', 
default_database = QLSV_DoAn_2
-- login cho giảng viên
CREATE LOGIN giangvien
WITH PASSWORD = 'giangvien@123', 
default_database = QLSV_DoAn_2

--------- Tạo user trong database ---------
-- user cho sinh viên
USE QLSV_DoAn_2
CREATE USER user_sinhvien
FOR LOGIN sinhvien
--user cho giảng viên
USE QLSV_DoAn_2
CREATE USER user_giangvien
FOR LOGIN giangvien

--------- Tạo nhóm quyền role ---------
CREATE ROLE role_sinhvien
CREATE ROLE role_giangvien

--------- Cấp quyền --------- 
-- Sinh viên chỉ được xem dữ liệu 
GRANT SELECT ON dbo.SINHVIEN TO role_sinhvien
-- Giảng viên được xem và cập nhật dữ liệu
GRANT SELECT, INSERT, UPDATE, DELETE 
ON dbo.giangvien 
TO role_giangvien

--------- Thu hồi quyền DELETE khỏi giảng viên --------- 
REVOKE DELETE 
ON dbo.giangvien
FROM role_giangvien

--------- thêm user vào role ---------
ALTER ROLE role_sinhvien ADD MEMBER user_sinhvien;
ALTER ROLE role_giangvien ADD MEMBER user_giangvien;
GO


--------- KIỂM TRA QUYỀN ---------
-- Liệt kê quyền của người dùng
EXEC sp_helprolemember 'role_sinhvien';
EXEC sp_helprolemember 'role_giangvien';
GO
------------- Lập lịch trình sao lưu định kỳ cho cơ sở dữ liệu và thiết lập sao lưu tự động ------------------
-- sao lưu toàn bộ
BACKUP DATABASE QLSV_DoAn_2
TO DISK = 'D:\Đồ án HQTCSDL.back'
WITH INIT,        
     NAME = 'Full Backup QLSV_DoAn',
     SKIP,
     FORMAT,
     STATS = 10
-- Sao lưu khác biệt: Dùng để sao lưu chỉ phần thay đổi kể từ lần sao lưu toàn bộ gần nhất.
BACKUP DATABASE QLSV_DoAn_2
TO DISK = 'D:\Đồ án HQTCSDL.back'
WITH DIFFERENTIAL,
     NAME = 'Sao lưu khác biệt QLSV_DoAn',
     STATS = 10;
--sao lưu nhật ký giao dịch
ALTER DATABASE QLSV_DoAn_2
SET RECOVERY FULL;

BACKUP LOG QLSV_DoAn_2
TO DISK = 'D:\Đồ án HQTCSDL_Log_20251025.trn'
WITH INIT,
     NAME = 'sao lưu nhật ký giao dịch QLSV_DoAn';
-- Sao lưu tự động
DECLARE @BackupFile NVARCHAR(255)
SET @BackupFile = 'D:\QLSV_DoAn_' + CONVERT(VARCHAR(8), GETDATE(), 112) + '.bak'

BACKUP DATABASE [QLSV_DoAn_2]
TO DISK = @BackupFile
WITH INIT, STATS = 10;