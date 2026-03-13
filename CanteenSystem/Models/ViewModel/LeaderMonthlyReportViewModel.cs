using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanteenSystem.Models.ViewModel
{
    public class LeaderMonthlyReportViewModel
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string CostCenter { get; set; }
        public string DepartmentName { get; set; }
        public int ManCount { get; set; }
        public int ChayCount { get; set; }
        public int TotalPortions { get; set; }
        public decimal TotalCost { get; set; }
    }
}