
using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class LopController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // GET: Lop
        public ActionResult Index(string searchString)
        {
            List<Lop> danhSachLop = new List<Lop>();

            string query = @"SELECT l.MaLop, l.TenLop, l.SiSo, l.MaKhoa, l.MaHeDT,
                            k.TenKhoa, h.TenHeDT
                            FROM Lop l
                            LEFT JOIN Khoa k ON l.MaKhoa = k.MaKhoa
                            LEFT JOIN HeDaoTao h ON l.MaHeDT = h.MaHeDT
                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (l.MaLop LIKE @Search OR l.TenLop LIKE @Search OR k.TenKhoa LIKE @Search OR h.TenHeDT LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY  l.MaLop";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                Lop lop = new Lop
                {
                    MaLop = row["MaLop"].ToString(),
                    TenLop = row["TenLop"].ToString(),
                    SiSo = Convert.ToInt32(row["SiSo"]),
                    MaKhoa = row["MaKhoa"].ToString(),
                    MaHeDT = row["MaHeDT"].ToString(),
                    TenKhoa = row["TenKhoa"].ToString(),
                    TenHeDT = row["TenHeDT"].ToString()


                };
                danhSachLop.Add(lop);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachLop);
        }


        // GET: Lop/Create
        public ActionResult Create()
        {
            LoadDanhSachKhoa();
            LoadDanhSachHeDaoTao();
            return View();
        }

        // POST: Lop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Lop lop)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"INSERT INTO Lop (MaLop, TenLop, SiSo, MaKhoa, MaHeDT)
                                   VALUES (@MaLop, @TenLop, @SiSo, @MaKhoa, @MaHeDT)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaLop", lop.MaLop),
                        new SqlParameter("@TenLop", lop.TenLop),
                        new SqlParameter("@SiSo", lop.SiSo),
                        new SqlParameter("@MaKhoa", (object)lop.MaKhoa ?? DBNull.Value),
                        new SqlParameter("@MaHeDT", (object)lop.MaHeDT ?? DBNull.Value)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm lớp học thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            LoadDanhSachHeDaoTao();
            return View(lop);
        }

        // GET: Lop/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = "SELECT * FROM Lop WHERE MaLop = @MaLop";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaLop", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            Lop lop = new Lop
            {
                MaLop = row["MaLop"].ToString(),
                TenLop = row["TenLop"].ToString(),
                SiSo = row["SiSo"] != DBNull.Value ? Convert.ToInt32(row["SiSo"]) : 0,
                MaKhoa = row["MaKhoa"].ToString(),
                MaHeDT = row["MaHeDT"].ToString()
            };

            LoadDanhSachKhoa();
            LoadDanhSachHeDaoTao();
            return View(lop);
        }

        // POST: Lop/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Lop lop)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE Lop 
                                   SET TenLop = @TenLop, 
                                       SiSo = @SiSo,
                                       MaKhoa = @MaKhoa,
                                       MaHeDT = @MaHeDT
                                   WHERE MaLop = @MaLop";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaLop", lop.MaLop),
                        new SqlParameter("@TenLop", lop.TenLop),
                        new SqlParameter("@SiSo", lop.SiSo),
                        new SqlParameter("@MaKhoa", (object)lop.MaKhoa ?? DBNull.Value),
                        new SqlParameter("@MaHeDT", (object)lop.MaHeDT ?? DBNull.Value)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            LoadDanhSachHeDaoTao();
            return View(lop);
        }

        // GET: Lop/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT l.*, k.TenKhoa, h.TenHeDT
                           FROM Lop l
                           LEFT JOIN Khoa k ON l.MaKhoa = k.MaKhoa
                           LEFT JOIN HeDaoTao h ON l.MaHeDT = h.MaHeDT
                           WHERE l.MaLop = @MaLop";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaLop", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            Lop lop = new Lop
            {
                MaLop = row["MaLop"].ToString(),
                TenLop = row["TenLop"].ToString(),
                SiSo = row["SiSo"] != DBNull.Value ? Convert.ToInt32(row["SiSo"]) : 0,
                MaKhoa = row["MaKhoa"].ToString(),
                MaHeDT = row["MaHeDT"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                TenHeDT = row["TenHeDT"].ToString()
            };

            return View(lop);
        }

        // POST: Lop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Kiểm tra xem lớp còn sinh viên không
                string checkQuery = "SELECT COUNT(*) FROM SinhVien WHERE MaLop = @MaLop";
                int soSV = Convert.ToInt32(db.ExecuteScalar(checkQuery, new SqlParameter[] { new SqlParameter("@MaLop", id) }));

                if (soSV > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa lớp này vì còn {soSV} sinh viên!";
                    return RedirectToAction("Index");
                }

                string query = "DELETE FROM Lop WHERE MaLop = @MaLop";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaLop", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa lớp học thành công!";
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

        private void LoadDanhSachHeDaoTao()
        {
            string query = "SELECT MaHeDT, TenHeDT FROM HeDaoTao ORDER BY TenHeDT";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSachHeDT = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSachHeDT.Add(new SelectListItem
                {
                    Value = row["MaHeDT"].ToString(),
                    Text = row["TenHeDT"].ToString()
                });
            }

            ViewBag.DanhSachHeDT = danhSachHeDT;
        }
    }
}