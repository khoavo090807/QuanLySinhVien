
using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class MonHocController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        public ActionResult Index(string searchString)
        {
            List<MonHoc> danhSachMh = new List<MonHoc>();

            string query = @"SELECT * 
                            FROM MonHoc 

                            WHERE 1=1";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (MonHoc.MaMH LIKE @Search OR MonHoc.TenMH LIKE @Search OR MonHoc.SoTinChi LIKE @Search )";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", "%" + searchString + "%")
                };
            }

            query += " ORDER BY MonHoc.SoTinChi";

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                MonHoc mh = new MonHoc
                {
                    MaMH = row["MaMH"].ToString(),
                    TenMH = row["TenMH"].ToString(),
                    SoTinChi=Convert.ToInt32(row["SoTinChi"]),
                    
                };
                danhSachMh.Add(mh);
            }

            ViewBag.SearchString = searchString;
            return View(danhSachMh);
        }
        // GET: MonHoc
        //public ActionResult Index()
        //{
        //    List<MonHoc> danhSachMH = new List<MonHoc>();

        //    string query = "SELECT * FROM MonHoc ORDER BY MaMH";
        //    DataTable dt = db.ExecuteQuery(query);

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        MonHoc mh = new MonHoc
        //        {
        //            MaMH = row["MaMH"].ToString(),
        //            TenMH = row["TenMH"].ToString(),
        //            SoTinChi = Convert.ToInt32(row["SoTinChi"])
        //        };
        //        danhSachMH.Add(mh);
        //    }

        //    return View(danhSachMH);
        //}

        // GET: MonHoc/Create
        public ActionResult Create()
        {
            LoadDanhSachMonHoc();
            return View();
        }

        // POST: MonHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MonHoc monHoc, string MaMHTienQuyet, int? HocKy, DateTime? ThoiGianBatDau)
        {
            LoadDanhSachMonHoc();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Insert môn học chính
                    string query = @"INSERT INTO MonHoc (MaMH, TenMH, SoTinChi)
                                   VALUES (@MaMH, @TenMH, @SoTinChi)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaMH", monHoc.MaMH),
                        new SqlParameter("@TenMH", monHoc.TenMH),
                        new SqlParameter("@SoTinChi", monHoc.SoTinChi)
                    };

                    db.ExecuteNonQuery(query, parameters);

                    // 2. Insert môn học tiên quyết nếu có chọn
                    if (!string.IsNullOrEmpty(MaMHTienQuyet))
                    {
                        string prereqQuery = @"INSERT INTO MonHoc_TienQuyet (MaMH, MaMHTienQuyet)
                                            VALUES (@MaMH, @MHTienQuyet)";

                        SqlParameter[] prereqParams = new SqlParameter[]
                        {
                            new SqlParameter("@MaMH", monHoc.MaMH),
                            new SqlParameter("@MHTienQuyet", MaMHTienQuyet)
                        };

                        db.ExecuteNonQuery(prereqQuery, prereqParams);
                    }

                    // 3. Nếu có học kỳ và thời gian, có thể tạo LopHocPhan nếu cần
                    if (HocKy.HasValue && ThoiGianBatDau.HasValue)
                    {
                        string checkHKQuery = "SELECT COUNT(*) FROM HocKy WHERE MaHK = @MaHK";
                        SqlParameter[] checkHKParams = new SqlParameter[]
                        {
                            new SqlParameter("@MaHK", "HK" + HocKy.Value)
                        };

                        int hkExists = Convert.ToInt32(db.ExecuteScalar(checkHKQuery, checkHKParams));

                        if (hkExists > 0)
                        {
                            string lhpQuery = @"INSERT INTO LopHocPhan (MaLHP, MaMH, MaHK)
                                             VALUES (@MaLHP, @MaMH, @MaHK)";

                            string maLHP = monHoc.MaMH + "_" + HocKy.Value;

                            SqlParameter[] lhpParams = new SqlParameter[]
                            {
                                new SqlParameter("@MaLHP", maLHP),
                                new SqlParameter("@MaMH", monHoc.MaMH),
                                new SqlParameter("@MaHK", "HK" + HocKy.Value)
                            };

                            db.ExecuteNonQuery(lhpQuery, lhpParams);
                        }
                    }

                    TempData["SuccessMessage"] = "Thêm môn học thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(monHoc);
        }

        // GET: MonHoc/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = "SELECT * FROM MonHoc WHERE MaMH = @MaMH";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaMH", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            MonHoc monHoc = new MonHoc
            {
                MaMH = row["MaMH"].ToString(),
                TenMH = row["TenMH"].ToString(),
                SoTinChi = Convert.ToInt32(row["SoTinChi"])
            };

            return View(monHoc);
        }

        // POST: MonHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MonHoc monHoc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE MonHoc 
                                   SET TenMH = @TenMH, 
                                       SoTinChi = @SoTinChi
                                   WHERE MaMH = @MaMH";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaMH", monHoc.MaMH),
                        new SqlParameter("@TenMH", monHoc.TenMH),
                        new SqlParameter("@SoTinChi", monHoc.SoTinChi)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật môn học thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(monHoc);
        }

        // GET: MonHoc/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            string query = "SELECT * FROM MonHoc WHERE MaMH = @MaMH";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaMH", id)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            MonHoc monHoc = new MonHoc
            {
                MaMH = row["MaMH"].ToString(),
                TenMH = row["TenMH"].ToString(),
                SoTinChi = Convert.ToInt32(row["SoTinChi"])
            };

            return View(monHoc);
        }

        // POST: MonHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                // Sử dụng stored procedure để xóa an toàn
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaMH", id)
                };

                db.ExecuteStoredProcedureNonQuery("sp_XoaMonHoc", parameters);

                // Kiểm tra xem xóa thực sự thành công chưa
                string checkQuery = "SELECT COUNT(*) FROM MonHoc WHERE MaMH = @MaMH";
                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@MaMH", id)
                };
                int remainingCourses = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                if (remainingCourses == 0)
                {
                    // Thực sự xóa thành công
                    TempData["SuccessMessage"] = "Xóa môn học thành công!";
                }
                else
                {
                    // Xóa không thành công (môn học vẫn tồn tại)
                    // Kiểm tra lý do cụ thể để đưa ra thông báo phù hợp
                    string checkLHPQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaMH = @MaMH";
                    SqlParameter[] checkLHPParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaMH", id)
                    };
                    int lhpCount = Convert.ToInt32(db.ExecuteScalar(checkLHPQuery, checkLHPParams));

                    if (lhpCount > 0)
                    {
                        TempData["ErrorMessage"] = $"Lỗi: Không thể xóa môn [{id}]. Môn học này đã được mở lớp học phần.";
                    }
                    else
                    {
                        string checkTienQuyetQuery = "SELECT COUNT(*) FROM MonHoc_TienQuyet WHERE MaMH_TienQuyet = @MaMH";
                        SqlParameter[] checkTienQuyetParams = new SqlParameter[]
                        {
                            new SqlParameter("@MaMH", id)
                        };
                        int tienQuyetCount = Convert.ToInt32(db.ExecuteScalar(checkTienQuyetQuery, checkTienQuyetParams));

                        if (tienQuyetCount > 0)
                        {
                            TempData["ErrorMessage"] = $"Lỗi: Không thể xóa môn [{id}]. Môn học này đang là tiên quyết cho môn khác.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Lỗi: Không thể xóa môn [{id}]. Vui lòng kiểm tra lại.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi cụ thể của SQL Server
                if (ex.Number == 2812) // Could not find stored procedure
                {
                    TempData["ErrorMessage"] = "Lỗi: Stored procedure 'sp_XoaMonHoc' chưa được tạo trong database. Vui lòng thực thi script CauTrucXuLy_Final.sql trước.";
                }
                else if (ex.Number == 50000) // Custom RAISERROR messages from procedures
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi SQL Server: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper method to load danh sách môn học cho dropdown
        private void LoadDanhSachMonHoc()
        {
            string query = "SELECT MaMH, TenMH FROM MonHoc ORDER BY MaMH";
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
    }
}