using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class KhoaController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // GET: Khoa
        public ActionResult Index(string searchString)
        {
            List<Khoa> danhSachKhoa = new List<Khoa>();

            string query = @"SELECT MaKhoa, TenKhoa, SoDienThoai
                           FROM Khoa 
                           WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (MaKhoa LIKE @Search OR TenKhoa LIKE @Search)";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY MaKhoa";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                Khoa khoa = new Khoa
                {
                    MaKhoa = row["MaKhoa"].ToString(),
                    TenKhoa = row["TenKhoa"].ToString(),
                    SoDienThoai = row["SoDienThoai"].ToString(),
                    
                };
                danhSachKhoa.Add(khoa);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachKhoa);
        }

        // GET: Khoa/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Khoa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Khoa khoa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra trùng mã khoa
                    string checkQuery = "SELECT COUNT(*) FROM Khoa WHERE MaKhoa = @MaKhoa";
                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@MaKhoa", khoa.MaKhoa) }));

                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Mã khoa đã tồn tại!");
                        return View(khoa);
                    }

                    // Kiểm tra trùng tên khoa
                    string checkNameQuery = "SELECT COUNT(*) FROM Khoa WHERE TenKhoa = @TenKhoa";
                    int nameCount = Convert.ToInt32(db.ExecuteScalar(checkNameQuery,
                        new SqlParameter[] { new SqlParameter("@TenKhoa", khoa.TenKhoa) }));

                    if (nameCount > 0)
                    {
                        ModelState.AddModelError("", "Tên khoa đã tồn tại!");
                        return View(khoa);
                    }

                    string query = @"INSERT INTO Khoa (MaKhoa, TenKhoa, SoDienThoai)
                                   VALUES (@MaKhoa, @TenKhoa, @SoDienThoai)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaKhoa", khoa.MaKhoa),
                        new SqlParameter("@TenKhoa", khoa.TenKhoa),
                        new SqlParameter("@SoDienThoai", (object)khoa.SoDienThoai ?? DBNull.Value),
                        
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Thêm khoa thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(khoa);
        }

        // GET: Khoa/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = "SELECT * FROM Khoa WHERE MaKhoa = @MaKhoa";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaKhoa", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            Khoa khoa = new Khoa
            {
                MaKhoa = row["MaKhoa"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                SoDienThoai = row["SoDienThoai"].ToString()
                
            };

            return View(khoa);
        }

        // POST: Khoa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Khoa khoa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE Khoa 
                                   SET TenKhoa = @TenKhoa, 
                                       SoDienThoai = @SoDienThoai
                                       
                                   WHERE MaKhoa = @MaKhoa";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaKhoa", khoa.MaKhoa),
                        new SqlParameter("@TenKhoa", khoa.TenKhoa),
                        new SqlParameter("@SoDienThoai", (object)khoa.SoDienThoai ?? DBNull.Value)
                        
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật khoa thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(khoa);
        }

        // GET: Khoa/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = "SELECT * FROM Khoa WHERE MaKhoa = @MaKhoa";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaKhoa", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            Khoa khoa = new Khoa
            {
                MaKhoa = row["MaKhoa"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                SoDienThoai = row["SoDienThoai"].ToString()
                
            };

            return View(khoa);
        }

        // POST: Khoa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Kiểm tra khoa có lớp học nào không
                string checkLopQuery = "SELECT COUNT(*) FROM Lop WHERE MaKhoa = @MaKhoa";
                int soLop = Convert.ToInt32(db.ExecuteScalar(checkLopQuery,
                    new SqlParameter[] { new SqlParameter("@MaKhoa", id) }));

                if (soLop > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa khoa này vì còn {soLop} lớp học!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra khoa có giáo viên nào không
                string checkGVQuery = "SELECT COUNT(*) FROM GiangVien WHERE MaKhoa = @MaKhoa";
                int soGV = Convert.ToInt32(db.ExecuteScalar(checkGVQuery,
                    new SqlParameter[] { new SqlParameter("@MaKhoa", id) }));

                if (soGV > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa khoa này vì còn {soGV} giáo viên!";
                    return RedirectToAction("Index");
                }

                string query = "DELETE FROM Khoa WHERE MaKhoa = @MaKhoa";
                int result = db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaKhoa", id) });

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa khoa thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Khoa/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = @"SELECT k.*, 
                                   COUNT(DISTINCT l.MaLop) AS SoLop,
                                   COUNT(DISTINCT gv.MaGV) AS SoGiangVien
                            FROM Khoa k
                            LEFT JOIN Lop l ON k.MaKhoa = l.MaKhoa
                            LEFT JOIN GiangVien gv ON k.MaKhoa = gv.MaKhoa
                            WHERE k.MaKhoa = @MaKhoa
                            GROUP BY k.MaKhoa, k.TenKhoa, k.SoDienThoai";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaKhoa", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            ViewBag.SoLop = row["SoLop"];
            ViewBag.SoGiangVien = row["SoGiangVien"];

            Khoa khoa = new Khoa
            {
                MaKhoa = row["MaKhoa"].ToString(),
                TenKhoa = row["TenKhoa"].ToString(),
                SoDienThoai = row["SoDienThoai"].ToString()
                
            };

            return View(khoa);
        }
    }
}