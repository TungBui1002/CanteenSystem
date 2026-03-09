using CanteenSystem.Data;
using CanteenSystem.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class LeadersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Leaders
        public ActionResult Index()
        {
            var leaders = db.Leaders.Include(l => l.Department);
            return View(leaders.ToList());
        }

        // GET: Leaders/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Leader leader = db.Leaders.Find(id);
            if (leader == null)
            {
                return HttpNotFound();
            }
            return View(leader);
        }

        // GET: Leaders/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");
            return View();
        }

        // POST: Leaders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeId,CostCenter,DepartmentId,Rank,FullName,CreatedAt,UpdatedAt,Creator,Modifier")] Leader leader)
        {
            if (ModelState.IsValid)
            {
                db.Leaders.Add(leader);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        // GET: Leaders/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Leader leader = db.Leaders.Find(id);
            if (leader == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        // POST: Leaders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeId,CostCenter,DepartmentId,Rank,FullName,CreatedAt,UpdatedAt,Creator,Modifier")] Leader leader)
        {
            if (ModelState.IsValid)
            {
                db.Entry(leader).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        // GET: Leaders/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Leader leader = db.Leaders.Find(id);
            if (leader == null)
            {
                return HttpNotFound();
            }
            return View(leader);
        }

        // POST: Leaders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Leader leader = db.Leaders.Find(id);
            db.Leaders.Remove(leader);
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
