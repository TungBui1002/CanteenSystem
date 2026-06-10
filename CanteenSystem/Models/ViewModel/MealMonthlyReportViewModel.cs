using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanteenSystem.Models.ViewModel
{
    public class MealMonthlyReportViewModel
    {
        public string CostCenter { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string PersonnelType { get; set; }

        // Tổng phần theo ca
        public int DayTotal { get; set; }
        public Dictionary<string, int> DayHours { get; set; } = new Dictionary<string, int>();

        public int OvertimeTotal { get; set; }
        public Dictionary<string, int> OvertimeHours { get; set; } = new Dictionary<string, int>();

        public int NightTotal { get; set; }
        public Dictionary<string, int> NightHours { get; set; } = new Dictionary<string, int>();

        public int TotalPortions { get; set; }
        public decimal TotalCost { get; set; }

        public int Night0130Mi { get; set; }          // Mì ăn liền 01:30
        public int Night0130ComMan { get; set; }      // Cơm mặn 01:30
        public int Night0130ComChay { get; set; }     // Cơm chay 01:30
        public int Night0130Pho { get; set; }         // Phở 01:30
    }
}