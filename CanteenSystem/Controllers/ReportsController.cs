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
                return RedirectToAction("Login", "Account");
            }

            DateTime start = fromDate ?? DateTime.Today.AddMonths(-1).Date;
            DateTime end = toDate ?? DateTime.Today.Date;

            var orders = db.LeaderOrders
                .Include(o => o.Meal)
                .Include(o => o.Leader)
                .Where(o => o.Date >= start && o.Date <= end && o.Leader.IsActive)
                .ToList();

            var report = orders
                .GroupBy(o => o.EmployeeId)
                .Select(g =>
                {
                    var first = g.First();

                    int manCount = g.Where(o => o.Status == "Đặt" && o.Meal?.MealName.Contains("mặn") == true)
                                   .Sum(o => o.Quantity);

                    int chayCount = g.Where(o => o.Status == "Đặt" && o.Meal?.MealName.Contains("chay") == true)
                                    .Sum(o => o.Quantity);

                    // Chỉ tính những record đã "Đặt"
                    int totalPortions = g.Where(o => o.Status == "Đặt").Sum(o => o.Quantity);
                    // Tổng tiền = Đơn giá × Số lượng (chỉ record đã Đặt)
                    decimal totalCost = g.Where(o => o.Status == "Đặt").Sum(o => o.Price * o.Quantity);

                    return new LeaderMonthlyReportViewModel
                    {
                        EmployeeId = first.EmployeeId,
                        FullName = first.Leader.FullName,
                        DepartmentName = first.Leader.Department?.DepartmentName ?? "Chưa gán",
                        CostCenter = first.Leader.CostCenter,
                        Category = first.Leader.Category ?? "-",
                        UnitPrice = first.Price,  // Lấy đơn giá thực từ DB

                        ManCount = manCount,
                        ChayCount = chayCount,
                        TotalPortions = totalPortions,
                        TotalCost = totalCost
                    };
                })
                .OrderBy(x => x.EmployeeId)
                .ToList();

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.TotalMan = report.Sum(r => r.ManCount);
            ViewBag.TotalChay = report.Sum(r => r.ChayCount);
            ViewBag.GrandTotalPortions = report.Sum(r => r.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(r => r.TotalCost);

            return View(report);
        }

        // POST: Reports/LeaderMonthly (hỗ trợ cả Xem và Export)
        [HttpPost]
        public ActionResult LeaderMonthly(DateTime fromDate, DateTime toDate, string submitType)
        {
            DateTime start = fromDate.Date;
            DateTime end = toDate.Date;

            var orders = db.LeaderOrders
                .Include(o => o.Meal)
                .Include(o => o.Leader)
                .Where(o => o.Date >= start && o.Date <= end && o.Leader.IsActive)
                .ToList();

            var report = orders
                .GroupBy(o => o.EmployeeId)
                .Select(g =>
                {
                    var first = g.First();

                    int manCount = g.Where(o => o.Status == "Đặt" && o.Meal?.MealName.Contains("mặn") == true)
                                   .Sum(o => o.Quantity);

                    int chayCount = g.Where(o => o.Status == "Đặt" && o.Meal?.MealName.Contains("chay") == true)
                                    .Sum(o => o.Quantity);

                    // Chỉ tính những record đã "Đặt"
                    int totalPortions = g.Where(o => o.Status == "Đặt").Sum(o => o.Quantity);
                    // Tổng tiền = Đơn giá × Số lượng (chỉ record đã Đặt)
                    decimal totalCost = g.Where(o => o.Status == "Đặt").Sum(o => o.Price * o.Quantity);

                    return new LeaderMonthlyReportViewModel
                    {
                        EmployeeId = first.EmployeeId,
                        FullName = first.Leader.FullName,
                        DepartmentName = first.Leader.Department?.DepartmentName ?? "Chưa gán",
                        CostCenter = first.Leader.CostCenter,
                        Category = first.Leader.Category ?? "-",
                        UnitPrice = first.Price,  // Lấy đơn giá thực từ DB

                        ManCount = manCount,
                        ChayCount = chayCount,
                        TotalPortions = totalPortions,
                        TotalCost = totalCost
                    };
                })
                .OrderBy(x => x.EmployeeId)
                .ToList();

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.TotalMan = report.Sum(r => r.ManCount);
            ViewBag.TotalChay = report.Sum(r => r.ChayCount);
            ViewBag.GrandTotalPortions = report.Sum(r => r.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(r => r.TotalCost);

            if (submitType == "export")
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Báo cáo tháng cán bộ");

                    worksheet.Cells[1, 1].Value = "STT";
                    worksheet.Cells[1, 2].Value = "TT Chi phí";
                    worksheet.Cells[1, 3].Value = "Mã cán bộ";
                    worksheet.Cells[1, 4].Value = "Họ tên";
                    worksheet.Cells[1, 5].Value = "Bộ phận";
                    worksheet.Cells[1, 6].Value = "Phân loại";
                    worksheet.Cells[1, 7].Value = "Đơn giá";
                    worksheet.Cells[1, 8].Value = "Cơm mặn (phần)";
                    worksheet.Cells[1, 9].Value = "Cơm chay (phần)";
                    worksheet.Cells[1, 10].Value = "Tổng phần";
                    worksheet.Cells[1, 11].Value = "Tổng tiền (VNĐ)";

                    int row = 2;
                    int stt = 1;
                    foreach (var item in report)
                    {
                        worksheet.Cells[row, 1].Value = stt++;
                        worksheet.Cells[row, 2].Value = item.CostCenter;
                        worksheet.Cells[row, 3].Value = item.EmployeeId;
                        worksheet.Cells[row, 4].Value = item.FullName;
                        worksheet.Cells[row, 5].Value = item.DepartmentName;
                        worksheet.Cells[row, 6].Value = item.Category;
                        worksheet.Cells[row, 7].Value = item.UnitPrice;
                        worksheet.Cells[row, 8].Value = item.ManCount;
                        worksheet.Cells[row, 9].Value = item.ChayCount;
                        worksheet.Cells[row, 10].Value = item.TotalPortions;
                        worksheet.Cells[row, 11].Value = item.TotalCost;
                        row++;
                    }

                    worksheet.Cells[row, 1].Value = "Tổng cộng";
                    worksheet.Cells[row, 8].Value = ViewBag.TotalMan;
                    worksheet.Cells[row, 9].Value = ViewBag.TotalChay;
                    worksheet.Cells[row, 10].Value = ViewBag.GrandTotalPortions;
                    worksheet.Cells[row, 11].Value = ViewBag.GrandTotalCost;

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"BaoCaoCanBo_{fromDate:dd-MM-yyyy}_den_{toDate:dd-MM-yyyy}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }

            return View(report);
        }

        // --------------------------------------------------- BÁO CÁO THÁNG BỘ PHẬN ---------------------------------------------------------------------------//

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

            // TÍNH TỔNG THEO TỪNG GIỜ 
            ViewBag.TotalDay06 = report.Sum(x => x.DayHours.ContainsKey("06:00") ? x.DayHours["06:00"] : 0);
            ViewBag.TotalDay10 = report.Sum(x => x.DayHours.ContainsKey("10:00") ? x.DayHours["10:00"] : 0);
            ViewBag.TotalDay1130 = report.Sum(x => x.DayHours.ContainsKey("11:30") ? x.DayHours["11:30"] : 0);
            ViewBag.TotalDay12 = report.Sum(x => x.DayHours.ContainsKey("12:00") ? x.DayHours["12:00"] : 0);

            ViewBag.TotalOvertime1630 = report.Sum(x => x.OvertimeHours.ContainsKey("16:30") ? x.OvertimeHours["16:30"] : 0);
            ViewBag.TotalOvertime17 = report.Sum(x => x.OvertimeHours.ContainsKey("17:00") ? x.OvertimeHours["17:00"] : 0);

            ViewBag.TotalNight20 = report.Sum(x => x.NightHours.ContainsKey("20:00") ? x.NightHours["20:00"] : 0);
            ViewBag.TotalNight0130 = report.Sum(x => x.NightHours.ContainsKey("01:30") ? x.NightHours["01:30"] : 0);

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;
            ViewBag.KitchenId = kitchenId;

            ViewBag.GrandTotalPortions = report.Sum(x => x.TotalPortions);
            ViewBag.GrandTotalCost = report.Sum(x => x.TotalCost);
            ViewBag.GrandQty17k = report.Sum(x => x.Qty17k);
            ViewBag.GrandQty25k = report.Sum(x => x.Qty25k);
            ViewBag.GrandTotal17k = report.Sum(x => x.Total17k);
            ViewBag.GrandTotal25k = report.Sum(x => x.Total25k);

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
                        NightHours = new Dictionary<string, int>(),
                    };

                    const int MealIdMi = 7;           // Món Mì — giá 17,000đ
                    const decimal Price17k = 17000m;
                    const decimal Price25k = 25000m;

                    foreach (var m in g)
                    {
                        string timeStr = m.Time.ToString(@"hh\:mm");

                        // --- Phân ca theo giờ ---
                        if (timeStr == "06:00" || timeStr == "10:00" || timeStr == "11:30" || timeStr == "12:00")
                        {
                            if (!item.DayHours.ContainsKey(timeStr)) item.DayHours[timeStr] = 0;
                            item.DayHours[timeStr] += m.Quantity;
                            item.DayTotal += m.Quantity;
                        }
                        else if (timeStr == "16:30" || timeStr == "17:00")
                        {
                            if (!item.OvertimeHours.ContainsKey(timeStr)) item.OvertimeHours[timeStr] = 0;
                            item.OvertimeHours[timeStr] += m.Quantity;
                            item.OvertimeTotal += m.Quantity;
                        }
                        else if (timeStr == "20:00" || timeStr == "01:30")
                        {
                            if (!item.NightHours.ContainsKey(timeStr)) item.NightHours[timeStr] = 0;
                            item.NightHours[timeStr] += m.Quantity;
                            item.NightTotal += m.Quantity;
                        }

                        // --- Phân loại 17k / 25k theo MealId ---
                        if (m.MealId == MealIdMi)
                        {
                            item.Qty17k += m.Quantity;
                            item.Total17k += Price17k * m.Quantity;
                        }
                        else
                        {
                            item.Qty25k += m.Quantity;
                            item.Total25k += Price25k * m.Quantity;
                        }

                        item.TotalPortions += m.Quantity;
                    }

                    // Tổng tiền = 17k + 25k
                    item.TotalCost = item.Total17k + item.Total25k;

                    return item;
                })
                .OrderBy(x => x.DepartmentCode)
                .ThenBy(x => x.PersonnelType)
                .ToList();

            // Khởi tạo key
            string[] dayTimes = { "06:00", "10:00", "11:30", "12:00" };
            string[] overtimeTimes = { "16:30", "17:00" };
            string[] nightTimes = { "20:00", "01:30" };

            foreach (var r in report)
            {
                foreach (var t in dayTimes) if (!r.DayHours.ContainsKey(t)) r.DayHours[t] = 0;
                foreach (var t in overtimeTimes) if (!r.OvertimeHours.ContainsKey(t)) r.OvertimeHours[t] = 0;
                foreach (var t in nightTimes) if (!r.NightHours.ContainsKey(t)) r.NightHours[t] = 0;
            }

            return report;
        }

        private void CreateSheet(ExcelPackage package, string sheetName, List<MealMonthlyReportViewModel> report, DateTime start, DateTime end)
        {
            var ws = package.Workbook.Worksheets.Add(sheetName);

            int row = 1;

            // ==================== HEADER DÒNG 1 ====================
            ws.Cells[row, 1].Value = "STT";
            ws.Cells[row, 2].Value = "Ngày bắt đầu";
            ws.Cells[row, 3].Value = "Ngày kết thúc";
            ws.Cells[row, 4].Value = "TT chịu phí";
            ws.Cells[row, 5].Value = "Mã bộ phận";
            ws.Cells[row, 6].Value = "Tên bộ phận";
            ws.Cells[row, 7].Value = "Phân loại";

            ws.Cells[row, 8].Value = "Ca ngày";
            ws.Cells[row, 12].Value = "Tăng ca";
            ws.Cells[row, 14].Value = "Ca đêm";

            ws.Cells[row, 16].Value = "SL 17k";
            ws.Cells[row, 17].Value = "SL 25k";
            ws.Cells[row, 18].Value = "Tổng phần";
            ws.Cells[row, 19].Value = "Tổng tiền (VNĐ)";
            ws.Cells[row, 20].Value = "Tổng giá 17K";
            ws.Cells[row, 21].Value = "Tổng giá 25K";

            // Merge
            ws.Cells[1, 8, 1, 11].Merge = true;   // Ca ngày
            ws.Cells[1, 12, 1, 13].Merge = true;  // Tăng ca
            ws.Cells[1, 14, 1, 15].Merge = true;  // Ca đêm (2 cột: 20:00 và 01:30)

            // ==================== HEADER DÒNG 2 ====================
            row = 2;

            // Ca ngày
            ws.Cells[row, 8].Value = "06:00";
            ws.Cells[row, 9].Value = "10:00";
            ws.Cells[row, 10].Value = "11:30";
            ws.Cells[row, 11].Value = "12:00";

            // Tăng ca
            ws.Cells[row, 12].Value = "16:30";
            ws.Cells[row, 13].Value = "17:00";

            // Ca đêm
            ws.Cells[row, 14].Value = "20:00";
            ws.Cells[row, 15].Value = "01:30";

            // Style header
            using (var range = ws.Cells[1, 1, 2, 21])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
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

                // Ca ngày
                ws.Cells[row, 8].Value = item.DayHours.ContainsKey("06:00") ? item.DayHours["06:00"] : 0;
                ws.Cells[row, 9].Value = item.DayHours.ContainsKey("10:00") ? item.DayHours["10:00"] : 0;
                ws.Cells[row, 10].Value = item.DayHours.ContainsKey("11:30") ? item.DayHours["11:30"] : 0;
                ws.Cells[row, 11].Value = item.DayHours.ContainsKey("12:00") ? item.DayHours["12:00"] : 0;

                // Tăng ca
                ws.Cells[row, 12].Value = item.OvertimeHours.ContainsKey("16:30") ? item.OvertimeHours["16:30"] : 0;
                ws.Cells[row, 13].Value = item.OvertimeHours.ContainsKey("17:00") ? item.OvertimeHours["17:00"] : 0;

                // Ca đêm
                ws.Cells[row, 14].Value = item.NightHours.ContainsKey("20:00") ? item.NightHours["20:00"] : 0;
                ws.Cells[row, 15].Value = item.NightHours.ContainsKey("01:30") ? item.NightHours["01:30"] : 0;

                // Phân loại giá
                ws.Cells[row, 16].Value = item.Qty17k;
                ws.Cells[row, 17].Value = item.Qty25k;
                ws.Cells[row, 18].Value = item.TotalPortions;
                ws.Cells[row, 19].Value = item.TotalCost;
                ws.Cells[row, 20].Value = item.Total17k;
                ws.Cells[row, 21].Value = item.Total25k;

                ws.Cells[row, 19].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 20].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 21].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // ==================== TỔNG CỘNG ====================
            ws.Cells[row, 1].Value = "Tổng cộng";
            ws.Cells[row, 1, row, 7].Merge = true;

            ws.Cells[row, 16].Value = report.Sum(x => x.Qty17k);
            ws.Cells[row, 17].Value = report.Sum(x => x.Qty25k);
            ws.Cells[row, 18].Value = report.Sum(x => x.TotalPortions);
            ws.Cells[row, 19].Value = report.Sum(x => x.TotalCost);
            ws.Cells[row, 20].Value = report.Sum(x => x.Total17k);
            ws.Cells[row, 21].Value = report.Sum(x => x.Total25k);

            ws.Cells[row, 16, row, 21].Style.Font.Bold = true;
            ws.Cells[row, 19].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 20].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 21].Style.Numberformat.Format = "#,##0";

            ws.Cells.AutoFitColumns();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}