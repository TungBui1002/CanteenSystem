using CanteenSystem.Data;
using CanteenSystem.Models;
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

        // GET: UserDepartments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDepartment userDepartment = db.UserDepartments.Find(id);
            if (userDepartment == null)
            {
                return HttpNotFound();
            }
            return View(userDepartment);
        }

        // GET: UserDepartments/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username");
            return View();
        }

        // POST: UserDepartments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,DepartmentId")] UserDepartment userDepartment)
        {
            if (ModelState.IsValid)
            {
                db.UserDepartments.Add(userDepartment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", userDepartment.DepartmentId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username", userDepartment.UserId);
            return View(userDepartment);
        }

        // GET: UserDepartments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDepartment userDepartment = db.UserDepartments.Find(id);
            if (userDepartment == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", userDepartment.DepartmentId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username", userDepartment.UserId);
            return View(userDepartment);
        }

        // POST: UserDepartments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,DepartmentId")] UserDepartment userDepartment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userDepartment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", userDepartment.DepartmentId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Username", userDepartment.UserId);
            return View(userDepartment);
        }

        // GET: UserDepartments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDepartment userDepartment = db.UserDepartments.Find(id);
            if (userDepartment == null)
            {
                return HttpNotFound();
            }
            return View(userDepartment);
        }

        // POST: UserDepartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserDepartment userDepartment = db.UserDepartments.Find(id);
            db.UserDepartments.Remove(userDepartment);
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
}
