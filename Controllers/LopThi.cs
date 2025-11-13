using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Controllers
{
    [Authorize]
    public class LopThiController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // GET: LopThi/Index - Danh sách lớp thi
        // ============================================
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string searchString, string trangThai)
        {
            List<LopThi> danhSachLopThi = new List<LopThi>();

            string query = @"
                SELECT 
                    lt.MaLopThi,
                    lt.MaDT,
                    dt.TenDT,
                    lt.TenLopThi,
                    lt.NgayThi,
                    lt.PhongThi,
                    lt.SoLuong,
                    lt.TrangThai,
                    k.TenKhoa,
                    ISNULL(gv.HoTenGV, N'N/A') AS TenGiangVien,
                    ISNULL(dt.ThoiGianLamBai, 0) AS ThoiGianLamBai,
                    (SELECT COUNT(*) FROM KetQuaThi WHERE MaLopThi = lt.MaLopThi AND DaThamGia = 1) AS SoDaTham
                FROM LopThi lt
                INNER JOIN DeThi dt ON lt.MaDT = dt.MaDT
                INNER JOIN Khoa k ON dt.MaKhoa = k.MaKhoa
                LEFT JOIN GiangVien gv ON lt.MaGV = gv.MaGV
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (lt.MaLopThi LIKE @Search OR lt.TenLopThi LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + searchString + "%"));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query += " AND lt.TrangThai = @TrangThai";
                parameters.Add(new SqlParameter("@TrangThai", trangThai));
            }

            query += " ORDER BY lt.NgayThi DESC";

            try
            {
                DataTable dt_result = db.ExecuteQuery(query, parameters.ToArray());

                foreach (DataRow row in dt_result.Rows)
                {
                    danhSachLopThi.Add(new LopThi
                    {
                        MaLopThi = row["MaLopThi"] != DBNull.Value ? row["MaLopThi"].ToString() : "",
                        MaDT = row["MaDT"] != DBNull.Value ? row["MaDT"].ToString() : "",
                        TenDeThi = row["TenDT"] != DBNull.Value ? row["TenDT"].ToString() : "",
                        TenLopThi = row["TenLopThi"] != DBNull.Value ? row["TenLopThi"].ToString() : "",
                        NgayThi = row["NgayThi"] != DBNull.Value ? Convert.ToDateTime(row["NgayThi"]) : DateTime.Now,
                        PhongThi = row["PhongThi"] != DBNull.Value ? row["PhongThi"].ToString() : "",
                        SoLuong = row["SoLuong"] != DBNull.Value ? Convert.ToInt32(row["SoLuong"]) : 0,
                        TrangThai = row["TrangThai"] != DBNull.Value ? row["TrangThai"].ToString() : "Chưa thi",
                        TenKhoa = row["TenKhoa"] != DBNull.Value ? row["TenKhoa"].ToString() : "",
                        TenGiangVien = row["TenGiangVien"] != DBNull.Value ? row["TenGiangVien"].ToString() : "N/A",
                        ThoiGianLamBai = row["ThoiGianLamBai"] != DBNull.Value ? Convert.ToInt32(row["ThoiGianLamBai"]) : 0,
                        SoDaTham = row["SoDaTham"] != DBNull.Value ? Convert.ToInt32(row["SoDaTham"]) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi tải danh sách: " + ex.Message;
            }

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            return View(danhSachLopThi);
        }

        // ============================================
        // GET: LopThi/Create - Form tạo lớp thi
        // ============================================
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var model = new TaoLopThiViewModel();
            LoadDanhSachDeThi();
            LoadDanhSachGiangVien();
            return View(model);
        }

        // ============================================
        // POST: LopThi/Create - Xử lý tạo lớp thi
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(TaoLopThiViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LoadDanhSachDeThi();
                    LoadDanhSachGiangVien();
                    return View(model);
                }

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaDT", model.MaDT),
                    new SqlParameter("@NgayThi", model.NgayThi),
                    new SqlParameter("@PhongThi", model.PhongThi),
                    new SqlParameter("@MaGV", (object)model.MaGV ?? DBNull.Value)
                };

                db.ExecuteStoredProcedureNonQuery("sp_TaoLopThi", parameters);

                TempData["SuccessMessage"] = "Tạo lớp thi thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                LoadDanhSachDeThi();
                LoadDanhSachGiangVien();
                return View(model);
            }
        }

        // ============================================
        // GET: LopThi/Details/{id}
        // ============================================
        [Authorize(Roles = "Admin")]
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = @"
                SELECT 
                    lt.MaLopThi,
                    lt.MaDT,
                    dt.TenDT,
                    lt.TenLopThi,
                    lt.NgayThi,
                    lt.PhongThi,
                    lt.SoLuong,
                    lt.TrangThai,
                    k.TenKhoa,
                    ISNULL(gv.HoTenGV, N'N/A') AS TenGiangVien,
                    ISNULL(dt.ThoiGianLamBai, 0) AS ThoiGianLamBai,
                    (SELECT COUNT(*) FROM KetQuaThi WHERE MaLopThi = lt.MaLopThi AND DaThamGia = 1) AS SoDaTham
                FROM LopThi lt
                INNER JOIN DeThi dt ON lt.MaDT = dt.MaDT
                INNER JOIN Khoa k ON dt.MaKhoa = k.MaKhoa
                LEFT JOIN GiangVien gv ON lt.MaGV = gv.MaGV
                WHERE lt.MaLopThi = @MaLopThi";

            try
            {
                DataTable dt_result = db.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@MaLopThi", id) });

                if (dt_result.Rows.Count == 0)
                    return HttpNotFound();

                DataRow row = dt_result.Rows[0];
                var lopThi = new LopThi
                {
                    MaLopThi = row["MaLopThi"].ToString(),
                    MaDT = row["MaDT"].ToString(),
                    TenDeThi = row["TenDT"].ToString(),
                    TenLopThi = row["TenLopThi"].ToString(),
                    NgayThi = Convert.ToDateTime(row["NgayThi"]),
                    PhongThi = row["PhongThi"].ToString(),
                    SoLuong = Convert.ToInt32(row["SoLuong"]),
                    TrangThai = row["TrangThai"].ToString(),
                    TenKhoa = row["TenKhoa"].ToString(),
                    TenGiangVien = row["TenGiangVien"].ToString(),
                    ThoiGianLamBai = Convert.ToInt32(row["ThoiGianLamBai"]),
                    SoDaTham = row["SoDaTham"] != DBNull.Value ? Convert.ToInt32(row["SoDaTham"]) : 0
                };

                return View(lopThi);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ============================================
        // GET: LopThi/DanhSachSinhVien/{id}
        // ============================================
        [Authorize(Roles = "Admin")]
        public ActionResult DanhSachSinhVien(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var danhSach = new List<dynamic>();

            string query = @"
                SELECT 
                    ct.ID,
                    ct.MaLopThi,
                    ct.MaSV,
                    ct.SoThuTu,
                    sv.HoTenSV,
                    sv.Email,
                    ISNULL(kq.DiemTong, 0) AS DiemTong,
                    ISNULL(kq.XepLoai, N'---') AS XepLoai,
                    ISNULL(kq.TrangThai, N'Chưa làm') AS TrangThai,
                    ISNULL(kq.DaThamGia, 0) AS DaThamGia
                FROM ChiTietLopThi ct
                INNER JOIN SinhVien sv ON ct.MaSV = sv.MaSV
                LEFT JOIN KetQuaThi kq ON ct.MaLopThi = kq.MaLopThi AND ct.MaSV = kq.MaSV
                WHERE ct.MaLopThi = @MaLopThi
                ORDER BY ct.SoThuTu ASC";

            try
            {
                DataTable dt = db.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@MaLopThi", id) });

                foreach (DataRow row in dt.Rows)
                {
                    dynamic item = new System.Dynamic.ExpandoObject();
                    item.ID = Convert.ToInt32(row["ID"]);
                    item.MaLopThi = row["MaLopThi"].ToString();
                    item.MaSV = row["MaSV"].ToString();
                    item.SoThuTu = Convert.ToInt32(row["SoThuTu"]);
                    item.HoTenSV = row["HoTenSV"].ToString();
                    item.Email = row["Email"].ToString();
                    item.DiemTong = row["DiemTong"] != DBNull.Value ? (float?)Convert.ToSingle(row["DiemTong"]) : null;
                    item.XepLoai = row["XepLoai"].ToString();
                    item.TrangThai = row["TrangThai"].ToString();
                    item.DaThamGia = Convert.ToBoolean(row["DaThamGia"]);

                    danhSach.Add(item);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            ViewBag.MaLopThi = id;
            return View(danhSach);
        }

        // ============================================
        // GET: LopThi/LamBai/{id} - Sinh viên làm bài thi
        // ============================================
        [Authorize(Roles = "User")]
        public ActionResult LamBai(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            try
            {
                // Lấy thông tin lớp thi
                string queryLopThi = @"
                    SELECT lt.MaLopThi, lt.MaDT, dt.TenDT, ISNULL(dt.ThoiGianLamBai, 0) AS ThoiGianLamBai
                    FROM LopThi lt
                    INNER JOIN DeThi dt ON lt.MaDT = dt.MaDT
                    WHERE lt.MaLopThi = @MaLopThi";

                DataTable dtLopThi = db.ExecuteQuery(queryLopThi,
                    new SqlParameter[] { new SqlParameter("@MaLopThi", id) });

                if (dtLopThi.Rows.Count == 0)
                    return HttpNotFound();

                DataRow rowLopThi = dtLopThi.Rows[0];
                string maDT = rowLopThi["MaDT"].ToString();
                int thoiGian = Convert.ToInt32(rowLopThi["ThoiGianLamBai"]);

                // Lấy danh sách câu hỏi
                string queryCau = @"
                    SELECT 
                        MaCau,
                        NoiDung,
                        DapAnA,
                        DapAnB,
                        DapAnC,
                        DapAnD,
                        ISNULL(Diem, 1) AS Diem,
                        ThuTu
                    FROM CauHoiTracNghiem
                    WHERE MaDT = @MaDT
                    ORDER BY ThuTu ASC";

                DataTable dtCau = db.ExecuteQuery(queryCau,
                    new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

                var model = new BaiThiViewModel
                {
                    MaLopThi = id,
                    MaDT = maDT,
                    TenDeThi = rowLopThi["TenDT"].ToString(),
                    ThoiGianLamBai = thoiGian,
                    TongCau = dtCau.Rows.Count,
                    ThoiGianBatDau = DateTime.Now,
                    DanhSachCau = new List<CauHoiTracNghiemViewModel>()
                };

                foreach (DataRow row in dtCau.Rows)
                {
                    model.DanhSachCau.Add(new CauHoiTracNghiemViewModel
                    {
                        MaCau = Convert.ToInt32(row["MaCau"]),
                        NoiDung = row["NoiDung"].ToString(),
                        DapAnA = row["DapAnA"].ToString(),
                        DapAnB = row["DapAnB"].ToString(),
                        DapAnC = row["DapAnC"].ToString(),
                        DapAnD = row["DapAnD"].ToString(),
                        Diem = Convert.ToSingle(row["Diem"]),
                        ThuTu = Convert.ToInt32(row["ThuTu"])
                    });
                }

                // Cập nhật thời gian bắt đầu
                string updateQuery = @"
                    UPDATE KetQuaThi
                    SET ThoiGianBatDau = GETDATE(),
                        TrangThai = N'Đang làm'
                    WHERE MaLopThi = @MaLopThi AND MaSV = @MaSV";

                string maSV = User.Identity.Name;
                db.ExecuteNonQuery(updateQuery, new SqlParameter[]
                {
                    new SqlParameter("@MaLopThi", id),
                    new SqlParameter("@MaSV", maSV)
                });

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ============================================
        // POST: LopThi/NopBai - Nộp bài thi
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public ActionResult NopBai(string maLopThi, string maDT, FormCollection formCollection)
        {
            try
            {
                string maSV = User.Identity.Name;

                // Lấy danh sách câu hỏi để kiểm tra đáp án
                string queryCau = @"
                    SELECT MaCau, DapAnDung, Diem
                    FROM CauHoiTracNghiem
                    WHERE MaDT = @MaDT
                    ORDER BY ThuTu ASC";

                DataTable dtCau = db.ExecuteQuery(queryCau,
                    new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

                int diemDung = 0;
                int tongDiem = 0;

                // Duyệt qua tất cả câu hỏi
                foreach (DataRow row in dtCau.Rows)
                {
                    int maCau = Convert.ToInt32(row["MaCau"]);
                    string dapAnDung = row["DapAnDung"].ToString();
                    int diem = Convert.ToInt32(row["Diem"]);

                    // Lấy đáp án sinh viên chọn
                    string keyForm = "dapAn[" + maCau + "]";
                    string dapAnChon = formCollection[keyForm];

                    float diemNhan = 0;
                    if (dapAnChon == dapAnDung)
                    {
                        diemDung += diem;
                        diemNhan = diem;
                    }

                    tongDiem += diem;

                    // Lưu câu trả lời
                    string insertQuery = @"
                        INSERT INTO CauTraLoiSinhVien (MaLopThi, MaSV, MaCau, DapAnChon, DiemCauNay)
                        VALUES (@MaLopThi, @MaSV, @MaCau, @DapAnChon, @DiemCauNay)";

                    db.ExecuteNonQuery(insertQuery, new SqlParameter[]
                    {
                        new SqlParameter("@MaLopThi", maLopThi),
                        new SqlParameter("@MaSV", maSV),
                        new SqlParameter("@MaCau", maCau),
                        new SqlParameter("@DapAnChon", dapAnChon ?? ""),
                        new SqlParameter("@DiemCauNay", diemNhan)
                    });
                }

                // Tính điểm tự động
                SqlParameter[] paramsTinhDiem = new SqlParameter[]
                {
                    new SqlParameter("@MaLopThi", maLopThi),
                    new SqlParameter("@MaSV", maSV)
                };

                db.ExecuteStoredProcedureNonQuery("sp_TinhDiemTuDong", paramsTinhDiem);

                TempData["SuccessMessage"] = "Nộp bài thành công! Kết quả đã được tính toán.";
                return RedirectToAction("KetQua", new { maLopThi = maLopThi, maSV = maSV });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("LamBai", new { id = maLopThi });
            }
        }

        // ============================================
        // GET: LopThi/KetQua - Xem kết quả thi
        // ============================================
        public ActionResult KetQua(string maLopThi, string maSV)
        {
            try
            {
                string query = @"
                    SELECT 
                        kq.ID,
                        kq.MaLopThi,
                        kq.MaSV,
                        kq.DiemTong,
                        kq.XepLoai,
                        kq.ThoiGianLamBai,
                        kq.ThoiGianBatDau,
                        kq.ThoiGianKetThuc,
                        kq.TrangThai,
                        sv.HoTenSV,
                        lt.TenLopThi
                    FROM KetQuaThi kq
                    INNER JOIN SinhVien sv ON kq.MaSV = sv.MaSV
                    INNER JOIN LopThi lt ON kq.MaLopThi = lt.MaLopThi
                    WHERE kq.MaLopThi = @MaLopThi AND kq.MaSV = @MaSV";

                DataTable dt = db.ExecuteQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@MaLopThi", maLopThi),
                    new SqlParameter("@MaSV", maSV)
                });

                if (dt.Rows.Count == 0)
                    return HttpNotFound();

                DataRow row = dt.Rows[0];
                var model = new KetQuaThiViewModel
                {
                    MaLopThi = row["MaLopThi"].ToString(),
                    MaSV = row["MaSV"].ToString(),
                    HoTenSV = row["HoTenSV"].ToString(),
                    DiemTong = row["DiemTong"] != DBNull.Value ? Convert.ToSingle(row["DiemTong"]) : (float?)null,
                    XepLoai = row["XepLoai"] != DBNull.Value ? row["XepLoai"].ToString() : "N/A",
                    ThoiGianLamBai = row["ThoiGianLamBai"] != DBNull.Value ? Convert.ToInt32(row["ThoiGianLamBai"]) : 0,
                    ThoiGianBatDau = row["ThoiGianBatDau"] != DBNull.Value ? Convert.ToDateTime(row["ThoiGianBatDau"]) : (DateTime?)null,
                    ThoiGianKetThuc = row["ThoiGianKetThuc"] != DBNull.Value ? Convert.ToDateTime(row["ThoiGianKetThuc"]) : (DateTime?)null,
                    TrangThai = row["TrangThai"].ToString(),
                    TenLopThi = row["TenLopThi"].ToString()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ============================================
        // Helper Methods
        // ============================================
        private void LoadDanhSachDeThi()
        {
            try
            {
                string query = @"
                    SELECT dt.MaDT, dt.TenDT, k.TenKhoa
                    FROM DeThi dt
                    INNER JOIN Khoa k ON dt.MaKhoa = k.MaKhoa
                    WHERE dt.TrangThai = 1
                    ORDER BY dt.TenDT";

                DataTable dt = db.ExecuteQuery(query);
                var danhSach = new List<SelectListItem>();

                foreach (DataRow row in dt.Rows)
                {
                    danhSach.Add(new SelectListItem
                    {
                        Value = row["MaDT"].ToString(),
                        Text = row["TenDT"] + " (" + row["TenKhoa"] + ")"
                    });
                }

                ViewBag.DanhSachDeThi = danhSach;
            }
            catch (Exception ex)
            {
                ViewBag.DanhSachDeThi = new List<SelectListItem>();
                TempData["ErrorMessage"] = "Lỗi tải danh sách đề thi: " + ex.Message;
            }
        }

        private void LoadDanhSachGiangVien()
        {
            try
            {
                string query = @"
                    SELECT MaGV, HoTenGV
                    FROM GiangVien
                    ORDER BY HoTenGV";

                DataTable dt = db.ExecuteQuery(query);
                var danhSach = new List<SelectListItem>();

                danhSach.Add(new SelectListItem { Value = "", Text = "-- Chọn giảng viên --" });

                foreach (DataRow row in dt.Rows)
                {
                    danhSach.Add(new SelectListItem
                    {
                        Value = row["MaGV"].ToString(),
                        Text = row["HoTenGV"].ToString()
                    });
                }

                ViewBag.DanhSachGiangVien = danhSach;
            }
            catch (Exception ex)
            {
                ViewBag.DanhSachGiangVien = new List<SelectListItem>();
                TempData["ErrorMessage"] = "Lỗi tải danh sách giảng viên: " + ex.Message;
            }
        }
    }
}