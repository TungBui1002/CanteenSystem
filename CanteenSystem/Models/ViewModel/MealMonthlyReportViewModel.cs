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

        // Phân loại theo giá
        public int Qty17k { get; set; }       // Món Mì (MealId = 7), giá 17,000đ
        public int Qty25k { get; set; }       // Các món còn lại, giá 25,000đ
        public decimal Total17k { get; set; } // Qty17k × 17,000
        public decimal Total25k { get; set; } // Qty25k × 25,000
    }
}