using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class MealsController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Meals
        public ActionResult Index()
        {
            return View(db.Meals.ToList());
        }

        // GET: Meals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Meal meal = db.Meals.Find(id);
            if (meal == null) return HttpNotFound();
            return View(meal);
        }

        // GET: Meals/Create
        public ActionResult Create()
        {
            ViewBag.ApplicableForList = new SelectList(new[] { "Department", "Leader" });
            return View();
        }

        // POST: Meals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MealName,Price,ApplicableFor")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                meal.CreatedAt = DateTime.Now;
                meal.Creator = User.Identity.Name ?? "Admin";
                meal.UpdatedAt = null;
                meal.Modifier = null;

                db.Meals.Add(meal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicableForList = new SelectList(new[] { "Department", "Leader" });
            return View(meal);
        }

        // GET: Meals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Meal meal = db.Meals.Find(id);
            if (meal == null) return HttpNotFound();

            ViewBag.ApplicableForList = new SelectList(new[] { "Department", "Leader" }, meal.ApplicableFor);
            return View(meal);
        }

        // POST: Meals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MealId,MealName,Price,ApplicableFor")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Meals.Find(meal.MealId);
                if (existing == null) return HttpNotFound();

                existing.MealName = meal.MealName;
                existing.Price = meal.Price;
                existing.ApplicableFor = meal.ApplicableFor;
                existing.UpdatedAt = DateTime.Now;
                existing.Modifier = User.Identity.Name ?? "Admin";

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicableForList = new SelectList(new[] { "Department", "Leader" }, meal.ApplicableFor);
            return View(meal);
        }

        // GET: Meals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Meal meal = db.Meals.Find(id);
            if (meal == null) return HttpNotFound();
            return View(meal);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Meal meal = db.Meals.Find(id);
            db.Meals.Remove(meal);
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
