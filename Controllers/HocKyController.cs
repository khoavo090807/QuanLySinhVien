// ============================================
// FILE: Controllers/HocKyController.cs
// CHỨC NĂNG: Quản lý Học kỳ (Thêm, Sửa, Xóa, Xem)
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
    public class HocKyController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ========== GET: HocKy/Index - Xem danh sách học kỳ ==========
        public ActionResult Index(string searchString)
        {
            List<HocKy> danhSachHK = new List<HocKy>();

            string query = @"SELECT 
                            MaHK, 
                            TenHK, 
                            NamHoc, 
                            NgayBatDau, 
                            NgayKetThuc,
                            (SELECT COUNT(*) FROM LopHocPhan WHERE MaHK = HocKy.MaHK) AS SoLopHocPhan,
                            (SELECT COUNT(DISTINCT dk.MaSV) 
                             FROM DangKyHocPhan dk 
                             INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP 
                             WHERE lhp.MaHK = HocKy.MaHK) AS SoSinhVienDangKy
                            FROM HocKy
                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (MaHK LIKE @Search OR TenHK LIKE @Search OR NamHoc LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY NgayBatDau DESC";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                HocKy hk = new HocKy
                {
                    MaHK = row["MaHK"].ToString(),
                    TenHK = row["TenHK"].ToString(),
                    NamHoc = row["NamHoc"].ToString(),
                    NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                    NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]),
                    SoLopHocPhan = row["SoLopHocPhan"] != DBNull.Value ? Convert.ToInt32(row["SoLopHocPhan"]) : 0,
                    SoSinhVienDangKy = row["SoSinhVienDangKy"] != DBNull.Value ? Convert.ToInt32(row["SoSinhVienDangKy"]) : 0
                };
                danhSachHK.Add(hk);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachHK);
        }

        // ========== GET: HocKy/Create - Form thêm học kỳ ==========
        public ActionResult Create()
        {
            return View();
        }

        // ========== POST: HocKy/Create - Xử lý thêm học kỳ ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HocKy hocKy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra trùng mã học kỳ
                    string checkQuery = "SELECT COUNT(*) FROM HocKy WHERE MaHK = @MaHK";
                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@MaHK", hocKy.MaHK) }));

                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Mã học kỳ đã tồn tại!");
                        return View(hocKy);
                    }

                    // Kiểm tra ngày bắt đầu < ngày kết thúc
                    if (hocKy.NgayBatDau >= hocKy.NgayKetThuc)
                    {
                        ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc!");
                        return View(hocKy);
                    }

                    // Kiểm tra không trùng lịch học kỳ
                    string checkOverlapQuery = @"
                        SELECT COUNT(*) FROM HocKy 
                        WHERE NamHoc = @NamHoc 
                        AND MaHK != @MaHK
                        AND (NgayBatDau <= @NgayKetThuc AND NgayKetThuc >= @NgayBatDau)";

                    SqlParameter[] checkOverlapParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaHK", hocKy.MaHK),
                        new SqlParameter("@NamHoc", hocKy.NamHoc),
                        new SqlParameter("@NgayBatDau", hocKy.NgayBatDau),
                        new SqlParameter("@NgayKetThuc", hocKy.NgayKetThuc)
                    };

                    int overlapCount = Convert.ToInt32(db.ExecuteScalar(checkOverlapQuery, checkOverlapParams));
                    if (overlapCount > 0)
                    {
                        ModelState.AddModelError("", "Học kỳ này trùng lịch với học kỳ khác trong cùng năm!");
                        return View(hocKy);
                    }

                    // Thêm học kỳ mới
                    string query = @"INSERT INTO HocKy (MaHK, TenHK, NamHoc, NgayBatDau, NgayKetThuc)
                                   VALUES (@MaHK, @TenHK, @NamHoc, @NgayBatDau, @NgayKetThuc)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaHK", hocKy.MaHK),
                        new SqlParameter("@TenHK", hocKy.TenHK),
                        new SqlParameter("@NamHoc", hocKy.NamHoc),
                        new SqlParameter("@NgayBatDau", hocKy.NgayBatDau),
                        new SqlParameter("@NgayKetThuc", hocKy.NgayKetThuc)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm học kỳ thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể thêm học kỳ. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(hocKy);
        }

        // ========== GET: HocKy/Edit/5 - Form chỉnh sửa học kỳ ==========
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = "SELECT * FROM HocKy WHERE MaHK = @MaHK";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHK", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            HocKy hocKy = new HocKy
            {
                MaHK = row["MaHK"].ToString(),
                TenHK = row["TenHK"].ToString(),
                NamHoc = row["NamHoc"].ToString(),
                NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"])
            };

            return View(hocKy);
        }

        // ========== POST: HocKy/Edit/5 - Xử lý cập nhật học kỳ ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HocKy hocKy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra ngày bắt đầu < ngày kết thúc
                    if (hocKy.NgayBatDau >= hocKy.NgayKetThuc)
                    {
                        ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc!");
                        return View(hocKy);
                    }

                    string query = @"UPDATE HocKy 
                                   SET TenHK = @TenHK, 
                                       NamHoc = @NamHoc, 
                                       NgayBatDau = @NgayBatDau, 
                                       NgayKetThuc = @NgayKetThuc
                                   WHERE MaHK = @MaHK";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaHK", hocKy.MaHK),
                        new SqlParameter("@TenHK", hocKy.TenHK),
                        new SqlParameter("@NamHoc", hocKy.NamHoc),
                        new SqlParameter("@NgayBatDau", hocKy.NgayBatDau),
                        new SqlParameter("@NgayKetThuc", hocKy.NgayKetThuc)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật học kỳ thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(hocKy);
        }

        // ========== GET: HocKy/Delete/5 - Xác nhận xóa học kỳ ==========
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = @"SELECT hk.*, 
                            (SELECT COUNT(*) FROM LopHocPhan WHERE MaHK = hk.MaHK) AS SoLopHocPhan,
                            (SELECT COUNT(DISTINCT dk.MaSV) 
                             FROM DangKyHocPhan dk 
                             INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP 
                             WHERE lhp.MaHK = hk.MaHK) AS SoSinhVienDangKy
                            FROM HocKy hk
                            WHERE hk.MaHK = @MaHK";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHK", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            HocKy hocKy = new HocKy
            {
                MaHK = row["MaHK"].ToString(),
                TenHK = row["TenHK"].ToString(),
                NamHoc = row["NamHoc"].ToString(),
                NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]),
                SoLopHocPhan = row["SoLopHocPhan"] != DBNull.Value ? Convert.ToInt32(row["SoLopHocPhan"]) : 0,
                SoSinhVienDangKy = row["SoSinhVienDangKy"] != DBNull.Value ? Convert.ToInt32(row["SoSinhVienDangKy"]) : 0
            };

            ViewBag.SoLopHocPhan = hocKy.SoLopHocPhan;
            ViewBag.SoSinhVienDangKy = hocKy.SoSinhVienDangKy;

            return View(hocKy);
        }

        // ========== POST: HocKy/Delete/5 - Xử lý xóa học kỳ ==========
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Kiểm tra xem học kỳ có lớp học phần không
                string checkLHPQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaHK = @MaHK";
                int soLHP = Convert.ToInt32(db.ExecuteScalar(checkLHPQuery,
                    new SqlParameter[] { new SqlParameter("@MaHK", id) }));

                if (soLHP > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa học kỳ này vì còn {soLHP} lớp học phần!";
                    return RedirectToAction("Index");
                }

                string query = "DELETE FROM HocKy WHERE MaHK = @MaHK";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaHK", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa học kỳ thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ========== GET: HocKy/Details/5 - Xem chi tiết học kỳ ==========
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = @"SELECT hk.*, 
                            (SELECT COUNT(*) FROM LopHocPhan WHERE MaHK = hk.MaHK) AS SoLopHocPhan,
                            (SELECT COUNT(DISTINCT dk.MaSV) 
                             FROM DangKyHocPhan dk 
                             INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP 
                             WHERE lhp.MaHK = hk.MaHK) AS SoSinhVienDangKy
                            FROM HocKy hk
                            WHERE hk.MaHK = @MaHK";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHK", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            HocKy hocKy = new HocKy
            {
                MaHK = row["MaHK"].ToString(),
                TenHK = row["TenHK"].ToString(),
                NamHoc = row["NamHoc"].ToString(),
                NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]),
                SoLopHocPhan = row["SoLopHocPhan"] != DBNull.Value ? Convert.ToInt32(row["SoLopHocPhan"]) : 0,
                SoSinhVienDangKy = row["SoSinhVienDangKy"] != DBNull.Value ? Convert.ToInt32(row["SoSinhVienDangKy"]) : 0
            };

            // Trạng thái học kỳ (Chưa bắt đầu, Đang mở, Đã kết thúc)
            DateTime now = DateTime.Now;
            if (now < hocKy.NgayBatDau)
                ViewBag.TrangThai = "Chưa bắt đầu";
            else if (now > hocKy.NgayKetThuc)
                ViewBag.TrangThai = "Đã kết thúc";
            else
                ViewBag.TrangThai = "Đang mở";

            return View(hocKy);
        }

        // ========== GET: HocKy/GetHocKyDangMo - Lấy học kỳ đang mở (JSON) ==========
        [HttpGet]
        public JsonResult GetHocKyDangMo()
        {
            List<object> danhSach = new List<object>();

            string query = @"SELECT MaHK, TenHK, NgayBatDau, NgayKetThuc
                            FROM HocKy
                            WHERE NgayBatDau <= GETDATE() AND NgayKetThuc >= GETDATE()
                            ORDER BY NgayBatDau DESC";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new
                {
                    id = row["MaHK"].ToString(),
                    text = row["TenHK"] + " (" + Convert.ToDateTime(row["NgayBatDau"]).ToString("dd/MM/yyyy") +
                           " - " + Convert.ToDateTime(row["NgayKetThuc"]).ToString("dd/MM/yyyy") + ")"
                });
            }

            return Json(danhSach, JsonRequestBehavior.AllowGet);
        }

        // ========== POST: HocKy/CheckRegistrationPeriod - Kiểm tra thời gian đăng ký ==========
        [HttpPost]
        public JsonResult CheckRegistrationPeriod(string maHK)
        {
            try
            {
                string query = @"SELECT NgayBatDau, NgayKetThuc, TenHK
                                FROM HocKy
                                WHERE MaHK = @MaHK";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaHK", maHK)
                };

                DataTable dt = db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count == 0)
                    return Json(new { success = false, message = "Học kỳ không tồn tại!" });

                DataRow row = dt.Rows[0];
                DateTime ngayBatDau = Convert.ToDateTime(row["NgayBatDau"]);
                DateTime ngayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]);
                string tenHK = row["TenHK"].ToString();
                DateTime now = DateTime.Now;

                if (now < ngayBatDau)
                    return Json(new
                    {
                        success = false,
                        message = $"Thời gian đăng ký học kỳ {tenHK} chưa mở. Vui lòng quay lại từ {ngayBatDau:dd/MM/yyyy}"
                    });

                if (now > ngayKetThuc)
                    return Json(new
                    {
                        success = false,
                        message = $"Thời gian đăng ký học kỳ {tenHK} đã đóng (kết thúc {ngayKetThuc:dd/MM/yyyy})"
                    });

                return Json(new
                {
                    success = true,
                    message = "Có thể đăng ký"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ========== HELPER METHODS ==========

        private void LoadDanhSachLopHocPhan()
        {
            string query = @"SELECT lhp.MaLHP, 
                                   mh.TenMH, 
                                   hk.TenHK, 
                                   gv.HoTenGV,
                                   CASE 
                                       WHEN GETDATE() < hk.NgayBatDau THEN '(Chưa mở) ' 
                                       WHEN GETDATE() > hk.NgayKetThuc THEN '(Đã đóng) '
                                       ELSE '(Đang mở) '
                                   END AS TrangThaiHK
                            FROM LopHocPhan lhp
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                            ORDER BY hk.NgayBatDau DESC, mh.TenMH";

            DataTable dt = db.ExecuteQuery(query);
            List<SelectListItem> danhSach = new List<SelectListItem>();

            foreach (DataRow row in dt.Rows)
            {
                string trangThaiHK = row["TrangThaiHK"].ToString();
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaLHP"].ToString(),
                    Text = row["MaLHP"] + " - " + row["TenMH"] + " (" + row["TenHK"] + ") " + trangThaiHK
                });
            }

            ViewBag.DanhSachLopHocPhan = danhSach;
        }
    }
}