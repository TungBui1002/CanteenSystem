using CanteenSystem.Data;
using CanteenSystem.Models;
using OfficeOpenXml;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class LeadersController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Leaders (với tìm kiếm)
        public ActionResult Index(string searchString)
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            var leaders = db.Leaders.Include(l => l.Department);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                leaders = leaders.Where(l =>
                    l.EmployeeId.ToLower().Contains(searchString) ||
                    l.FullName.ToLower().Contains(searchString) ||
                    l.Rank.ToLower().Contains(searchString) ||
                    l.Department.DepartmentCode.ToLower().Contains(searchString) ||
                    l.Department.DepartmentName.ToLower().Contains(searchString));
            }

            return View(leaders.ToList());
        }

        // GET: Leaders/Details/5
        public ActionResult Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Leader leader = db.Leaders.Find(id);
            if (leader == null) return HttpNotFound();
            return View(leader);
        }

        // GET: Leaders/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");
            return View();
        }

        // POST: Leaders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeId,CostCenter,DepartmentId,Rank,FullName")] Leader leader)
        {
            if (ModelState.IsValid)
            {
                leader.CreatedAt = DateTime.Now;
                leader.Creator = User.Identity.Name ?? "Admin";
                leader.UpdatedAt = null;
                leader.Modifier = null;

                db.Leaders.Add(leader);
                db.SaveChanges();
                TempData["Success"] = "Thêm cán bộ thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        // GET: Leaders/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Leader leader = db.Leaders.Find(id);
            if (leader == null) return HttpNotFound();

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeId,CostCenter,DepartmentId,Rank,FullName")] Leader leader)  // Bỏ Creator/Modifier/CreatedAt/UpdatedAt khỏi Bind
        {
            if (ModelState.IsValid)
            {
                // Lấy object cũ từ DB (để giữ Creator cũ)
                var existing = db.Leaders.Find(leader.EmployeeId);
                if (existing == null) return HttpNotFound();

                // Chỉ update các field người dùng chỉnh sửa
                existing.CostCenter = leader.CostCenter;
                existing.DepartmentId = leader.DepartmentId;
                existing.Rank = leader.Rank;
                existing.FullName = leader.FullName;

                // Cập nhật audit fields
                existing.UpdatedAt = DateTime.Now;
                existing.Modifier = User.Identity.Name ?? "Admin";

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật cán bộ thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", leader.DepartmentId);
            return View(leader);
        }

        // GET: Leaders/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Leader leader = db.Leaders.Find(id);
            if (leader == null) return HttpNotFound();
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
            TempData["Success"] = "Xóa cán bộ thành công!";
            return RedirectToAction("Index");
        }

        // GET: Leaders/Import
        public ActionResult Import()
        {
            return View();
        }

        // POST: Leaders/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction("Import");
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Chỉ hỗ trợ file .xlsx!";
                return RedirectToAction("Import");
            }

            try
            {
                using (var stream = excelFile.InputStream)
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets["Sheet1"];
                    int rowCount = worksheet.Dimension.Rows;

                    int importedCount = 0;

                    for (int row = 2; row <= rowCount; row++) // Bỏ header row 1
                    {
                        string employeeId = worksheet.Cells[row, 1].Text?.Trim();
                        string fullName = worksheet.Cells[row, 2].Text?.Trim();
                        string departmentCode = worksheet.Cells[row, 3].Text?.Trim();
                        string rank = worksheet.Cells[row, 4].Text?.Trim();
                        string costCenter = worksheet.Cells[row, 5].Text?.Trim();

                        if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(departmentCode))
                            continue;

                        var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == departmentCode);
                        if (department == null) continue; 

                        if (db.Leaders.Any(l => l.EmployeeId == employeeId))
                            continue; 

                        var leader = new Leader
                        {
                            EmployeeId = employeeId,
                            FullName = fullName,
                            DepartmentId = department.DepartmentId,
                            Rank = rank,
                            CostCenter = costCenter,
                            CreatedAt = DateTime.Now,
                            Creator = User.Identity.Name ?? "Admin"
                        };

                        db.Leaders.Add(leader);
                        importedCount++;
                    }

                    db.SaveChanges();
                    TempData["Success"] = $"Import thành công {importedCount} cán bộ!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi import: " + ex.Message;
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
