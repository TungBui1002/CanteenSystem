using CanteenSystem.Data;
using CanteenSystem.Models;
using CanteenSystem.Models.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CanteenSystem.Controllers
{
    public class DashboardController : Controller
    {
        private CanteenDbContext db = new CanteenDbContext();

        // GET: Dashboard
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Không phải Admin → redirect về Login
                return RedirectToAction("Login", "Account");
            }

            DateTime today = DateTime.Today;

            // Tổng quan hôm nay
            var totalPortionsToday = db.MealOrders
                .Where(m => m.Date == today)
                .Sum(m => (int?)m.Quantity) ?? 0;

            var totalCostToday = db.MealOrders
                .Where(m => m.Date == today)
                .Sum(m => (decimal?)m.Price) ?? 0;

            // Top 5 bộ phận báo nhiều nhất hôm nay
            var topDepartments = db.MealOrders
                .Where(m => m.Date == today)
                .GroupBy(m => m.Department)
                .Select(g => new DashboardViewModel
                {
                    DepartmentName = g.Key.DepartmentName ?? "Không xác định",
                    TotalPortions = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(g => g.TotalPortions)
                .Take(5)
                .ToList();

            // Top 3 món ăn theo ca hôm nay
            var topMealsByShift = db.MealOrders
                .Where(m => m.Date == today)
                .GroupBy(m => new { m.Shift, m.Meal.MealName })
                .Select(g => new DashboardViewModel
                {
                    Shift = g.Key.Shift ?? "Không xác định",
                    MealName = g.Key.MealName ?? "-",
                    TotalPortions = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(g => g.TotalPortions)
                .Take(3)
                .ToList();

            ViewBag.TopDepartments = topDepartments;
            ViewBag.TopMealsByShift = topMealsByShift;

            // Thống kê 7 ngày gần nhất
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Select(d => new
                {
                    Date = d,
                    Portions = db.MealOrders.Count(m => m.Date == d)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Tỷ lệ ca (Day/Overtime/Night) hôm nay
            var shiftRatio = db.MealOrders
                .Where(m => m.Date == today)
                .GroupBy(m => m.Shift)
                .Select(g => new
                {
                    Shift = g.Key,
                    Portions = g.Sum(m => m.Quantity)
                })
                .ToList();

            // Số bộ phận chưa báo hôm nay
            var departmentsToday = db.Departments.Count();
            var departmentsReported = db.MealOrders
                .Where(m => m.Date == today)
                .Select(m => m.DepartmentId)
                .Distinct()
                .Count();
            var departmentsNotReported = departmentsToday - departmentsReported;

            // Số cán bộ chưa đặt hôm nay
            var leadersNotOrdered = db.LeaderOrders
                .Count(l => l.Date == today && l.Status == "Chưa đặt");

            ViewBag.TotalPortionsToday = totalPortionsToday;
            ViewBag.TotalCostToday = totalCostToday;
            ViewBag.TopDepartments = topDepartments;
            ViewBag.TopMealsByShift = topMealsByShift;
            ViewBag.Last7Days = last7Days;
            ViewBag.ShiftRatio = shiftRatio;
            ViewBag.DepartmentsNotReported = departmentsNotReported;
            ViewBag.LeadersNotOrdered = leadersNotOrdered;

            return View();
        }
    }
}