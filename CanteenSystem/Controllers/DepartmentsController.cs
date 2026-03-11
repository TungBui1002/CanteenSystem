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
    public class DepartmentsController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Departments
        public ActionResult Index(string searchString)
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            var departments = from d in db.Departments
                              select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d => d.DepartmentCode.Contains(searchString) ||
                                                     d.DepartmentName.Contains(searchString));
            }

            return View(departments.ToList());
        }

        // GET: Departments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                department.CreatedAt = DateTime.Now;
                department.Creator = User.Identity.Name ?? "Admin";
                department.UpdatedAt = null;
                department.Modifier = null;

                db.Departments.Add(department);
                db.SaveChanges();
                TempData["Success"] = "Thêm bộ phận thành công!";
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Departments.Find(department.DepartmentId);
                if (existing == null) return HttpNotFound();

                existing.DepartmentCode = department.DepartmentCode;
                existing.DepartmentName = department.DepartmentName;
                existing.CostCenter = department.CostCenter;
                existing.UpdatedAt = DateTime.Now;
                existing.Modifier = User.Identity.Name ?? "Admin";

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật bộ phận thành công!";
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments.Find(id);
            db.Departments.Remove(department);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Import form
        public ActionResult Import()
        {
            return View();
        }

        // POST: Import Excel
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
                    var worksheet = package.Workbook.Worksheets["Sheet1"]; // sheet đầu tiên
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Bỏ header row 1
                    {
                        var department = new Department
                        {
                            DepartmentCode = worksheet.Cells[row, 1].Text?.Trim(),
                            DepartmentName = worksheet.Cells[row, 2].Text?.Trim(),
                            CostCenter = worksheet.Cells[row, 3].Text?.Trim(),
                            CreatedAt = DateTime.Now,
                            Creator = User.Identity.Name ?? "Admin"
                        };

                        if (!string.IsNullOrEmpty(department.DepartmentCode) && !string.IsNullOrEmpty(department.DepartmentName))
                        {
                            // Kiểm tra trùng mã bộ phận
                            if (!db.Departments.Any(d => d.DepartmentCode == department.DepartmentCode))
                            {
                                db.Departments.Add(department);
                            }
                        }
                    }

                    db.SaveChanges();
                    TempData["Success"] = $"Import thành công {rowCount - 1} bộ phận!";
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
