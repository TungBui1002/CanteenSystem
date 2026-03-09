using CanteenSystem.Data;
using CanteenSystem.Models;
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
        public ActionResult Index()
        {
            var leaderOrders = db.LeaderOrders.Include(l => l.Leader).Include(l => l.Meal);
            return View(leaderOrders.ToList());
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

        // GET: LeaderOrders/Create
        public ActionResult Create()
        {
            ViewBag.EmployeeId = new SelectList(db.Leaders, "EmployeeId", "CostCenter");
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName");
            return View();
        }

        // POST: LeaderOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderId,EmployeeId,Date,MealId,Status,Price,CreatedAt,UpdatedAt,Creator,Modifier")] LeaderOrder leaderOrder)
        {
            if (ModelState.IsValid)
            {
                db.LeaderOrders.Add(leaderOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EmployeeId = new SelectList(db.Leaders, "EmployeeId", "CostCenter", leaderOrder.EmployeeId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", leaderOrder.MealId);
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
