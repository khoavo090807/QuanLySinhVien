using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using OfficeOpenXml;

namespace QuanLySinhVien.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CauHoiTracNghiemController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // INDEX - Danh sách câu hỏi trắc nghiệm
        // ============================================
        public ActionResult Index(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            // Kiểm tra đề thi tồn tại
            string checkDTQuery = "SELECT TenDT FROM DeThi WHERE MaDT = @MaDT";
            DataTable checkDT = db.ExecuteQuery(checkDTQuery,
                new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

            if (checkDT.Rows.Count == 0)
                return HttpNotFound();

            ViewBag.TenDeThi = checkDT.Rows[0]["TenDT"].ToString();
            ViewBag.MaDT = maDT;

            List<CauHoiTracNghiem> danhSach = new List<CauHoiTracNghiem>();

            string query = @"
                SELECT MaCau, MaDT, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, 
                       DapAnDung, Diem, ThuTu, NgayTao
                FROM CauHoiTracNghiem
                WHERE MaDT = @MaDT
                ORDER BY ThuTu ASC";

            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaDT", maDT) });

            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new CauHoiTracNghiem
                {
                    MaCau = Convert.ToInt32(row["MaCau"]),
                    MaDT = row["MaDT"].ToString(),
                    NoiDung = row["NoiDung"].ToString(),
                    DapAnA = row["DapAnA"].ToString(),
                    DapAnB = row["DapAnB"].ToString(),
                    DapAnC = row["DapAnC"].ToString(),
                    DapAnD = row["DapAnD"].ToString(),
                    DapAnDung = row["DapAnDung"].ToString(),
                    Diem = Convert.ToSingle(row["Diem"]),
                    ThuTu = Convert.ToInt32(row["ThuTu"])
                });
            }

            return View(danhSach);
        }

        // ============================================
        // CREATE - Thêm câu hỏi đơn
        // ============================================
        public ActionResult Create(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View(new CauHoiTracNghiem { Diem = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string maDT, CauHoiTracNghiem cauHoi)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(cauHoi.NoiDung))
                {
                    ModelState.AddModelError("", "Nội dung câu hỏi không được để trống!");
                    ViewBag.MaDT = maDT;
                    ViewBag.TenDeThi = LayTenDeThi(maDT);
                    return View(cauHoi);
                }

                if (string.IsNullOrEmpty(cauHoi.DapAnA) || string.IsNullOrEmpty(cauHoi.DapAnB) ||
                    string.IsNullOrEmpty(cauHoi.DapAnC) || string.IsNullOrEmpty(cauHoi.DapAnD))
                {
                    ModelState.AddModelError("", "Cần điền đầy đủ 4 đáp án!");
                    ViewBag.MaDT = maDT;
                    ViewBag.TenDeThi = LayTenDeThi(maDT);
                    return View(cauHoi);
                }

                if (string.IsNullOrEmpty(cauHoi.DapAnDung) ||
                    !new[] { "A", "B", "C", "D" }.Contains(cauHoi.DapAnDung.ToUpper()))
                {
                    ModelState.AddModelError("", "Đáp án đúng phải là A, B, C hoặc D!");
                    ViewBag.MaDT = maDT;
                    ViewBag.TenDeThi = LayTenDeThi(maDT);
                    return View(cauHoi);
                }

                // Lấy thứ tự mới
                string maxThuTuQuery = "SELECT MAX(ThuTu) FROM CauHoiTracNghiem WHERE MaDT = @MaDT";
                object maxThuTu = db.ExecuteScalar(maxThuTuQuery,
                    new SqlParameter[] { new SqlParameter("@MaDT", maDT) });
                int thuTuMoi = (maxThuTu == DBNull.Value || maxThuTu == null ? 0 : Convert.ToInt32(maxThuTu)) + 1;

                string query = @"
                    INSERT INTO CauHoiTracNghiem (MaDT, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, 
                                                   DapAnDung, Diem, ThuTu, NgayTao)
                    VALUES (@MaDT, @NoiDung, @DapAnA, @DapAnB, @DapAnC, @DapAnD, 
                            @DapAnDung, @Diem, @ThuTu, @NgayTao)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaDT", maDT),
                    new SqlParameter("@NoiDung", cauHoi.NoiDung),
                    new SqlParameter("@DapAnA", cauHoi.DapAnA),
                    new SqlParameter("@DapAnB", cauHoi.DapAnB),
                    new SqlParameter("@DapAnC", cauHoi.DapAnC),
                    new SqlParameter("@DapAnD", cauHoi.DapAnD),
                    new SqlParameter("@DapAnDung", cauHoi.DapAnDung.ToUpper()),
                    new SqlParameter("@Diem", cauHoi.Diem > 0 ? cauHoi.Diem : 1),
                    new SqlParameter("@ThuTu", thuTuMoi),
                    new SqlParameter("@NgayTao", DateTime.Now)
                };

                int result = db.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    UpdateSoCauDeThi(maDT);
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
            return View(cauHoi);
        }

        // ============================================
        // EDIT - Chỉnh sửa câu hỏi
        // ============================================
        public ActionResult Edit(int id, string maDT)
        {
            string query = "SELECT * FROM CauHoiTracNghiem WHERE MaCau = @MaCau";
            DataTable dt = db.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@MaCau", id) });

            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            CauHoiTracNghiem cauHoi = new CauHoiTracNghiem
            {
                MaCau = Convert.ToInt32(row["MaCau"]),
                MaDT = row["MaDT"].ToString(),
                NoiDung = row["NoiDung"].ToString(),
                DapAnA = row["DapAnA"].ToString(),
                DapAnB = row["DapAnB"].ToString(),
                DapAnC = row["DapAnC"].ToString(),
                DapAnD = row["DapAnD"].ToString(),
                DapAnDung = row["DapAnDung"].ToString(),
                Diem = Convert.ToSingle(row["Diem"]),
                ThuTu = Convert.ToInt32(row["ThuTu"])
            };

            ViewBag.MaDT = cauHoi.MaDT;
            ViewBag.TenDeThi = LayTenDeThi(cauHoi.MaDT);
            return View(cauHoi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CauHoiTracNghiem cauHoi, string maDT)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(cauHoi.NoiDung))
                {
                    ModelState.AddModelError("", "Nội dung câu hỏi không được để trống!");
                    ViewBag.MaDT = maDT;
                    ViewBag.TenDeThi = LayTenDeThi(maDT);
                    return View(cauHoi);
                }

                if (!new[] { "A", "B", "C", "D" }.Contains(cauHoi.DapAnDung.ToUpper()))
                {
                    ModelState.AddModelError("", "Đáp án đúng phải là A, B, C hoặc D!");
                    ViewBag.MaDT = maDT;
                    ViewBag.TenDeThi = LayTenDeThi(maDT);
                    return View(cauHoi);
                }

                string query = @"
                    UPDATE CauHoiTracNghiem
                    SET NoiDung = @NoiDung, DapAnA = @DapAnA, DapAnB = @DapAnB, 
                        DapAnC = @DapAnC, DapAnD = @DapAnD, DapAnDung = @DapAnDung, 
                        Diem = @Diem, NgayCapNhat = @NgayCapNhat
                    WHERE MaCau = @MaCau";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaCau", cauHoi.MaCau),
                    new SqlParameter("@NoiDung", cauHoi.NoiDung),
                    new SqlParameter("@DapAnA", cauHoi.DapAnA),
                    new SqlParameter("@DapAnB", cauHoi.DapAnB),
                    new SqlParameter("@DapAnC", cauHoi.DapAnC),
                    new SqlParameter("@DapAnD", cauHoi.DapAnD),
                    new SqlParameter("@DapAnDung", cauHoi.DapAnDung.ToUpper()),
                    new SqlParameter("@Diem", cauHoi.Diem > 0 ? cauHoi.Diem : 1),
                    new SqlParameter("@NgayCapNhat", DateTime.Now)
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
                string query = "DELETE FROM CauHoiTracNghiem WHERE MaCau = @MaCau";
                db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaCau", id) });

                UpdateSoCauDeThi(maDT);
                TempData["SuccessMessage"] = "Xóa câu hỏi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index", new { maDT = maDT });
        }

        // ============================================
        // IMPORT EXCEL - Import câu hỏi từ Excel
        // ============================================
        public ActionResult ImportExcel(string maDT)
        {
            if (string.IsNullOrEmpty(maDT))
                return RedirectToAction("Index", "DeThi");

            ViewBag.MaDT = maDT;
            ViewBag.TenDeThi = LayTenDeThi(maDT);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(string maDT, System.Web.HttpPostedFileBase fileExcel)
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

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; using (ExcelPackage package = new ExcelPackage(fileExcel.InputStream))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        TempData["ErrorMessage"] = "File Excel không có worksheet!";
                        return RedirectToAction("ImportExcel", new { maDT = maDT });
                    }

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null)
                    {
                        TempData["ErrorMessage"] = "Worksheet không có dữ liệu!";
                        return RedirectToAction("ImportExcel", new { maDT = maDT });
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    if (rowCount < 2)
                    {
                        TempData["ErrorMessage"] = "File Excel phải có ít nhất 2 hàng (header + dữ liệu)!";
                        return RedirectToAction("ImportExcel", new { maDT = maDT });
                    }

                    // Lấy thứ tự bắt đầu
                    string maxThuTuQuery = "SELECT MAX(ThuTu) FROM CauHoiTracNghiem WHERE MaDT = @MaDT";
                    object maxThuTu = db.ExecuteScalar(maxThuTuQuery,
                        new SqlParameter[] { new SqlParameter("@MaDT", maDT) });
                    int thuTu = (maxThuTu == DBNull.Value || maxThuTu == null ? 0 : Convert.ToInt32(maxThuTu));

                    int soThemThanhCong = 0;
                    var loi = new List<string>();

                    // Format Excel: NoiDung | DapAnA | DapAnB | DapAnC | DapAnD | DapAnDung | Diem
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            string noiDung = worksheet.Cells[row, 1].Value?.ToString().Trim();
                            string dapAnA = worksheet.Cells[row, 2].Value?.ToString().Trim();
                            string dapAnB = worksheet.Cells[row, 3].Value?.ToString().Trim();
                            string dapAnC = worksheet.Cells[row, 4].Value?.ToString().Trim();
                            string dapAnD = worksheet.Cells[row, 5].Value?.ToString().Trim();
                            string dapAnDung = worksheet.Cells[row, 6].Value?.ToString().Trim().ToUpper();
                            string diemStr = worksheet.Cells[row, 7].Value?.ToString().Trim();

                            // Kiểm tra bắt buộc
                            if (string.IsNullOrEmpty(noiDung))
                                continue;

                            if (string.IsNullOrEmpty(dapAnA) || string.IsNullOrEmpty(dapAnB) ||
                                string.IsNullOrEmpty(dapAnC) || string.IsNullOrEmpty(dapAnD))
                            {
                                loi.Add($"Hàng {row}: Thiếu đáp án");
                                continue;
                            }

                            if (string.IsNullOrEmpty(dapAnDung) ||
                                !new[] { "A", "B", "C", "D" }.Contains(dapAnDung))
                            {
                                loi.Add($"Hàng {row}: Đáp án đúng không hợp lệ (phải là A, B, C hoặc D)");
                                continue;
                            }

                            float diem = 1;
                            if (!string.IsNullOrEmpty(diemStr) && float.TryParse(diemStr, out float d))
                                diem = d > 0 ? d : 1;

                            thuTu++;

                            string query = @"
                                INSERT INTO CauHoiTracNghiem (MaDT, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, 
                                                               DapAnDung, Diem, ThuTu, NgayTao)
                                VALUES (@MaDT, @NoiDung, @DapAnA, @DapAnB, @DapAnC, @DapAnD, 
                                        @DapAnDung, @Diem, @ThuTu, @NgayTao)";

                            SqlParameter[] parameters = new SqlParameter[]
                            {
                                new SqlParameter("@MaDT", maDT),
                                new SqlParameter("@NoiDung", noiDung),
                                new SqlParameter("@DapAnA", dapAnA),
                                new SqlParameter("@DapAnB", dapAnB),
                                new SqlParameter("@DapAnC", dapAnC),
                                new SqlParameter("@DapAnD", dapAnD),
                                new SqlParameter("@DapAnDung", dapAnDung),
                                new SqlParameter("@Diem", diem),
                                new SqlParameter("@ThuTu", thuTu),
                                new SqlParameter("@NgayTao", DateTime.Now)
                            };

                            int result = db.ExecuteNonQuery(query, parameters);
                            if (result > 0)
                                soThemThanhCong++;
                        }
                        catch (Exception ex)
                        {
                            loi.Add($"Hàng {row}: {ex.Message}");
                        }
                    }

                    if (soThemThanhCong > 0)
                    {
                        UpdateSoCauDeThi(maDT);
                        TempData["SuccessMessage"] = $"Import thành công {soThemThanhCong} câu hỏi!";

                        if (loi.Count > 0)
                            TempData["WarningMessage"] = $"Có {loi.Count} hàng lỗi: " + string.Join("; ", loi.Take(5));

                        return RedirectToAction("Index", new { maDT = maDT });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không có câu hỏi nào được thêm. " +
                            (loi.Count > 0 ? string.Join("; ", loi.Take(3)) : "Kiểm tra định dạng file!");
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
        // HELPER - Cập nhật số câu hỏi
        // ============================================
        private void UpdateSoCauDeThi(string maDT)
        {
            try
            {
                string query = @"
                    UPDATE DeThi 
                    SET SoCau = (SELECT COUNT(*) FROM CauHoiTracNghiem WHERE MaDT = @MaDT)
                    WHERE MaDT = @MaDT";

                db.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@MaDT", maDT) });
            }
            catch { }
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