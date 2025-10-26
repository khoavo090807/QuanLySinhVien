using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class GiangVienController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // GET: GiangVien
        public ActionResult Index(string searchString)
        {
            List<GiangVien> danhSachGV = new List<GiangVien>();

            string query = @"SELECT gv.MaGV, gv.HoTenGV, gv.NgaySinh, gv.GioiTinh, 
                            gv.Email, gv.SoDT, gv.MaKhoa, gv.MaChucVu, 
                            k.TenKhoa, cv.TenChucVu
                            FROM GiangVien gv
                            LEFT JOIN Khoa k ON gv.MaKhoa = k.MaKhoa
                            LEFT JOIN ChucVu cv ON gv.MaChucVu = cv.MaChucVu
                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (gv.MaGV LIKE @Search OR gv.HoTenGV LIKE @Search OR gv.Email LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY gv.MaGV";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                GiangVien gv = new GiangVien
                {
                    MaGV = row["MaGV"].ToString(),
                    HoTenGV = row["HoTenGV"].ToString(),
                    NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                    GioiTinh = row["GioiTinh"].ToString(),
                    Email = row["Email"].ToString(),
                    SoDT = row["SoDT"].ToString(),
                    MaKhoa = row["MaKhoa"].ToString(),
                    MaChucVu = row["MaChucVu"].ToString(),
                    TenKhoa = row["TenKhoa"].ToString(),
                    TenChucVu = row["TenChucVu"].ToString()
                };
                danhSachGV.Add(gv);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachGV);
        }

        // GET: GiangVien/Create
        public ActionResult Create()
        {
            LoadDanhSachKhoa();
            LoadDanhSachChucVu();
            return View();
        }

        // POST: GiangVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GiangVien giangVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"INSERT INTO GiangVien (MaGV, HoTenGV, NgaySinh, GioiTinh, Email, SoDT, MaKhoa, MaChucVu)
                                   VALUES (@MaGV, @HoTenGV, @NgaySinh, @GioiTinh, @Email, @SoDT, @MaKhoa, @MaChucVu)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaGV", giangVien.MaGV),
                        new SqlParameter("@HoTenGV", giangVien.HoTenGV),
                        new SqlParameter("@NgaySinh", (object)giangVien.NgaySinh ?? DBNull.Value),
                        new SqlParameter("@GioiTinh", (object)giangVien.GioiTinh ?? DBNull.Value),
                        new SqlParameter("@Email", giangVien.Email),
                        new SqlParameter("@SoDT", (object)giangVien.SoDT ?? DBNull.Value),
                        new SqlParameter("@MaKhoa", (object)giangVien.MaKhoa ?? DBNull.Value),
                        new SqlParameter("@MaChucVu", (object)giangVien.MaChucVu ?? DBNull.Value)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm giáo viên thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể thêm giáo viên. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            LoadDanhSachChucVu();
            return View(giangVien);
        }

        // GET: GiangVien/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT gv.*, k.TenKhoa, cv.TenChucVu
                           FROM GiangVien gv
                           LEFT JOIN Khoa k ON gv.MaKhoa = k.MaKhoa
                           LEFT JOIN ChucVu cv ON gv.MaChucVu = cv.MaChucVu
                           WHERE gv.MaGV = @MaGV";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaGV", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            GiangVien giangVien = new GiangVien
            {
                MaGV = row["MaGV"].ToString(),
                HoTenGV = row["HoTenGV"].ToString(),
                NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                GioiTinh = row["GioiTinh"].ToString(),
                Email = row["Email"].ToString(),
                SoDT = row["SoDT"].ToString(),
                MaKhoa = row["MaKhoa"].ToString(),
                MaChucVu = row["MaChucVu"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                TenChucVu = row["TenChucVu"].ToString()
            };

            LoadDanhSachKhoa();
            LoadDanhSachChucVu();
            return View(giangVien);
        }

        // POST: GiangVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GiangVien giangVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE GiangVien 
                                   SET HoTenGV = @HoTenGV, 
                                       NgaySinh = @NgaySinh, 
                                       GioiTinh = @GioiTinh, 
                                       Email = @Email, 
                                       SoDT = @SoDT, 
                                       MaKhoa = @MaKhoa,
                                       MaChucVu = @MaChucVu
                                   WHERE MaGV = @MaGV";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaGV", giangVien.MaGV),
                        new SqlParameter("@HoTenGV", giangVien.HoTenGV),
                        new SqlParameter("@NgaySinh", (object)giangVien.NgaySinh ?? DBNull.Value),
                        new SqlParameter("@GioiTinh", (object)giangVien.GioiTinh ?? DBNull.Value),
                        new SqlParameter("@Email", giangVien.Email),
                        new SqlParameter("@SoDT", (object)giangVien.SoDT ?? DBNull.Value),
                        new SqlParameter("@MaKhoa", (object)giangVien.MaKhoa ?? DBNull.Value),
                        new SqlParameter("@MaChucVu", (object)giangVien.MaChucVu ?? DBNull.Value)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật giáo viên thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            LoadDanhSachChucVu();
            return View(giangVien);
        }

        // GET: GiangVien/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT gv.*, k.TenKhoa, cv.TenChucVu
                           FROM GiangVien gv
                           LEFT JOIN Khoa k ON gv.MaKhoa = k.MaKhoa
                           LEFT JOIN ChucVu cv ON gv.MaChucVu = cv.MaChucVu
                           WHERE gv.MaGV = @MaGV";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaGV", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            GiangVien giangVien = new GiangVien
            {
                MaGV = row["MaGV"].ToString(),
                HoTenGV = row["HoTenGV"].ToString(),
                NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                GioiTinh = row["GioiTinh"].ToString(),
                Email = row["Email"].ToString(),
                SoDT = row["SoDT"].ToString(),
                MaKhoa = row["MaKhoa"].ToString(),
                MaChucVu = row["MaChucVu"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                TenChucVu = row["TenChucVu"].ToString()
            };

            return View(giangVien);
        }

        // POST: GiangVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Kiểm tra xem giáo viên có dạy lớp học phần nào không
                string checkQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaGV = @MaGV";
                int soLHP = Convert.ToInt32(db.ExecuteScalar(checkQuery, new SqlParameter[] { new SqlParameter("@MaGV", id) }));

                if (soLHP > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa giáo viên này vì còn dạy {soLHP} lớp học phần!";
                    return RedirectToAction("Index");
                }

                string query = "DELETE FROM GiangVien WHERE MaGV = @MaGV";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaGV", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa giáo viên thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper methods
        private void LoadDanhSachKhoa()
        {
            string query = "SELECT MaKhoa, TenKhoa FROM Khoa ORDER BY TenKhoa";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSachKhoa = new List<SelectListItem>();
            danhSachKhoa.Add(new SelectListItem { Value = "", Text = "-- Chọn khoa --" });
            foreach (DataRow row in dt.Rows)
            {
                danhSachKhoa.Add(new SelectListItem
                {
                    Value = row["MaKhoa"].ToString(),
                    Text = row["TenKhoa"].ToString()
                });
            }

            ViewBag.DanhSachKhoa = danhSachKhoa;
        }

        private void LoadDanhSachChucVu()
        {
            string query = "SELECT MaChucVu, TenChucVu FROM ChucVu ORDER BY TenChucVu";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSachChucVu = new List<SelectListItem>();
            danhSachChucVu.Add(new SelectListItem { Value = "", Text = "-- Chọn chức vụ --" });
            foreach (DataRow row in dt.Rows)
            {
                danhSachChucVu.Add(new SelectListItem
                {
                    Value = row["MaChucVu"].ToString(),
                    Text = row["TenChucVu"].ToString()
                });
            }

            ViewBag.DanhSachChucVu = danhSachChucVu;
        }
    }
}