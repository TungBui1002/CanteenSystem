using CanteenSystem.Data;
using CanteenSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class KitchensController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Kitchens
        public ActionResult Index()
        {
            return View(db.Kitchens.ToList());
        }

        // GET: Kitchens/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Kitchen kitchen = db.Kitchens.Find(id);
            if (kitchen == null)
            {
                return HttpNotFound();
            }
            return View(kitchen);
        }

        // GET: Kitchens/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Kitchens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "KitchenName")] Kitchen kitchen)
        {
            if (ModelState.IsValid)
            {
                kitchen.CreatedAt = DateTime.Now;
                kitchen.Creator = User.Identity.Name ?? "Admin";
                kitchen.UpdatedAt = null;
                kitchen.Modifier = null;

                db.Kitchens.Add(kitchen);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kitchen);
        }

        // GET: Kitchens/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Kitchen kitchen = db.Kitchens.Find(id);
            if (kitchen == null)
            {
                return HttpNotFound();
            }
            return View(kitchen);
        }

        // POST: Kitchens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "KitchenId,KitchenName")] Kitchen kitchen)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Kitchens.Find(kitchen.KitchenId);
                if (existing == null) return HttpNotFound();

                existing.KitchenName = kitchen.KitchenName;
                existing.UpdatedAt = DateTime.Now;
                existing.Modifier = User.Identity.Name ?? "Admin";

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kitchen);
        }

        // GET: Kitchens/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Kitchen kitchen = db.Kitchens.Find(id);
            if (kitchen == null)
            {
                return HttpNotFound();
            }
            return View(kitchen);
        }

        // POST: Kitchens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Kitchen kitchen = db.Kitchens.Find(id);
            db.Kitchens.Remove(kitchen);
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
