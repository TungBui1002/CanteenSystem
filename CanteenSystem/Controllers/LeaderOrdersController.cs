using CanteenSystem.Data;
using CanteenSystem.Models;
using CanteenSystem.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class LeaderOrdersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: LeaderOrders 
        public ActionResult Index(DateTime? date)
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            DateTime selectedDate = date ?? DateTime.Today.Date;
            bool isToday = selectedDate.Date == DateTime.Today.Date;

            var orders = db.LeaderOrders
                .Include(o => o.Leader)
                .Include(o => o.Meal)
                .Where(o => DbFunctions.TruncateTime(o.Date) == selectedDate.Date)
                .OrderBy(o => o.Leader.EmployeeId)
                .ToList();

            var allLeaders = db.Leaders.Include(l => l.Department).OrderBy(l => l.EmployeeId).ToList();

            var viewModel = allLeaders.Select(l =>
            {
                var order = orders.FirstOrDefault(o => o.EmployeeId == l.EmployeeId);
                return new LeaderOrderViewModel
                {
                    OrderId = order?.OrderId,
                    CostCenter = l.CostCenter,
                    EmployeeId = l.EmployeeId,
                    FullName = l.FullName,
                    DepartmentName = l.Department?.DepartmentName ?? "Chưa gán",
                    MealName = order?.Meal?.MealName ?? ".",
                    Status = order?.Status ?? "Chưa đặt",
                    Price = order?.Price ?? 30000M,
                    Date = selectedDate,
                    Creator = order?.Creator
                };
            }).ToList();

            ViewBag.SelectedDate = selectedDate;
            var dates = db.LeaderOrders
                .Select(o => DbFunctions.TruncateTime(o.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList()
                .Where(d => d.HasValue)
                .Select(d => new SelectListItem
                {
                    Value = d.Value.ToString("yyyy-MM-dd"),
                    Text = d.Value.ToString("dd/MM/yyyy")
                })
                .ToList();

            // Thêm ngày hiện tại nếu chưa có (để luôn có ngày mặc định)
            if (!dates.Any(d => d.Value == selectedDate.ToString("yyyy-MM-dd")))
            {
                dates.Insert(0, new SelectListItem
                {
                    Value = selectedDate.ToString("yyyy-MM-dd"),
                    Text = selectedDate.ToString("dd/MM/yyyy"),
                    Selected = true
                });
            }

            ViewBag.Dates = dates;

            // Thêm ngày hiện tại nếu chưa có trong list
            var datesList = ViewBag.Dates as List<SelectListItem>;
            if (datesList != null && !datesList.Any(d => d.Value == selectedDate.ToString("yyyy-MM-dd")))
            {
                datesList.Insert(0, new SelectListItem
                {
                    Value = selectedDate.ToString("yyyy-MM-dd"),
                    Text = selectedDate.ToString("dd/MM/yyyy"),
                    Selected = true
                });
            }

            // Nếu là ngày hôm nay và chưa có record nào → hiện thông báo thân thiện
            if (isToday && !orders.Any())
            {
                ViewBag.NoOrderToday = true;
            }

            return View(viewModel);
        }

        // GET: LeaderOrders/Create (báo cơm ngày hôm nay)
        public ActionResult Create()
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            DateTime today = DateTime.Today.Date;

            // Khóa nếu ngày hôm nay đã có record
            if (db.LeaderOrders.Any(o => DbFunctions.TruncateTime(o.Date) == today))
            {
                return RedirectToAction("Index", new { date = today });
            }

            var leaders = db.Leaders
             .Include(l => l.Department)
             .OrderBy(l => l.EmployeeId)
             .ToList();

            var meals = db.Meals.Where(m => m.ApplicableFor == "Leader").ToList();

            ViewBag.SelectedDate = today;
            ViewBag.Meals = meals;

            return View(leaders);
        }

        // POST: LeaderOrders/Create (lưu batch toàn bộ cán bộ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DateTime selectedDate, int[] mealIds, string[] statuses)
        {
            if (db.LeaderOrders.Any(o => DbFunctions.TruncateTime(o.Date) == selectedDate.Date))
            {
                return RedirectToAction("Index", new { date = selectedDate });
            }

            var leaders = db.Leaders.ToList();
            int savedCount = 0;

            if (mealIds.Length != leaders.Count || statuses.Length != leaders.Count)
            {
                return RedirectToAction("Create");
            }

            for (int i = 0; i < leaders.Count; i++)
            {
                int mealId = mealIds[i];
                string status = statuses[i];

                var meal = db.Meals.Find(mealId);
                if (meal == null) continue;

                var order = new LeaderOrder
                {
                    EmployeeId = leaders[i].EmployeeId,
                    Date = selectedDate,
                    MealId = mealId,
                    Status = status,
                    Price = meal.Price, // Lấy giá thực từ món ăn
                    CreatedAt = DateTime.Now,
                    Creator = User.Identity.Name ?? "Admin"
                };

                db.LeaderOrders.Add(order);
                savedCount++;
            }

            db.SaveChanges();
            return RedirectToAction("Index", new { date = selectedDate });
        }

        // GET: LeaderOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaderOrder leaderOrder = db.LeaderOrders.Find(id);
            if (leaderOrder == null)
            {
                return HttpNotFound();
            }
            return View(leaderOrder);
        }

        // GET: LeaderOrders/Edit/5
        public ActionResult Edit(int? id, string employeeId, DateTime date)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LeaderOrder order = null;
            if (id.HasValue)
            {
                order = db.LeaderOrders.Find(id.Value);
            }

            // Nếu không tìm thấy record → tạo mới cho cán bộ này
            if (order == null)
            {
                var leader = db.Leaders.FirstOrDefault(l => l.EmployeeId == employeeId);
                if (leader == null) return HttpNotFound();

                order = new LeaderOrder
                {
                    EmployeeId = employeeId,
                    Date = date,
                    Status = "Chưa đặt",
                    Price = 30000
                };
            }

            var meals = db.Meals.Where(m => m.ApplicableFor == "Leader").ToList();
            ViewBag.Meals = new SelectList(meals, "MealId", "MealName", order.MealId);
            ViewBag.Date = date;
            ViewBag.EmployeeName = db.Leaders.FirstOrDefault(l => l.EmployeeId == employeeId)?.FullName ?? "Unknown";

            return View(order);
        }

        // POST: LeaderOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderId,EmployeeId,Date,MealId,Status,Price,CreatedAt,Creator,UpdatedAt,Modifier")] LeaderOrder order)
        {
            if (ModelState.IsValid)
            {
                if (order.OrderId > 0)
                {
                    // Update
                    db.Entry(order).State = EntityState.Modified;
                    order.UpdatedAt = DateTime.Now;
                    order.Modifier = User.Identity.Name ?? "Admin";
                }
                else
                {
                    // Create mới
                    order.CreatedAt = DateTime.Now;
                    order.Creator = User.Identity.Name ?? "Admin";
                    db.LeaderOrders.Add(order);
                }

                db.SaveChanges();
                return RedirectToAction("Index", new { date = order.Date });
            }

            var meals = db.Meals.Where(m => m.ApplicableFor == "Leader").ToList();
            ViewBag.Meals = new SelectList(meals, "MealId", "MealName", order.MealId);
            ViewBag.Date = order.Date;
            ViewBag.EmployeeName = db.Leaders.FirstOrDefault(l => l.EmployeeId == order.EmployeeId)?.FullName ?? "Unknown";

            return View(order);
        }

        // GET: LeaderOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaderOrder leaderOrder = db.LeaderOrders.Find(id);
            if (leaderOrder == null)
            {
                return HttpNotFound();
            }
            return View(leaderOrder);
        }

        // POST: LeaderOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LeaderOrder leaderOrder = db.LeaderOrders.Find(id);
            db.LeaderOrders.Remove(leaderOrder);
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
