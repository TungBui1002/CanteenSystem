using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
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

            DateTime selectedDate = date ?? DateTime.Today;

            var orders = db.LeaderOrders
                .Include(o => o.Leader)
                .Include(o => o.Meal)
                .Where(o => o.Date == selectedDate)
                .ToList();

            ViewBag.SelectedDate = selectedDate;
            return View(orders);
        }

        // GET: LeaderOrders/Create (batch edit theo ngày)
        public ActionResult Create(DateTime? date)
        {
            DateTime selectedDate = date ?? DateTime.Today;

            // Lấy tất cả cán bộ
            var leaders = db.Leaders.Include(l => l.Department).ToList();

            // Lấy danh sách món áp dụng cho cán bộ
            var meals = db.Meals.Where(m => m.ApplicableFor == "Leader").ToList();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.Meals = new SelectList(meals, "MealId", "MealName");

            return View(leaders);
        }

        // POST: LeaderOrders/Create (lưu batch)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DateTime selectedDate, int[] mealIds, string[] statuses)
        {
            var leaders = db.Leaders.ToList();

            for (int i = 0; i < leaders.Count; i++)
            {
                int mealId = mealIds[i];
                string status = statuses[i];

                // Nếu chọn "Chưa đặt" thì bỏ qua
                if (status == "Chưa đặt") continue;

                var order = new LeaderOrder
                {
                    EmployeeId = leaders[i].EmployeeId,
                    Date = selectedDate,
                    MealId = mealId,
                    Status = status,
                    Price = 30000,          
                    CreatedAt = DateTime.Now,
                    Creator = User.Identity.Name ?? "Admin"
                };

                db.LeaderOrders.Add(order);
            }

            db.SaveChanges();
            TempData["Success"] = $"Đã lưu báo cơm cán bộ ngày {selectedDate:dd/MM/yyyy}";
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
        public ActionResult Edit(int? id)
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
            ViewBag.EmployeeId = new SelectList(db.Leaders, "EmployeeId", "CostCenter", leaderOrder.EmployeeId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", leaderOrder.MealId);
            return View(leaderOrder);
        }

        // POST: LeaderOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderId,EmployeeId,Date,MealId,Status,Price,CreatedAt,UpdatedAt,Creator,Modifier")] LeaderOrder leaderOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(leaderOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(db.Leaders, "EmployeeId", "CostCenter", leaderOrder.EmployeeId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", leaderOrder.MealId);
            return View(leaderOrder);
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
