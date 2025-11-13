using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;

namespace QuanLySinhVien.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CauHoiController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // INDEX - Danh sách câu hỏi của một đề thi
        // ============================================
        public ActionResult Index(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            // Kiểm tra đề thi có tồn tại
            string checkDTQuery = "SELECT TenDT FROM DeThi WHERE MaDT = @MaDT";
            DataTable checkDT = db.ExecuteQuery(checkDTQuery,
                new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

            if (checkDT.Rows.Count == 0)
                return HttpNotFound();

            ViewBag.TenDeThi = checkDT.Rows[0]["TenDT"].ToString();
            ViewBag.MaDT = maDT;

            List<CauHoi> danhSachCauHoi = new List<CauHoi>();

            string query = @"
                SELECT MaCauHoi, MaDT, NoiDungCau, LoaiCau, DiemCau, ThuTu, DapAnDung
                FROM CauHoi
                WHERE MaDT = @MaDT
                ORDER BY ThuTu ASC";

            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

            foreach (DataRow row in dt.Rows)
            {
                danhSachCauHoi.Add(new CauHoi
                {
                    MaCauHoi = Convert.ToInt32(row["MaCauHoi"]),
                    MaDT = row["MaDT"].ToString(),
                    NoiDungCau = row["NoiDungCau"].ToString(),
                    LoaiCau = row["LoaiCau"].ToString(),
                    DiemCau = Convert.ToSingle(row["DiemCau"]),
                    ThuTu = Convert.ToInt32(row["ThuTu"]),
                    DapAnDung = row["DapAnDung"] != DBNull.Value ? row["DapAnDung"].ToString() : ""
                });
            }

            return View(danhSachCauHoi);
        }

        // ============================================
        // CREATE - Form thêm câu hỏi
        // ============================================
        public ActionResult Create(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View();
        }

        // ============================================
        // CREATE - POST: Thêm câu hỏi đơn
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string maDT, CauHoi cauHoi)
        {
            try
            {
                // Lấy thứ tự mới nhất
                string maxThuTuQuery = "SELECT MAX(ThuTu) FROM CauHoi WHERE MaDT = @MaDT";
                object maxThuTu = db.ExecuteScalar(maxThuTuQuery,
                    new SqlParameter[] { new SqlParameter("@MaDT", maDT) });
                int thuTuMoi = (maxThuTu == DBNull.Value || maxThuTu == null ? 0 : Convert.ToInt32(maxThuTu)) + 1;

                string query = @"
                    INSERT INTO CauHoi (MaDT, NoiDungCau, LoaiCau, DapAnDung, DiemCau, ThuTu, NgayTao)
                    VALUES (@MaDT, @NoiDungCau, @LoaiCau, @DapAnDung, @DiemCau, @ThuTu, @NgayTao)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaDT", maDT),
                    new SqlParameter("@NoiDungCau", cauHoi.NoiDungCau),
                    new SqlParameter("@LoaiCau", cauHoi.LoaiCau),
                    new SqlParameter("@DapAnDung", (object)cauHoi.DapAnDung ?? DBNull.Value),
                    new SqlParameter("@DiemCau", cauHoi.DiemCau),
                    new SqlParameter("@ThuTu", thuTuMoi),
                    new SqlParameter("@NgayTao", DateTime.Now)
                };

                int result = db.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    // Cập nhật số câu hỏi trong bảng DeThi
                    db.ExecuteStoredProcedureNonQuery("sp_CapNhatSoCauDeThi",
                        new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

                    TempData["SuccessMessage"] = "Thêm câu hỏi thành công!";
                    return RedirectToAction("Index", new { maDT = maDT });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View();
        }

        // ============================================
        // EDIT - Form chỉnh sửa câu hỏi
        // ============================================
        public ActionResult Edit(int id, string maDT)
        {
            string query = "SELECT * FROM CauHoi WHERE MaCauHoi = @MaCauHoi";
            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaCauHoi", id) });

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            CauHoi cauHoi = new CauHoi
            {
                MaCauHoi = Convert.ToInt32(row["MaCauHoi"]),
                MaDT = row["MaDT"].ToString(),
                NoiDungCau = row["NoiDungCau"].ToString(),
                LoaiCau = row["LoaiCau"].ToString(),
                DiemCau = Convert.ToSingle(row["DiemCau"]),
                ThuTu = Convert.ToInt32(row["ThuTu"]),
                DapAnDung = row["DapAnDung"] != DBNull.Value ? row["DapAnDung"].ToString() : ""
            };

            ViewBag.MaDT = cauHoi.MaDT;
            ViewBag.TenDeThi = LayTenDeThi(cauHoi.MaDT);
            return View(cauHoi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CauHoi cauHoi, string maDT)
        {
            try
            {
                string query = @"
                    UPDATE CauHoi
                    SET NoiDungCau = @NoiDungCau, LoaiCau = @LoaiCau, 
                        DapAnDung = @DapAnDung, DiemCau = @DiemCau
                    WHERE MaCauHoi = @MaCauHoi";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaCauHoi", cauHoi.MaCauHoi),
                    new SqlParameter("@NoiDungCau", cauHoi.NoiDungCau),
                    new SqlParameter("@LoaiCau", cauHoi.LoaiCau),
                    new SqlParameter("@DapAnDung", (object)cauHoi.DapAnDung ?? DBNull.Value),
                    new SqlParameter("@DiemCau", cauHoi.DiemCau)
                };

                int result = db.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công!";
                    return RedirectToAction("Index", new { maDT = maDT });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View(cauHoi);
        }

        // ============================================
        // DELETE - Xóa câu hỏi
        // ============================================
        public ActionResult Delete(int id, string maDT)
        {
            try
            {
                string query = "DELETE FROM CauHoi WHERE MaCauHoi = @MaCauHoi";
                db.ExecuteNonQuery(query,
                    new SqlParameter[] { new SqlParameter("@MaCauHoi", id) });

                // Cập nhật số câu hỏi
                db.ExecuteStoredProcedureNonQuery("sp_CapNhatSoCauDeThi",
                    new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

                TempData["SuccessMessage"] = "Xóa câu hỏi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index", new { maDT = maDT });
        }

        // ============================================
        // IMPORT EXCEL - Form import câu hỏi từ Excel
        // ============================================
        public ActionResult ImportExcel(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View();
        }

        // ============================================
        // POST: Import Excel
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(string maDT, HttpPostedFileBase fileExcel)
        {
            try
            {
                if (fileExcel == null || fileExcel.ContentLength == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn file Excel!";
                    return RedirectToAction("ImportExcel", new { maDT = maDT });
                }

                if (!fileExcel.FileName.EndsWith(".xlsx") && !fileExcel.FileName.EndsWith(".xls"))
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn file Excel (.xlsx hoặc .xls)!";
                    return RedirectToAction("ImportExcel", new { maDT = maDT });
                }

                

                using (ExcelPackage package = new ExcelPackage(fileExcel.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int soThemThanhCong = 0;

                    // Lấy thứ tự bắt đầu
                    string maxThuTuQuery = "SELECT MAX(ThuTu) FROM CauHoi WHERE MaDT = @MaDT";
                    object maxThuTu = db.ExecuteScalar(maxThuTuQuery,
                        new SqlParameter[] { new SqlParameter("@MaDT", maDT) });
                    int thuTu = (maxThuTu == DBNull.Value || maxThuTu == null ? 0 : Convert.ToInt32(maxThuTu));

                    // Duyệt từ hàng 2 (hàng 1 là header)
                    // Format Excel: 
                    // Cột 1: Nội dung câu hỏi
                    // Cột 2: Loại câu (TN: Trắc nghiệm, TL: Tự luận)
                    // Cột 3: Đáp án đúng (JSON hoặc text)
                    // Cột 4: Điểm
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string noiDung = worksheet.Cells[row, 1].Value?.ToString().Trim();
                        string loaiCau = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        string dapAn = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        string diemStr = worksheet.Cells[row, 4].Value?.ToString().Trim();

                        if (string.IsNullOrEmpty(noiDung) || string.IsNullOrEmpty(loaiCau))
                            continue;

                        float diem = 1;
                        if (!string.IsNullOrEmpty(diemStr) && float.TryParse(diemStr, out float d))
                            diem = d;

                        thuTu++;

                        string query = @"
                            INSERT INTO CauHoi (MaDT, NoiDungCau, LoaiCau, DapAnDung, DiemCau, ThuTu, NgayTao)
                            VALUES (@MaDT, @NoiDungCau, @LoaiCau, @DapAnDung, @DiemCau, @ThuTu, @NgayTao)";

                        SqlParameter[] parameters = new SqlParameter[]
                        {
                            new SqlParameter("@MaDT", maDT),
                            new SqlParameter("@NoiDungCau", noiDung),
                            new SqlParameter("@LoaiCau", loaiCau),
                            new SqlParameter("@DapAnDung", (object)dapAn ?? DBNull.Value),
                            new SqlParameter("@DiemCau", diem),
                            new SqlParameter("@ThuTu", thuTu),
                            new SqlParameter("@NgayTao", DateTime.Now)
                        };

                        int result = db.ExecuteNonQuery(query, parameters);
                        if (result > 0)
                            soThemThanhCong++;
                    }

                    if (soThemThanhCong > 0)
                    {
                        // Cập nhật số câu hỏi
                        db.ExecuteStoredProcedureNonQuery("sp_CapNhatSoCauDeThi",
                            new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

                        TempData["SuccessMessage"] = $"Import thành công {soThemThanhCong} câu hỏi!";
                        return RedirectToAction("Index", new { maDT = maDT });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không có câu hỏi nào được thêm. Kiểm tra định dạng file!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi import: " + ex.Message;
            }

            return RedirectToAction("ImportExcel", new { maDT = maDT });
        }

        // ============================================
        // HELPER - Lấy tên đề thi
        // ============================================
        private string LayTenDeThi(string maDT)
        {
            string query = "SELECT TenDT FROM DeThi WHERE MaDT = @MaDT";
            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

            return dt.Rows.Count > 0 ? dt.Rows[0]["TenDT"].ToString() : "";
        }
    }
}