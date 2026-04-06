using CanteenSystem.Data;
using CanteenSystem.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
            ViewBag.Shift = new SelectList(new[] { "Ca sáng", "Tăng ca", "Ca đêm" });
            ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "Nghiệp vụ", "NCPT1", "NCPT2", "NCPT3" });
            ViewBag.SelectedDate = DateTime.Today.Date;
            // Default Time cho từng ca (dùng JS để lọc)
            ViewBag.TimeOptions = new Dictionary<string, List<string>>
            {
                { "Ca sáng", new List<string> { "06:00", "10:00", "11:30", "12:00" } },
                { "Tăng ca", new List<string> { "16:30", "17:00", "20:00" } },
                { "Ca đêm", new List<string> { "01:30" } }
            };

            ViewBag.MealOptions = new Dictionary<string, Dictionary<string, List<string>>>
            {
                {
                    "Ca sáng", new Dictionary<string, List<string>>
                    {
                        { "06:00", new List<string> { "Mì" } },
                        { "10:00", new List<string> { "Cơm mặn", "Cơm chay" } },
                        { "11:30", new List<string> { "Cơm mặn", "Cơm chay" } },
                        { "12:00", new List<string> { "Cơm mặn", "Cơm chay" } }
                    }
                },
                {
                    "Tăng ca", new Dictionary<string, List<string>>
                    {
                        { "16:30", new List<string> { "Cơm mặn", "Cơm chay", "Phở" } },
                        { "17:00", new List<string> { "Cơm mặn", "Cơm chay", "Phở" } },
                        { "20:00", new List<string> { "Cơm mặn", "Cơm chay", "Phở" } }
                    }
                },
                {
                    "Ca đêm", new Dictionary<string, List<string>>
                    {
                        { "01:30", new List<string> { "Cơm mặn", "Cơm chay", "Phở", "Mì" } }
                    }
                }
            };

            var model = new MealOrder
            {
                Date = DateTime.Today.Date,
                Quantity = 1
            };

            return View(model);
        }

        // POST: MealOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string[] DepartmentId, string[] Shift, string[] Time, string[] MealId,
                                   string[] KitchenId, string[] PersonnelType, int[] Quantity)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var departments = GetAccessibleDepartments();
            var role = Session["Role"]?.ToString();

            if (DepartmentId == null || DepartmentId.Length == 0)
            {
                TempData["Error"] = "Vui lòng thêm ít nhất một suất ăn!";
                return RedirectToAction("Create");
            }

            for (int i = 0; i < DepartmentId.Length; i++)
            {
                if (string.IsNullOrEmpty(DepartmentId[i]) || string.IsNullOrEmpty(MealId[i]))
                    continue;

                var mealOrder = new MealOrder
                {
                    DepartmentId = int.Parse(DepartmentId[i]),
                    Date = DateTime.Today,
                    Shift = Shift[i],
                    Time = TimeSpan.Parse(Time[i]),
                    MealId = int.Parse(MealId[i]),
                    KitchenId = int.Parse(KitchenId[i]),
                    PersonnelType = PersonnelType[i],
                    Quantity = Quantity[i],
                    CreatedAt = DateTime.Now,
                    Creator = User.Identity.Name ?? "Admin"
                };

                // Tính giá
                var meal = db.Meals.Find(mealOrder.MealId);
                if (meal != null)
                    mealOrder.Price = meal.Price * mealOrder.Quantity;

                // Kiểm tra quyền
                if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                    !departments.Any(d => d.DepartmentId == mealOrder.DepartmentId))
                {
                    return RedirectToAction("Create");
                }

                db.MealOrders.Add(mealOrder);
            }

            db.SaveChanges();
            return RedirectToAction("History", new { date = DateTime.Today });
        }

        // GET: MealOrders/History
        public ActionResult History(DateTime? date, string search)
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

            // Phân quyền
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var deptIds = accessibleDepts.Select(d => d.DepartmentId);
                query = query.Where(m => deptIds.Contains(m.DepartmentId));
            }

            // TÌM KIẾM theo Mã bộ phận hoặc Tên bộ phận
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(m =>
                    m.Department.DepartmentCode.ToLower().Contains(search) ||
                    m.Department.DepartmentName.ToLower().Contains(search)
                );
            }

            var orders = query
                .OrderBy(m => m.Department == null ? string.Empty : m.Department.DepartmentCode)
                .ToList();

            // Lấy danh sách ngày có báo cơm (cho dropdown)
            var dates = db.MealOrders
                .Select(m => DbFunctions.TruncateTime(m.Date))
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

            // Thêm ngày hiện tại nếu chưa có
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
            ViewBag.SelectedDate = selectedDate;
            ViewBag.Search = search;

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

        // GET: MealOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                TempData["Error"] = "Chỉ Admin mới có quyền sửa báo cơm!";
                return RedirectToAction("History");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MealOrder mealOrder = db.MealOrders.Find(id);
            if (mealOrder == null)
            {
                return HttpNotFound();
            }

            var departments = GetAccessibleDepartments();
            var meals = db.Meals.Where(m => m.ApplicableFor == "Department").ToList();
            var kitchens = db.Kitchens.ToList();

            ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.MealId = new SelectList(meals, "MealId", "MealName", mealOrder.MealId);
            ViewBag.KitchenId = new SelectList(kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.Shift = new SelectList(new[] { "Ca sáng", "Tăng ca", "Ca đêm" }, mealOrder.Shift);
            ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "Nghiệp vụ", "NCPT1", "NCPT2", "NCPT3" }, mealOrder.PersonnelType);

            var timeOptions = new Dictionary<string, List<string>>
            {
                { "Ca sáng", new List<string> { "06:00", "10:00", "11:30", "12:00" } },
                { "Tăng ca", new List<string> { "16:30", "17:00", "20:00" } },
                { "Ca đêm", new List<string> { "01:30" } }
            };
            ViewBag.TimeOptions = timeOptions;

            // Lấy danh sách giờ theo ca hiện tại để render sẵn trên view
            string currentShift = mealOrder.Shift ?? "";
            var currentTimeList = timeOptions.ContainsKey(currentShift)
                ? timeOptions[currentShift]
                : new List<string> { "06:00", "10:00", "11:30", "12:00", "16:30", "17:00", "20:00", "01:30" };
            string currentTime = mealOrder.Time.ToString(@"hh\:mm");
            ViewBag.TimeSelect = new SelectList(currentTimeList, currentTime);
            ViewBag.CurrentTime = currentTime;

            // Lấy tên bộ phận để hiển thị readonly
            var dept = departments.FirstOrDefault(d => d.DepartmentId == mealOrder.DepartmentId);
            ViewBag.DepartmentDisplay = dept != null ? $"{dept.DepartmentCode} - {dept.DepartmentName}" : "";

            return View(mealOrder);
        }

        // POST: MealOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderId,DepartmentId,Date,Shift,Time,MealId,KitchenId,PersonnelType,Quantity,Price,CreatedAt,Creator,UpdatedAt,Modifier")] MealOrder mealOrder)
        {
            if (!User.Identity.IsAuthenticated || Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                mealOrder.UpdatedAt = DateTime.Now;
                mealOrder.Modifier = User.Identity.Name ?? "Admin";

                // Tính lại giá
                var meal = db.Meals.Find(mealOrder.MealId);
                if (meal != null)
                {
                    mealOrder.Price = meal.Price * mealOrder.Quantity;
                }

                db.Entry(mealOrder).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("History", new { date = mealOrder.Date });
            }

            // Load lại dropdown nếu lỗi
            var departments = GetAccessibleDepartments();
            var meals = db.Meals.Where(m => m.ApplicableFor == "Department").ToList();
            var kitchens = db.Kitchens.ToList();

            ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DepartmentCode", mealOrder.DepartmentId);
            ViewBag.MealId = new SelectList(meals, "MealId", "MealName", mealOrder.MealId);
            ViewBag.KitchenId = new SelectList(kitchens, "KitchenId", "KitchenName", mealOrder.KitchenId);
            ViewBag.Shift = new SelectList(new[] { "Ca sáng", "Tăng ca", "Ca đêm" }, mealOrder.Shift);
            ViewBag.PersonnelType = new SelectList(new[] { "Trực tiếp", " Gián tiếp", "Quản lý", "Nghiệp vụ", "NCPT1", "NCPT2", "NCPT3" }, mealOrder.PersonnelType);

            var timeOptions2 = new Dictionary<string, List<string>>
            {
                { "Ca sáng", new List<string> { "06:00", "10:00", "11:30", "12:00" } },
                { "Tăng ca", new List<string> { "16:30", "17:00", "20:00" } },
                { "Ca đêm", new List<string> { "01:30" } }
            };
            ViewBag.TimeOptions = timeOptions2;
            string currentShift2 = mealOrder.Shift ?? "";
            var currentTimeList2 = timeOptions2.ContainsKey(currentShift2)
                ? timeOptions2[currentShift2]
                : new List<string> { "06:00", "10:00", "11:30", "12:00", "16:30", "17:00", "20:00", "01:30" };
            string currentTime2 = mealOrder.Time.ToString(@"hh\:mm");
            ViewBag.TimeSelect = new SelectList(currentTimeList2, currentTime2);
            ViewBag.CurrentTime = currentTime2;

            var dept2 = departments.FirstOrDefault(d => d.DepartmentId == mealOrder.DepartmentId);
            ViewBag.DepartmentDisplay = dept2 != null ? $"{dept2.DepartmentCode} - {dept2.DepartmentName}" : "";

            return View(mealOrder);
        }

        // GET: MealOrders/Delete/5
        public ActionResult Delete(int? id, DateTime? date)
        {
            if (!User.Identity.IsAuthenticated || Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

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
        public ActionResult DeleteConfirmed(int id, DateTime? date)
        {
            if (!User.Identity.IsAuthenticated || Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            MealOrder mealOrder = db.MealOrders.Find(id);
            if (mealOrder != null)
            {
                db.MealOrders.Remove(mealOrder);
                db.SaveChanges();
            }

            return RedirectToAction("History", new { date = date ?? DateTime.Today.Date });
        }

        // POST: MealOrders/ExportDailyMeal
        [HttpPost]
        [Authorize]
        public ActionResult ExportDailyMeal(DateTime date)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            DateTime selectedDate = date.Date;

            var orders = db.MealOrders
                .Include(m => m.Department)
                .Include(m => m.Meal)
                .Include(m => m.Kitchen)
                .Where(m => m.Date == selectedDate)
                .OrderBy(m => m.Department.DepartmentCode)
                .ToList();

            if (!orders.Any())
            {
                TempData["Error"] = "Ngày này chưa có báo cơm nào!";
                return RedirectToAction("History", new { date = selectedDate });
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Báo cáo ngày bộ phận");

                // Header
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Ngày";
                worksheet.Cells[1, 3].Value = "Mã bộ phận";
                worksheet.Cells[1, 4].Value = "Bộ phận";
                worksheet.Cells[1, 5].Value = "Loại nhân sự";
                worksheet.Cells[1, 6].Value = "Ca làm";
                worksheet.Cells[1, 7].Value = "Giờ ăn";
                worksheet.Cells[1, 8].Value = "Món ăn";
                worksheet.Cells[1, 9].Value = "Nhà ăn";
                worksheet.Cells[1, 10].Value = "Số lượng";
                worksheet.Cells[1, 11].Value = "Tổng tiền (VNĐ)";
                worksheet.Cells[1, 12].Value = "Người tạo";

                int row = 2;
                int stt = 1;
                foreach (var item in orders)
                {
                    worksheet.Cells[row, 1].Value = stt++;
                    worksheet.Cells[row, 2].Value = item.Date.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 3].Value = item.Department.DepartmentCode;
                    worksheet.Cells[row, 4].Value = item.Department?.DepartmentName ?? "Chưa gán";
                    worksheet.Cells[row, 5].Value = item.PersonnelType;
                    worksheet.Cells[row, 6].Value = item.Shift;
                    worksheet.Cells[row, 7].Value = item.Time.ToString(@"hh\:mm") ?? "-";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "hh:mm";
                    worksheet.Cells[row, 8].Value = item.Meal?.MealName ?? "-";
                    worksheet.Cells[row, 9].Value = item.Kitchen?.KitchenName ?? "-";
                    worksheet.Cells[row, 10].Value = item.Quantity;
                    worksheet.Cells[row, 11].Value = item.Price;
                    worksheet.Cells[row, 12].Value = item.Creator;

                    row++;
                }

                // Footer tổng cộng
                worksheet.Cells[row, 1].Value = "Tổng cộng";
                worksheet.Cells[row, 10].Value = orders.Sum(o => o.Quantity);
                worksheet.Cells[row, 11].Value = orders.Sum(o => o.Price);

                worksheet.Cells[row, 10, row, 11].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"BaoCaoNgayBoPhan_{selectedDate:dd-MM-yyyy}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}