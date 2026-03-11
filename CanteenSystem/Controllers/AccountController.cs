using CanteenSystem.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace CanteenSystem.Controllers
{
    public class AccountController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username.Trim());
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View();
            }

            // Đăng nhập thành công
            FormsAuthentication.SetAuthCookie(username, false);  // false = không remember me

            // Lưu Session (dùng để hiển thị tên, role)
            Session["Role"] = user.Role;
            Session["Fullname"] = user.Fullname;
            Session["UserId"] = user.UserId;

            // Debug: kiểm tra cookie đã set chưa
            System.Diagnostics.Debug.WriteLine("Login success for: " + username);

            // Redirect về returnUrl nếu hợp lệ, nếu không thì MealOrders
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "MealOrders");
        }

        // GET: Account/Logout
        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}