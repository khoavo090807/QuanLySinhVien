// ============================================
// FILE: Controllers/LopHocPhanController.cs
// CHỨC NĂNG: Quản lý Lớp Học Phần
// LIÊN KẾT: Học kỳ → Lớp Học Phần → Đăng Ký Học Phần
// ============================================

using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class LopHocPhanController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ========== GET: LopHocPhan/Index - Xem danh sách lớp học phần ==========
        public ActionResult Index(string searchString)
        {
            List<LopHocPhan> danhSachLHP = new List<LopHocPhan>();

            string query = @"SELECT 
                            lhp.MaLHP, 
                            mh.TenMH, 
                            hk.TenHK, 
                            gv.HoTenGV,
                            lhp.PhongHoc,
                            lhp.SoLuongToiDa,
                            hk.NgayBatDau,
                            hk.NgayKetThuc,
                            (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy,
                            CASE 
                                WHEN GETDATE() < hk.NgayBatDau THEN 'Chưa mở'
                                WHEN GETDATE() > hk.NgayKetThuc THEN 'Đã đóng'
                                ELSE 'Đang mở'
                            END AS TrangThai
                            FROM LopHocPhan lhp
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (lhp.MaLHP LIKE @Search OR mh.TenMH LIKE @Search OR hk.TenHK LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY hk.NgayBatDau DESC, mh.TenMH";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                LopHocPhan lhp = new LopHocPhan
                {
                    MaLHP = row["MaLHP"].ToString(),
                    TenMH = row["TenMH"].ToString(),
                    TenHK = row["TenHK"].ToString(),
                    TenGiangVien = row["HoTenGV"].ToString(),
                    PhongHoc = row["PhongHoc"].ToString(),
                    SoLuongToiDa = Convert.ToInt32(row["SoLuongToiDa"]),
                    SoSinhVienDangKy = Convert.ToInt32(row["SoSinhVienDangKy"]),
                    NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                    NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]),
                    TrangThai = row["TrangThai"].ToString()
                };
                danhSachLHP.Add(lhp);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachLHP);
        }

        // ========== GET: LopHocPhan/Create - Form thêm lớp học phần ==========
        public ActionResult Create()
        {
            LoadDanhSachMonHoc();
            LoadDanhSachHocKy();
            LoadDanhSachGiangVien();
            return View();
        }

        // ========== POST: LopHocPhan/Create - Xử lý thêm lớp học phần ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LopHocPhan lopHocPhan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra trùng mã lớp học phần
                    string checkQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaLHP = @MaLHP";
                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@MaLHP", lopHocPhan.MaLHP) }));

                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Mã lớp học phần đã tồn tại!");
                        LoadDanhSachMonHoc();
                        LoadDanhSachHocKy();
                        LoadDanhSachGiangVien();
                        return View(lopHocPhan);
                    }

                    // Kiểm tra môn học có tồn tại không
                    string checkMHQuery = "SELECT COUNT(*) FROM MonHoc WHERE MaMH = @MaMH";
                    int mhCount = Convert.ToInt32(db.ExecuteScalar(checkMHQuery,
                        new SqlParameter[] { new SqlParameter("@MaMH", lopHocPhan.MaMH) }));

                    if (mhCount == 0)
                    {
                        ModelState.AddModelError("", "Môn học không tồn tại!");
                        LoadDanhSachMonHoc();
                        LoadDanhSachHocKy();
                        LoadDanhSachGiangVien();
                        return View(lopHocPhan);
                    }

                    // Kiểm tra học kỳ có tồn tại không
                    string checkHKQuery = "SELECT COUNT(*) FROM HocKy WHERE MaHK = @MaHK";
                    int hkCount = Convert.ToInt32(db.ExecuteScalar(checkHKQuery,
                        new SqlParameter[] { new SqlParameter("@MaHK", lopHocPhan.MaHK) }));

                    if (hkCount == 0)
                    {
                        ModelState.AddModelError("", "Học kỳ không tồn tại!");
                        LoadDanhSachMonHoc();
                        LoadDanhSachHocKy();
                        LoadDanhSachGiangVien();
                        return View(lopHocPhan);
                    }

                    // Thêm lớp học phần
                    string query = @"INSERT INTO LopHocPhan (MaLHP, MaMH, MaHK, MaGV, PhongHoc, SoLuongToiDa)
                                   VALUES (@MaLHP, @MaMH, @MaHK, @MaGV, @PhongHoc, @SoLuongToiDa)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaLHP", lopHocPhan.MaLHP),
                        new SqlParameter("@MaMH", lopHocPhan.MaMH),
                        new SqlParameter("@MaHK", lopHocPhan.MaHK),
                        new SqlParameter("@MaGV", (object)lopHocPhan.MaGV ?? DBNull.Value),
                        new SqlParameter("@PhongHoc", (object)lopHocPhan.PhongHoc ?? DBNull.Value),
                        new SqlParameter("@SoLuongToiDa", lopHocPhan.SoLuongToiDa)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm lớp học phần thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể thêm lớp học phần. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachMonHoc();
            LoadDanhSachHocKy();
            LoadDanhSachGiangVien();
            return View(lopHocPhan);
        }

        // ========== GET: LopHocPhan/Edit/5 - Form chỉnh sửa lớp học phần ==========
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = "SELECT * FROM LopHocPhan WHERE MaLHP = @MaLHP";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaLHP", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            LopHocPhan lopHocPhan = new LopHocPhan
            {
                MaLHP = row["MaLHP"].ToString(),
                MaMH = row["MaMH"].ToString(),
                MaHK = row["MaHK"].ToString(),
                MaGV = row["MaGV"] != DBNull.Value ? row["MaGV"].ToString() : "",
                PhongHoc = row["PhongHoc"] != DBNull.Value ? row["PhongHoc"].ToString() : "",
                SoLuongToiDa = Convert.ToInt32(row["SoLuongToiDa"])
            };

            LoadDanhSachMonHoc();
            LoadDanhSachHocKy();
            LoadDanhSachGiangVien();
            return View(lopHocPhan);
        }

        // ========== POST: LopHocPhan/Edit/5 - Xử lý cập nhật lớp học phần ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LopHocPhan lopHocPhan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE LopHocPhan 
                                   SET MaMH = @MaMH, 
                                       MaHK = @MaHK, 
                                       MaGV = @MaGV,
                                       PhongHoc = @PhongHoc,
                                       SoLuongToiDa = @SoLuongToiDa
                                   WHERE MaLHP = @MaLHP";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaLHP", lopHocPhan.MaLHP),
                        new SqlParameter("@MaMH", lopHocPhan.MaMH),
                        new SqlParameter("@MaHK", lopHocPhan.MaHK),
                        new SqlParameter("@MaGV", (object)lopHocPhan.MaGV ?? DBNull.Value),
                        new SqlParameter("@PhongHoc", (object)lopHocPhan.PhongHoc ?? DBNull.Value),
                        new SqlParameter("@SoLuongToiDa", lopHocPhan.SoLuongToiDa)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật lớp học phần thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachMonHoc();
            LoadDanhSachHocKy();
            LoadDanhSachGiangVien();
            return View(lopHocPhan);
        }

        // ========== GET: LopHocPhan/Delete/5 - Xác nhận xóa lớp học phần ==========
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = @"SELECT lhp.*, 
                            mh.TenMH, 
                            hk.TenHK, 
                            gv.HoTenGV,
                            (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy
                            FROM LopHocPhan lhp
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                            WHERE lhp.MaLHP = @MaLHP";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaLHP", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            LopHocPhan lopHocPhan = new LopHocPhan
            {
                MaLHP = row["MaLHP"].ToString(),
                TenMH = row["TenMH"].ToString(),
                TenHK = row["TenHK"].ToString(),
                TenGiangVien = row["HoTenGV"] != DBNull.Value ? row["HoTenGV"].ToString() : "Chưa có",
                PhongHoc = row["PhongHoc"] != DBNull.Value ? row["PhongHoc"].ToString() : "",
                SoLuongToiDa = Convert.ToInt32(row["SoLuongToiDa"]),
                SoSinhVienDangKy = Convert.ToInt32(row["SoSinhVienDangKy"])
            };

            ViewBag.SoSinhVienDangKy = lopHocPhan.SoSinhVienDangKy;
            return View(lopHocPhan);
        }

        // ========== POST: LopHocPhan/Delete/5 - Xử lý xóa lớp học phần ==========
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Kiểm tra xem lớp có sinh viên đăng ký không
                string checkDKQuery = "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = @MaLHP";
                int soDK = Convert.ToInt32(db.ExecuteScalar(checkDKQuery,
                    new SqlParameter[] { new SqlParameter("@MaLHP", id) }));

                if (soDK > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa lớp này vì còn {soDK} sinh viên đăng ký!";
                    return RedirectToAction("Index");
                }

                string query = "DELETE FROM LopHocPhan WHERE MaLHP = @MaLHP";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaLHP", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa lớp học phần thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ========== GET: LopHocPhan/Details/5 - Xem chi tiết lớp học phần ==========
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = @"SELECT lhp.*, 
                            mh.TenMH, 
                            hk.TenHK,
                            hk.NgayBatDau,
                            hk.NgayKetThuc,
                            gv.HoTenGV,
                            (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy
                            FROM LopHocPhan lhp
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                            WHERE lhp.MaLHP = @MaLHP";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaLHP", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];

            // Tính trạng thái
            DateTime ngayBatDau = Convert.ToDateTime(row["NgayBatDau"]);
            DateTime ngayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]);
            DateTime now = DateTime.Now;
            string trangThai = "";

            if (now < ngayBatDau)
                trangThai = "Chưa mở";
            else if (now > ngayKetThuc)
                trangThai = "Đã đóng";
            else
                trangThai = "Đang mở";

            LopHocPhan lopHocPhan = new LopHocPhan
            {
                MaLHP = row["MaLHP"].ToString(),
                TenMH = row["TenMH"].ToString(),
                TenHK = row["TenHK"].ToString(),
                TenGiangVien = row["HoTenGV"] != DBNull.Value ? row["HoTenGV"].ToString() : "Chưa có",
                PhongHoc = row["PhongHoc"] != DBNull.Value ? row["PhongHoc"].ToString() : "",
                SoLuongToiDa = Convert.ToInt32(row["SoLuongToiDa"]),
                SoSinhVienDangKy = Convert.ToInt32(row["SoSinhVienDangKy"]),
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                TrangThai = trangThai
            };

            ViewBag.TrangThai = trangThai;
            ViewBag.SoChoConLai = lopHocPhan.SoLuongToiDa - lopHocPhan.SoSinhVienDangKy;
            ViewBag.TiLeDay = lopHocPhan.SoLuongToiDa > 0 ? Math.Round((double)lopHocPhan.SoSinhVienDangKy / lopHocPhan.SoLuongToiDa * 100, 2) : 0;

            return View(lopHocPhan);
        }

        // ========== HELPER METHODS ==========

        private void LoadDanhSachMonHoc()
        {
            string query = "SELECT MaMH, TenMH FROM MonHoc ORDER BY TenMH";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaMH"].ToString(),
                    Text = row["MaMH"] + " - " + row["TenMH"].ToString()
                });
            }

            ViewBag.DanhSachMonHoc = danhSach;
        }

        private void LoadDanhSachHocKy()
        {
            string query = "SELECT MaHK, TenHK FROM HocKy ORDER BY NgayBatDau DESC";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaHK"].ToString(),
                    Text = row["TenHK"].ToString()
                });
            }

            ViewBag.DanhSachHocKy = danhSach;
        }

        private void LoadDanhSachGiangVien()
        {
            string query = "SELECT MaGV, HoTenGV FROM GiangVien ORDER BY HoTenGV";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            danhSach.Add(new SelectListItem { Value = "", Text = "-- Chọn giáo viên --" });

            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaGV"].ToString(),
                    Text = row["MaGV"] + " - " + row["HoTenGV"].ToString()
                });
            }

            ViewBag.DanhSachGiangVien = danhSach;
        }
    }
}