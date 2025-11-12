using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OfficeOpenXml;

namespace QuanLySinhVien.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeThiController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // INDEX - Danh sách đề thi
        // ============================================
        public ActionResult Index(string searchString = "")
        {
            List<DeThi> danhSachDeThi = new List<DeThi>();

            string query = @"
                SELECT dt.MaDT, dt.TenDT, dt.MoTa, dt.MaKhoa, k.TenKhoa, 
                       dt.SoCau, dt.ThoiGianLamBai, dt.TrangThai, dt.NgayTao,
                       (SELECT COUNT(*) FROM CauHoi WHERE MaDT = dt.MaDT) AS TongCau
                FROM DeThi dt
                INNER JOIN Khoa k ON dt.MaKhoa = k.MaKhoa
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (dt.MaDT LIKE @Search OR dt.TenDT LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + searchString + "%"));
            }

            query += " ORDER BY dt.NgayTao DESC";

            DataTable dt_result = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt_result.Rows)
            {
                danhSachDeThi.Add(new DeThi
                {
                    MaDT = row["MaDT"].ToString(),
                    TenDT = row["TenDT"].ToString(),
                    MoTa = row["MoTa"] != DBNull.Value ? row["MoTa"].ToString() : "",
                    MaKhoa = row["MaKhoa"].ToString(),
                    TenKhoa = row["TenKhoa"].ToString(),
                    SoCau = Convert.ToInt32(row["TongCau"]),
                    ThoiGianLamBai = row["ThoiGianLamBai"] != DBNull.Value ? Convert.ToInt32(row["ThoiGianLamBai"]) : 0,
                    TrangThai = Convert.ToBoolean(row["TrangThai"]),
                    NgayTao = Convert.ToDateTime(row["NgayTao"])
                });
            }

            ViewBag.SearchString = searchString;
            return View(danhSachDeThi);
        }

        // ============================================
        // CREATE - Form tạo đề thi
        // ============================================
        public ActionResult Create()
        {
            LoadDanhSachKhoa();
            return View();
        }

        // ============================================
        // CREATE - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DeThi deThi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra mã đề thi đã tồn tại
                    string checkQuery = "SELECT COUNT(*) FROM DeThi WHERE MaDT = @MaDT";
                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@MaDT", deThi.MaDT) }));

                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Mã đề thi đã tồn tại!");
                        LoadDanhSachKhoa();
                        return View(deThi);
                    }

                    string query = @"
                        INSERT INTO DeThi (MaDT, TenDT, MoTa, MaKhoa, ThoiGianLamBai, TrangThai, NgayTao)
                        VALUES (@MaDT, @TenDT, @MoTa, @MaKhoa, @ThoiGianLamBai, @TrangThai, @NgayTao)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaDT", deThi.MaDT),
                        new SqlParameter("@TenDT", deThi.TenDT),
                        new SqlParameter("@MoTa", (object)deThi.MoTa ?? DBNull.Value),
                        new SqlParameter("@MaKhoa", deThi.MaKhoa),
                        new SqlParameter("@ThoiGianLamBai", (object)deThi.ThoiGianLamBai ?? DBNull.Value),
                        new SqlParameter("@TrangThai", deThi.TrangThai),
                        new SqlParameter("@NgayTao", DateTime.Now)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Tạo đề thi thành công!";
                        return RedirectToAction("Edit", new { id = deThi.MaDT });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            return View(deThi);
        }

        // ============================================
        // EDIT - Form chỉnh sửa đề thi
        // ============================================
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = "SELECT * FROM DeThi WHERE MaDT = @MaDT";
            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaDT", id) });

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            DeThi deThi = new DeThi
            {
                MaDT = row["MaDT"].ToString(),
                TenDT = row["TenDT"].ToString(),
                MoTa = row["MoTa"] != DBNull.Value ? row["MoTa"].ToString() : "",
                MaKhoa = row["MaKhoa"].ToString(),
                SoCau = Convert.ToInt32(row["SoCau"]),
                ThoiGianLamBai = row["ThoiGianLamBai"] != DBNull.Value ? Convert.ToInt32(row["ThoiGianLamBai"]) : 0,
                TrangThai = Convert.ToBoolean(row["TrangThai"]),
                NgayTao = Convert.ToDateTime(row["NgayTao"])
            };

            LoadDanhSachKhoa();
            return View(deThi);
        }

        // ============================================
        // EDIT - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DeThi deThi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"
                        UPDATE DeThi 
                        SET TenDT = @TenDT, MoTa = @MoTa, MaKhoa = @MaKhoa, 
                            ThoiGianLamBai = @ThoiGianLamBai, TrangThai = @TrangThai, 
                            NgayCapNhat = @NgayCapNhat
                        WHERE MaDT = @MaDT";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaDT", deThi.MaDT),
                        new SqlParameter("@TenDT", deThi.TenDT),
                        new SqlParameter("@MoTa", (object)deThi.MoTa ?? DBNull.Value),
                        new SqlParameter("@MaKhoa", deThi.MaKhoa),
                        new SqlParameter("@ThoiGianLamBai", (object)deThi.ThoiGianLamBai ?? DBNull.Value),
                        new SqlParameter("@TrangThai", deThi.TrangThai),
                        new SqlParameter("@NgayCapNhat", DateTime.Now)
                    };

                    int result = db.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Cập nhật đề thi thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadDanhSachKhoa();
            return View(deThi);
        }

        // ============================================
        // DELETE - Xóa đề thi
        // ============================================
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            string query = "SELECT * FROM DeThi WHERE MaDT = @MaDT";
            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaDT", id) });

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            DeThi deThi = new DeThi
            {
                MaDT = row["MaDT"].ToString(),
                TenDT = row["TenDT"].ToString(),
                SoCau = Convert.ToInt32(row["SoCau"])
            };

            return View(deThi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                db.ExecuteStoredProcedureNonQuery("sp_XoaDeThi",
                    new SqlParameter[] { new SqlParameter("@MaDT", id) });

                TempData["SuccessMessage"] = "Xóa đề thi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ============================================
        // HELPER - Load danh sách khoa
        // ============================================
        private void LoadDanhSachKhoa()
        {
            string query = "SELECT MaKhoa, TenKhoa FROM Khoa ORDER BY TenKhoa";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaKhoa"].ToString(),
                    Text = row["TenKhoa"].ToString()
                });
            }

            ViewBag.DanhSachKhoa = danhSach;
        }
    }
}