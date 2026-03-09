using CanteenSystem.Data;
using CanteenSystem.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class MealOrdersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: MealOrders
        public ActionResult Index()
        {
            var mealOrders = db.MealOrders.Include(m => m.Department).Include(m => m.Kitchen).Include(m => m.Meal);
            return View(mealOrders.ToList());
        }

        // GET: MealOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealOrder mealOrder = db.MealOrders.Find(id);
            if (mealOrder == null)
            {
                return HttpNotFound();
            }
            return View(mealOrder);
        }

        // GET: MealOrders/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");
            ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName");
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName");
            return View();
        }

        // POST: MealOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderId,DepartmentId,Date,Shift,Time,MealId,PersonnelType,Quantity,Price,KitchenId,CreatedAt,UpdatedAt,Creator,Modifier")] MealOrder mealOrder)
        {
            if (ModelState.IsValid)
            {
                db.MealOrders.Add(mealOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", mealOrder.MealId);
            return View(mealOrder);
        }

        // GET: MealOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealOrder mealOrder = db.MealOrders.Find(id);
            if (mealOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", mealOrder.MealId);
            return View(mealOrder);
        }

        // POST: MealOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderId,DepartmentId,Date,Shift,Time,MealId,PersonnelType,Quantity,Price,KitchenId,CreatedAt,UpdatedAt,Creator,Modifier")] MealOrder mealOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mealOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.KitchenId = new SelectList(db.Kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.MealId = new SelectList(db.Meals, "MealId", "MealName", mealOrder.MealId);
            return View(mealOrder);
        }

        // GET: MealOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealOrder mealOrder = db.MealOrders.Find(id);
            if (mealOrder == null)
            {
                return HttpNotFound();
            }
            return View(mealOrder);
        }

        // POST: MealOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MealOrder mealOrder = db.MealOrders.Find(id);
            db.MealOrders.Remove(mealOrder);
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
