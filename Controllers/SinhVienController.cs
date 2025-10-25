
using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class SinhVienController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // GET: SinhVien
        public ActionResult Index(string searchString)
        {
            List<SinhVien> danhSachSV = new List<SinhVien>();

            string query = @"SELECT sv.MaSV, sv.HoTenSV, sv.NgaySinh, sv.GioiTinh, 
                            sv.DiaChi, sv.Email, sv.SoDT, sv.MaLop, l.TenLop
                            FROM SinhVien sv
                            LEFT JOIN Lop l ON sv.MaLop = l.MaLop
                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (sv.MaSV LIKE @Search OR sv.HoTenSV LIKE @Search OR sv.Email LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY sv.MaSV";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                SinhVien sv = new SinhVien
                {
                    MaSV = row["MaSV"].ToString(),
                    HoTenSV = row["HoTenSV"].ToString(),
                    NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                    GioiTinh = row["GioiTinh"].ToString(),
                    DiaChi = row["DiaChi"].ToString(),
                    Email = row["Email"].ToString(),
                    SoDT = row["SoDT"].ToString(),
                    MaLop = row["MaLop"].ToString(),
                    TenLop = row["TenLop"].ToString()
                };
                danhSachSV.Add(sv);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachSV);
        }

        // GET: SinhVien/Create
        public ActionResult Create()
        {
            LoadDanhSachLop();
            return View();
        }

        // POST: SinhVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SinhVien sinhVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"INSERT INTO SinhVien (MaSV, HoTenSV, NgaySinh, GioiTinh, DiaChi, Email, SoDT, MaLop)
                                   VALUES (@MaSV, @HoTenSV, @NgaySinh, @GioiTinh, @DiaChi, @Email, @SoDT, @MaLop)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaSV", sinhVien.MaSV),
                        new SqlParameter("@HoTenSV", sinhVien.HoTenSV),
                        new SqlParameter("@NgaySinh", (object)sinhVien.NgaySinh ?? DBNull.Value),
                        new SqlParameter("@GioiTinh", (object)sinhVien.GioiTinh ?? DBNull.Value),
                        new SqlParameter("@DiaChi", (object)sinhVien.DiaChi ?? DBNull.Value),
                        new SqlParameter("@Email", sinhVien.Email),
                        new SqlParameter("@SoDT", (object)sinhVien.SoDT ?? DBNull.Value),
                        new SqlParameter("@MaLop", sinhVien.MaLop)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm sinh viên thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể thêm sinh viên. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachLop();
            return View(sinhVien);
        }

        // GET: SinhVien/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT * FROM SinhVien WHERE MaSV = @MaSV";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSV", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            SinhVien sinhVien = new SinhVien
            {
                MaSV = row["MaSV"].ToString(),
                HoTenSV = row["HoTenSV"].ToString(),
                NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                GioiTinh = row["GioiTinh"].ToString(),
                DiaChi = row["DiaChi"].ToString(),
                Email = row["Email"].ToString(),
                SoDT = row["SoDT"].ToString(),
                MaLop = row["MaLop"].ToString()
            };

            LoadDanhSachLop();
            return View(sinhVien);
        }

        // POST: SinhVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SinhVien sinhVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE SinhVien 
                                   SET HoTenSV = @HoTenSV, 
                                       NgaySinh = @NgaySinh, 
                                       GioiTinh = @GioiTinh, 
                                       DiaChi = @DiaChi, 
                                       Email = @Email, 
                                       SoDT = @SoDT, 
                                       MaLop = @MaLop
                                   WHERE MaSV = @MaSV";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaSV", sinhVien.MaSV),
                        new SqlParameter("@HoTenSV", sinhVien.HoTenSV),
                        new SqlParameter("@NgaySinh", (object)sinhVien.NgaySinh ?? DBNull.Value),
                        new SqlParameter("@GioiTinh", (object)sinhVien.GioiTinh ?? DBNull.Value),
                        new SqlParameter("@DiaChi", (object)sinhVien.DiaChi ?? DBNull.Value),
                        new SqlParameter("@Email", sinhVien.Email),
                        new SqlParameter("@SoDT", (object)sinhVien.SoDT ?? DBNull.Value),
                        new SqlParameter("@MaLop", sinhVien.MaLop)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật sinh viên thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachLop();
            return View(sinhVien);
        }

        // GET: SinhVien/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT sv.*, l.TenLop 
                           FROM SinhVien sv
                           LEFT JOIN Lop l ON sv.MaLop = l.MaLop
                           WHERE sv.MaSV = @MaSV";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSV", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            SinhVien sinhVien = new SinhVien
            {
                MaSV = row["MaSV"].ToString(),
                HoTenSV = row["HoTenSV"].ToString(),
                NgaySinh = row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : (DateTime?)null,
                GioiTinh = row["GioiTinh"].ToString(),
                DiaChi = row["DiaChi"].ToString(),
                Email = row["Email"].ToString(),
                SoDT = row["SoDT"].ToString(),
                MaLop = row["MaLop"].ToString(),
                TenLop = row["TenLop"].ToString()
            };

            return View(sinhVien);
        }

        // POST: SinhVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Xóa đăng ký học phần trước
                string queryDK = "DELETE FROM DangKyHocPhan WHERE MaSV = @MaSV";
                db.ExecuteNonQuery(queryDK, new SqlParameter[] { new SqlParameter("@MaSV", id) });

                // Xóa sinh viên
                string query = "DELETE FROM SinhVien WHERE MaSV = @MaSV";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaSV", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa sinh viên thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper method để load danh sách lớp vào ViewBag
        private void LoadDanhSachLop()
        {
            string query = "SELECT MaLop, TenLop FROM Lop ORDER BY TenLop";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSachLop = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSachLop.Add(new SelectListItem
                {
                    Value = row["MaLop"].ToString(),
                    Text = row["TenLop"].ToString()
                });
            }

            ViewBag.DanhSachLop = danhSachLop;
        }
    }
}