using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web;
namespace QuanLySinhVien.Controllers
{
    public class AccountController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // GET: Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            // Nếu user đã đăng nhập, chuyển đến trang chủ
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Mã hóa mật khẩu nhập vào
                    string hashedPassword = HashPassword(model.MatKhau);

                    // Truy vấn tài khoản
                    string query = @"SELECT MaTaiKhoan, TenDangNhap, Email, LoaiTaiKhoan, TrangThai 
                                   FROM Account 
                                   WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau AND TrangThai = 1";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TenDangNhap", model.TenDangNhap),
                        new SqlParameter("@MatKhau", hashedPassword)
                    };

                    DataTable dt = db.ExecuteQuery(query, parameters);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        string maTaiKhoan = row["MaTaiKhoan"].ToString();
                        string tenDangNhap = row["TenDangNhap"].ToString();
                        string loaiTaiKhoan = row["LoaiTaiKhoan"].ToString();

                        // Tạo authentication ticket
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            version: 1,
                            name: tenDangNhap,
                            issueDate: DateTime.Now,
                            expiration: DateTime.Now.AddHours(24),
                            isPersistent: model.GhiNho,
                            userData: loaiTaiKhoan
                        );

                        string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                        if (model.GhiNho)
                        {
                            authCookie.Expires = DateTime.Now.AddDays(30);
                        }

                        Response.Cookies.Add(authCookie);

                        // Ghi log đăng nhập
                        LogLoginActivity(maTaiKhoan, true);

                        TempData["SuccessMessage"] = "Đăng nhập thành công!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa!";
                        LogLoginActivity(model.TenDangNhap, false);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi đăng nhập: " + ex.Message;
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        [HttpGet]
        [Authorize]
        public ActionResult Logout()
        {
            string tenDangNhap = User.Identity.Name;

            // Ghi log đăng xuất
            LogLogoutActivity(tenDangNhap);

            // Xóa authentication cookie
            FormsAuthentication.SignOut();

            // Xóa session
            Session.Clear();
            Session.Abandon();

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // GET: Account/Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Account model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM Account WHERE TenDangNhap = @TenDangNhap";
                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@TenDangNhap", model.TenDangNhap) }));

                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
                        return View(model);
                    }

                    // Kiểm tra email đã tồn tại chưa
                    string checkEmailQuery = "SELECT COUNT(*) FROM Account WHERE Email = @Email";
                    int emailCount = Convert.ToInt32(db.ExecuteScalar(checkEmailQuery,
                        new SqlParameter[] { new SqlParameter("@Email", model.Email) }));

                    if (emailCount > 0)
                    {
                        ModelState.AddModelError("", "Email đã được đăng ký!");
                        return View(model);
                    }

                    // Mã hóa mật khẩu
                    string hashedPassword = HashPassword(model.MatKhau);

                    // Tạo mã tài khoản
                    string maTaiKhoan = "ACC" + DateTime.Now.Ticks;

                    // Insert tài khoản mới
                    string query = @"INSERT INTO Account (MaTaiKhoan, TenDangNhap, MatKhau, Email, LoaiTaiKhoan, TrangThai, NgayTao)
                                   VALUES (@MaTaiKhoan, @TenDangNhap, @MatKhau, @Email, @LoaiTaiKhoan, @TrangThai, @NgayTao)";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MaTaiKhoan", maTaiKhoan),
                        new SqlParameter("@TenDangNhap", model.TenDangNhap),
                        new SqlParameter("@MatKhau", hashedPassword),
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@LoaiTaiKhoan", "Student"),
                        new SqlParameter("@TrangThai", 1),
                        new SqlParameter("@NgayTao", DateTime.Now)
                    };

                    db.ExecuteNonQuery(query, parameters);

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi đăng ký: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string tenDangNhap = User.Identity.Name;
                    string hashedCurrentPassword = HashPassword(model.MatKhauHienTai);

                    // Kiểm tra mật khẩu hiện tại
                    string checkQuery = "SELECT COUNT(*) FROM Account WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau";
                    SqlParameter[] checkParams = new SqlParameter[]
                    {
                        new SqlParameter("@TenDangNhap", tenDangNhap),
                        new SqlParameter("@MatKhau", hashedCurrentPassword)
                    };

                    int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                    if (count == 0)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu hiện tại không chính xác!";
                        return View(model);
                    }

                    // Cập nhật mật khẩu mới
                    string hashedNewPassword = HashPassword(model.MatKhauMoi);
                    string updateQuery = @"UPDATE Account 
                                         SET MatKhau = @MatKhau, NgayCapNhat = @NgayCapNhat 
                                         WHERE TenDangNhap = @TenDangNhap";

                    SqlParameter[] updateParams = new SqlParameter[]
                    {
                        new SqlParameter("@MatKhau", hashedNewPassword),
                        new SqlParameter("@NgayCapNhat", DateTime.Now),
                        new SqlParameter("@TenDangNhap", tenDangNhap)
                    };

                    int result = db.ExecuteNonQuery(updateQuery, updateParams);

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                }
            }

            return View(model);
        }

        // Helper method: Mã hóa mật khẩu
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashedBytes)
                {
                    builder.Append(b.ToString("x2")); // Chuyển thành hexadecimal
                }
                return builder.ToString();
            }
        }

        // Ghi log đăng nhập
        private void LogLoginActivity(string tenDangNhap, bool thanhCong)
        {
            try
            {
                string query = @"INSERT INTO LoginLog (TenDangNhap, NgayGio, ThanhCong, DiaChiIP)
                               VALUES (@TenDangNhap, @NgayGio, @ThanhCong, @DiaChiIP)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@TenDangNhap", tenDangNhap),
                    new SqlParameter("@NgayGio", DateTime.Now),
                    new SqlParameter("@ThanhCong", thanhCong),
                    new SqlParameter("@DiaChiIP", Request.UserHostAddress)
                };

                db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
            }
        }

        // Ghi log đăng xuất
        private void LogLogoutActivity(string tenDangNhap)
        {
            try
            {
                string query = @"INSERT INTO LogoutLog (TenDangNhap, NgayGio, DiaChiIP)
                               VALUES (@TenDangNhap, @NgayGio, @DiaChiIP)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@TenDangNhap", tenDangNhap),
                    new SqlParameter("@NgayGio", DateTime.Now),
                    new SqlParameter("@DiaChiIP", Request.UserHostAddress)
                };

                db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
            }
        }
    }
}