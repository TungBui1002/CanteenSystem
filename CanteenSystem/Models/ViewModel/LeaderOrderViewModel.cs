using System;

namespace CanteenSystem.Models.ViewModel
{
    public class LeaderOrderViewModel
    {
        public int? OrderId { get; set; }
        public string CostCenter { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string MealName { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Creator { get; set; }
    }
}