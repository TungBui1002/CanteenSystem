using CanteenSystem.Data;
using CanteenSystem.Models;
using CanteenSystem.Models.ViewModel;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System;
using System.Collections.Generic;
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

        // ---------------------------------------------- BÁO CÁO THÁNG CÁN BỘ ---------------------------------------------------------

        // GET: Reports/LeaderMonthly
        public ActionResult LeaderMonthly(DateTime? fromDate, DateTime? toDate)
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

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

        // ---------------------------------------------- BÁO CÁO THÁNG BỘ PHẬN ---------------------------------------------------------

        // GET: Reports/MealMonthly
        public ActionResult MealMonthly(DateTime? fromDate, DateTime? toDate, int? kitchenId)
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime start = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = toDate ?? start.AddMonths(1).AddDays(-1);

            // Query DB
            var query = db.MealOrders
                .Include(m => m.Department)
                .Where(m => m.Date >= start && m.Date <= end);

            // filter nhà ăn
            if (kitchenId.HasValue)
            {
                query = query.Where(m => m.KitchenId == kitchenId.Value);
            }

            // load vào RAM
            var data = query.ToList();

            // build report
            var report = BuildMealMonthlyReport(data);

            // tổng
            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.KitchenId = kitchenId;

            ViewBag.TotalDay = report.Sum(x => x.DayTotal);
            ViewBag.TotalOvertime = report.Sum(x => x.OvertimeTotal);
            ViewBag.TotalNight = report.Sum(x => x.NightTotal);
            ViewBag.GrandTotalPortions = report.Sum(x => x.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(x => x.TotalCost);

            return View(report);
        }

        // POST: Reports/MealMonthlyExport
        [HttpPost]
        public ActionResult MealMonthlyExport(DateTime fromDate, DateTime toDate)
        {
            DateTime start = fromDate.Date;
            DateTime end = toDate.Date;

            var data = db.MealOrders
                .Include(m => m.Department)
                .Where(m => m.Date >= start && m.Date <= end)
                .ToList();

            var allData = data;
            var namPhong = data.Where(x => x.KitchenId == 1).ToList();
            var hongPhat = data.Where(x => x.KitchenId == 2).ToList();

            using (var package = new ExcelPackage())
            {
                CreateSheet(package, "Tất cả", BuildMealMonthlyReport(allData), start, end);

                CreateSheet(package, "Nam Phong", BuildMealMonthlyReport(namPhong), start, end);

                CreateSheet(package, "Hồng Phát", BuildMealMonthlyReport(hongPhat), start, end);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"MealReport_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }

        private List<MealMonthlyReportViewModel> BuildMealMonthlyReport(List<MealOrder> data)
        {
            var report = data
                .GroupBy(m => new { m.DepartmentId, m.PersonnelType })
                .Select(g =>
                {
                    var dept = g.First().Department;

                    var item = new MealMonthlyReportViewModel
                    {
                        CostCenter = dept?.CostCenter ?? "",
                        DepartmentCode = dept?.DepartmentCode ?? "",
                        DepartmentName = dept?.DepartmentName ?? "",
                        PersonnelType = g.Key.PersonnelType ?? "",
                        DayHours = new Dictionary<string, int>(),
                        OvertimeHours = new Dictionary<string, int>(),
                        NightHours = new Dictionary<string, int>()
                    };

                    foreach (var m in g)
                    {
                        string time = m.Time.ToString(@"hh\:mm");

                        if (m.Shift == "Ca sáng")
                        {
                            if (!item.DayHours.ContainsKey(time))
                                item.DayHours[time] = 0;

                            item.DayHours[time] += m.Quantity;
                            item.DayTotal += m.Quantity;
                        }

                        else if (m.Shift == "Tăng ca")
                        {
                            if (!item.OvertimeHours.ContainsKey(time))
                                item.OvertimeHours[time] = 0;

                            item.OvertimeHours[time] += m.Quantity;
                            item.OvertimeTotal += m.Quantity;
                        }

                        else if (m.Shift == "Ca đêm")
                        {
                            if (!item.NightHours.ContainsKey(time))
                                item.NightHours[time] = 0;

                            item.NightHours[time] += m.Quantity;
                            item.NightTotal += m.Quantity;
                        }

                        item.TotalPortions += m.Quantity;
                        item.TotalCost += m.Price;
                    }

                    return item;
                })
                .OrderBy(x => x.DepartmentCode)
                .ThenBy(x => x.PersonnelType)
                .ToList();

            string[] dayTimes = { "06:00", "10:00", "11:30", "12:00" };
            string[] overtimeTimes = { "16:30", "17:00", "20:00" };

            foreach (var r in report)
            {
                foreach (var t in dayTimes)
                    if (!r.DayHours.ContainsKey(t))
                        r.DayHours[t] = 0;

                foreach (var t in overtimeTimes)
                    if (!r.OvertimeHours.ContainsKey(t))
                        r.OvertimeHours[t] = 0;

                if (!r.NightHours.ContainsKey("01:30"))
                    r.NightHours["01:30"] = 0;
            }

            return report;
        }

        private void CreateSheet(ExcelPackage package, string sheetName,List<MealMonthlyReportViewModel> report,DateTime start, DateTime end)
        {
            var ws = package.Workbook.Worksheets.Add(sheetName);

            int row = 1;

            // HEADER DÒNG 1
            ws.Cells[row, 1].Value = "STT";
            ws.Cells[row, 2].Value = "First Day";
            ws.Cells[row, 3].Value = "Last Day";
            ws.Cells[row, 4].Value = "Cost Center";
            ws.Cells[row, 5].Value = "Dept.No";
            ws.Cells[row, 6].Value = "Dept Name";
            ws.Cells[row, 7].Value = "Type";

            ws.Cells[row, 8].Value = "Day Shift";
            ws.Cells[row, 12].Value = "Overtime Shift";
            ws.Cells[row, 15].Value = "Night Shift";

            ws.Cells[row, 17].Value = "Total Portions";
            ws.Cells[row, 18].Value = "Total Cost (VND)";

            // MERGE
            ws.Cells[1, 1, 2, 1].Merge = true;
            ws.Cells[1, 2, 2, 2].Merge = true;
            ws.Cells[1, 3, 2, 3].Merge = true;
            ws.Cells[1, 4, 2, 4].Merge = true;
            ws.Cells[1, 5, 2, 5].Merge = true;
            ws.Cells[1, 6, 2, 6].Merge = true;
            ws.Cells[1, 7, 2, 7].Merge = true;

            ws.Cells[1, 8, 1, 11].Merge = true;
            ws.Cells[1, 12, 1, 14].Merge = true;
            ws.Cells[1, 15, 1, 15].Merge = true;

            ws.Cells[1, 17, 2, 17].Merge = true;
            ws.Cells[1, 18, 2, 18].Merge = true;

            // HEADER DÒNG 2
            row = 2;

            ws.Cells[row, 8].Value = "06:00";
            ws.Cells[row, 9].Value = "10:00";
            ws.Cells[row, 10].Value = "11:30";
            ws.Cells[row, 11].Value = "12:00";

            ws.Cells[row, 12].Value = "16:30";
            ws.Cells[row, 13].Value = "17:00";
            ws.Cells[row, 14].Value = "20:00";

            ws.Cells[row, 15].Value = "01:30";

            // STYLE HEADER
            using (var range = ws.Cells[1, 1, 2, 18])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            row = 3;
            int stt = 1;

            foreach (var item in report)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = start.ToString("dd/MM/yyyy");
                ws.Cells[row, 3].Value = end.ToString("dd/MM/yyyy");

                ws.Cells[row, 4].Value = item.CostCenter;
                ws.Cells[row, 5].Value = item.DepartmentCode;
                ws.Cells[row, 6].Value = item.DepartmentName;
                ws.Cells[row, 7].Value = item.PersonnelType;

                ws.Cells[row, 8].Value = item.DayHours.ContainsKey("06:00") ? item.DayHours["06:00"] : 0;
                ws.Cells[row, 9].Value = item.DayHours.ContainsKey("10:00") ? item.DayHours["10:00"] : 0;
                ws.Cells[row, 10].Value = item.DayHours.ContainsKey("11:30") ? item.DayHours["11:30"] : 0;
                ws.Cells[row, 11].Value = item.DayHours.ContainsKey("12:00") ? item.DayHours["12:00"] : 0;

                ws.Cells[row, 12].Value = item.OvertimeHours.ContainsKey("16:30") ? item.OvertimeHours["16:30"] : 0;
                ws.Cells[row, 13].Value = item.OvertimeHours.ContainsKey("17:00") ? item.OvertimeHours["17:00"] : 0;
                ws.Cells[row, 14].Value = item.OvertimeHours.ContainsKey("20:00") ? item.OvertimeHours["20:00"] : 0;

                ws.Cells[row, 15].Value = item.NightHours.ContainsKey("01:30") ? item.NightHours["01:30"] : 0;

                ws.Cells[row, 17].Value = item.TotalPortions;
                ws.Cells[row, 18].Value = item.TotalCost;

                ws.Cells[row, 18].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // TỔNG
            ws.Cells[row, 1].Value = "Tổng cộng";
            ws.Cells[row, 1, row, 7].Merge = true;

            ws.Cells[row, 17].Value = report.Sum(x => x.TotalPortions);
            ws.Cells[row, 18].Value = report.Sum(x => x.TotalCost);

            ws.Cells[row, 18].Style.Numberformat.Format = "#,##0";

            using (var range = ws.Cells[row, 1, row, 18])
            {
                range.Style.Font.Bold = true;
            }

            ws.Cells.AutoFitColumns();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}