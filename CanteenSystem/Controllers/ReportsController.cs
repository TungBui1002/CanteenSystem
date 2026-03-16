using CanteenSystem.Data;
using CanteenSystem.Models.ViewModel;
using OfficeOpenXml;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class ReportsController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();
        
        // GET: Reports
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // GET: Reports/LeaderMonthly
        public ActionResult LeaderMonthly(DateTime? fromDate, DateTime? toDate)
        {
            DateTime start = fromDate ?? DateTime.Today.AddMonths(-1).Date;
            DateTime end = toDate ?? DateTime.Today.Date;

            // Lấy tất cả cán bộ
            var leaders = db.Leaders.Include(l => l.Department).ToList();

            // Tạo báo cáo tổng hợp
            var report = leaders.Select(l =>
            {
                var orders = db.LeaderOrders
                    .Include(o => o.Meal)
                    .Where(o => o.EmployeeId == l.EmployeeId && o.Date >= start && o.Date <= end)
                    .ToList();

                int manCount = orders.Count(o => o.Meal.MealName.Contains("mặn") && o.Status == "Đặt");
                int chayCount = orders.Count(o => o.Meal.MealName.Contains("chay") && o.Status == "Đặt");
                decimal totalPrice = orders.Where(o => o.Status == "Đặt").Sum(o => o.Price);

                return new LeaderMonthlyReportViewModel
                {
                    EmployeeId = l.EmployeeId,
                    FullName = l.FullName,
                    DepartmentName = l.Department?.DepartmentName ?? "Chưa gán",
                    CostCenter = l.CostCenter,
                    ManCount = manCount,
                    ChayCount = chayCount,
                    TotalPortions = manCount + chayCount,
                    TotalCost = totalPrice
                };
            }).OrderBy(r => r.EmployeeId).ToList();

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.TotalMan = report.Sum(r => r.ManCount);
            ViewBag.TotalChay = report.Sum(r => r.ChayCount);
            ViewBag.GrandTotalPortions = report.Sum(r => r.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(r => r.TotalCost);

            return View(report);
        }

        // POST: Reports/LeaderMonthlyExport
        [HttpPost]
        public ActionResult LeaderMonthly(DateTime fromDate, DateTime toDate, string submitType)
        {
            DateTime start = fromDate.Date;
            DateTime end = toDate.Date;

            // Logic lấy report (giữ nguyên như cũ)
            var leaders = db.Leaders.Include(l => l.Department).ToList();

            var report = leaders.Select(l =>
            {
                var orders = db.LeaderOrders
                    .Include(o => o.Meal)
                    .Where(o => o.EmployeeId == l.EmployeeId && o.Date >= start && o.Date <= end)
                    .ToList();

                int manCount = orders.Count(o => o.Meal.MealName.Contains("mặn") && o.Status == "Đặt");
                int chayCount = orders.Count(o => o.Meal.MealName.Contains("chay") && o.Status == "Đặt");
                decimal totalPrice = orders.Where(o => o.Status == "Đặt").Sum(o => o.Price);

                return new LeaderMonthlyReportViewModel
                {
                    EmployeeId = l.EmployeeId,
                    FullName = l.FullName,
                    DepartmentName = l.Department?.DepartmentName ?? "Chưa gán",
                    CostCenter = l.CostCenter,
                    ManCount = manCount,
                    ChayCount = chayCount,
                    TotalPortions = manCount + chayCount,
                    TotalCost = totalPrice
                };
            }).OrderBy(r => r.EmployeeId).ToList();

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.TotalMan = report.Sum(r => r.ManCount);
            ViewBag.TotalChay = report.Sum(r => r.ChayCount);
            ViewBag.GrandTotalPortions = report.Sum(r => r.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(r => r.TotalCost);

            // Kiểm tra nút nào được bấm
            if (submitType == "export")
            {
                // Logic xuất Excel (copy phần bạn có)
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Báo cáo tháng cán bộ");

                    // Header
                    worksheet.Cells[1, 1].Value = "STT";
                    worksheet.Cells[1, 2].Value = "TT Chi phí";
                    worksheet.Cells[1, 3].Value = "Mã cán bộ";
                    worksheet.Cells[1, 4].Value = "Họ tên";
                    worksheet.Cells[1, 5].Value = "Bộ phận";
                    worksheet.Cells[1, 6].Value = "Cơm mặn (phần)";
                    worksheet.Cells[1, 7].Value = "Cơm chay (phần)";
                    worksheet.Cells[1, 8].Value = "Tổng phần";
                    worksheet.Cells[1, 9].Value = "Tổng tiền (VNĐ)";

                    int row = 2;
                    int stt = 1;
                    foreach (var item in report)
                    {
                        worksheet.Cells[row, 1].Value = stt++;
                        worksheet.Cells[row, 2].Value = item.CostCenter;  // TT Chi phí
                        worksheet.Cells[row, 3].Value = item.EmployeeId;
                        worksheet.Cells[row, 4].Value = item.FullName;
                        worksheet.Cells[row, 5].Value = item.DepartmentName;
                        worksheet.Cells[row, 6].Value = item.ManCount;
                        worksheet.Cells[row, 7].Value = item.ChayCount;
                        worksheet.Cells[row, 8].Value = item.TotalPortions;
                        worksheet.Cells[row, 9].Value = item.TotalCost;
                        row++;
                    }

                    // Tổng cộng
                    worksheet.Cells[row, 1].Value = "Tổng cộng";
                    worksheet.Cells[row, 6].Value = ViewBag.TotalMan;
                    worksheet.Cells[row, 7].Value = ViewBag.TotalChay;
                    worksheet.Cells[row, 8].Value = ViewBag.GrandTotalPortions;
                    worksheet.Cells[row, 9].Value = ViewBag.GrandTotalCost;

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"BaoCaoCanBo_{fromDate:dd-MM-yyyy}_den_{toDate:dd-MM-yyyy}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }

            // Nếu bấm "Xem báo cáo" → hiển thị view
            return View(report);
        }

        // ---------------------- BÁO CÁO THÁNG BỘ PHẬN ----------------------

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}