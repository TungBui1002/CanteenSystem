using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanteenSystem.Models.ViewModel
{
    public class DashboardViewModel
    {
        //Top bộ phận báo nhiều nhất
        public string DepartmentName { get; set; }
        public int TotalPortions { get; set; }

        // Cho Top món ăn
        public string Shift { get; set; }
        public string MealName { get; set; }
    }
}