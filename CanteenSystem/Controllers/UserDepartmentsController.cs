using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class UserDepartmentsController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: UserDepartments
        public ActionResult Index()
        {
            var userDepartments = db.UserDepartments.Include(u => u.Department).Include(u => u.User);
            return View(userDepartments.ToList());
        }

        // GET: UserDepartments/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username");
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");
            return View();
        }

        // POST: UserDepartments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,DepartmentId")] UserDepartment userDepartment)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng để tránh insert duplicate
                if (db.UserDepartments.Any(ud => ud.UserId == userDepartment.UserId && ud.DepartmentId == userDepartment.DepartmentId))
                {
                    ModelState.AddModelError("", "User này đã được gán cho bộ phận này rồi!");
                }
                else
                {
                    db.UserDepartments.Add(userDepartment);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username", userDepartment.UserId);
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", userDepartment.DepartmentId);
            return View(userDepartment);
        }

        // GET: UserDepartments/Delete (xóa theo composite key)
        public ActionResult Delete(int? userId, int? departmentId)
        {
            if (userId == null || departmentId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserDepartment userDepartment = db.UserDepartments.Find(userId, departmentId);
            if (userDepartment == null) return HttpNotFound();

            return View(userDepartment);
        }

        // POST: UserDepartments/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int userId, int departmentId)
        {
            UserDepartment userDepartment = db.UserDepartments.Find(userId, departmentId);
            if (userDepartment != null)
            {
                db.UserDepartments.Remove(userDepartment);
                db.SaveChanges();
            }
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
}