using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class MealOrdersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: MealOrders
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.PathAndQuery });
            }
            var mealOrders = db.MealOrders.Include(m => m.Department).Include(m => m.Kitchen).Include(m => m.Meal);
            return View(mealOrders.ToList());
        }

        // GET: MealOrders/Create
        public ActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var departments = GetAccessibleDepartments();
            var meals = db.Meals.Where(m => m.ApplicableFor == "Department").ToList();
            var kitchens = db.Kitchens.ToList();

            ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DepartmentCode");
            ViewBag.MealId = new SelectList(meals, "MealId", "MealName");
            ViewBag.KitchenId = new SelectList(kitchens, "KitchenId", "KitchenName");
            ViewBag.Shift = new SelectList(new[] {"Day", "Overtime", "Night" });
            ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "NCPT1", "NCPT2", "NCPT3" });
            ViewBag.Time = new SelectList(new[] { "06:00", "10:00", "11:30", "12:00", "16:30", "17:00", "20:00", "01:30" });
            ViewBag.SelectedDate = DateTime.Today.Date;

            return View(new MealOrder
            {
                Date = DateTime.Today
            });
        }

        // POST: MealOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DepartmentId,Date,Shift,Time,MealId,KitchenId,PersonnelType,Quantity,Price,CreatedAt,Creator")] MealOrder mealOrder)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var departments = GetAccessibleDepartments();

            // Kiểm tra quyền bộ phận
            var role = Session["Role"]?.ToString();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                && !departments.Any(d => d.DepartmentId == mealOrder.DepartmentId))
            {
                ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
                ViewBag.MealId = new SelectList(db.Meals.Where(m => m.ApplicableFor == "Department"), "MealId", "MealName", mealOrder.MealId);
                ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
                ViewBag.Shift = new SelectList(new[] { "Day", "Overtime", "Night" });
                ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "NCPT1", "NCPT2", "NCPT3" }, mealOrder.PersonnelType);
                ViewBag.Time = new SelectList(new[] { "06:00", "10:00", "11:30", "12:00", "16:30", "17:00", "20:00", "01:30" }, mealOrder.Time);
                ViewBag.SelectedDate = mealOrder.Date;
                return View(mealOrder);
            }

            if (ModelState.IsValid)
            {
                mealOrder.CreatedAt = DateTime.Now;
                mealOrder.Creator = User.Identity.Name ?? "Admin";

                var meal = db.Meals.Find(mealOrder.MealId);
                if (meal != null)
                {
                    mealOrder.Price = meal.Price * mealOrder.Quantity;
                }

                db.MealOrders.Add(mealOrder);
                db.SaveChanges();
                return RedirectToAction("History", new { date = mealOrder.Date });
            }

            ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.MealId = new SelectList(db.Meals.Where(m => m.ApplicableFor == "Department"), "MealId", "MealName", mealOrder.MealId);
            ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.Shift = new SelectList(new[] { "Day", "Overtime", "Night" }, mealOrder.Shift);
            ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "NCPT1", "NCPT2", "NCPT3" }, mealOrder.PersonnelType);
            ViewBag.Time = new SelectList(new[] { "06:00", "10:00", "11:30", "12:00", "16:30", "17:00","20:00", "01:30" }, mealOrder.Time);
            ViewBag.SelectedDate = mealOrder.Date;
            return View(mealOrder);
        }

        // GET: MealOrders/History (xem lịch sử theo ngày)
        public ActionResult History(DateTime? date)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime selectedDate = date ?? DateTime.Today.Date;

            string role = Session["Role"]?.ToString();
            var accessibleDepts = GetAccessibleDepartments();

            var query = db.MealOrders
                .Include(m => m.Department)
                .Include(m => m.Meal)
                .Include(m => m.Kitchen)
                .Where(m => DbFunctions.TruncateTime(m.Date) == selectedDate.Date);

            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var deptIds = accessibleDepts.Select(d => d.DepartmentId);
                query = query.Where(m => deptIds.Contains(m.DepartmentId));
            }

            var orders = query.OrderBy(m => m.Department.DepartmentCode).ToList();

            ViewBag.SelectedDate = selectedDate;
            return View(orders);
        }

        private List<Department> GetAccessibleDepartments()
        {
            string username = User.Identity.Name;
            if (string.IsNullOrEmpty(username)) return new List<Department>();

            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return new List<Department>();

            if (user.Role == "Admin")
            {
                return db.Departments.ToList();
            }

            return db.UserDepartments
                .Where(ud => ud.UserId == user.UserId)
                .Select(ud => ud.Department)
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
