using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class UsersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Users
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,Fullname,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                // Hash mật khẩu mới (sẽ dùng BCrypt sau)
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedAt = DateTime.Now;
                user.Creator = User.Identity.Name ?? "Admin";
                user.UpdatedAt = null;
                user.Modifier = null;

                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            // Không hiển thị mật khẩu cũ trong form
            user.Password = null;
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Username,Fullname,Role,CreatedAt,UpdatedAt,Creator,Modifier")] User user)
        {
            if (ModelState.IsValid)
            {
                // Lấy user cũ từ DB để giữ nguyên password và các giá trị không gửi từ form
                var existing = db.Users.Find(user.UserId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                // Chỉ update các field người dùng được phép sửa
                existing.Username = user.Username;
                existing.Fullname = user.Fullname;
                existing.Role = user.Role;

                // Cập nhật audit fields
                existing.UpdatedAt = DateTime.Now;
                existing.Modifier = User.Identity.Name ?? "Admin";

                // Giữ nguyên password cũ (không update)
                existing.Password = existing.Password;

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Index");
            }

            // Nếu lỗi validation, load lại dropdown nếu có
            return View(user);
        }

        // GET: Users/ResetPassword/5 
        public ActionResult ResetPassword(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            var model = new ResetPasswordViewModel { UserId = user.UserId, Username = user.Username };
            return View(model);
        }

        // POST: Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Find(model.UserId);
                if (user == null) return HttpNotFound();

                // Kiểm tra mật khẩu mới khớp nhau
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp!");
                    return View(model);
                }

                // Hash mật khẩu mới
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.Now;
                user.Modifier = User.Identity.Name ?? "Admin";

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Đặt lại mật khẩu thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModel cho reset mật khẩu
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}
