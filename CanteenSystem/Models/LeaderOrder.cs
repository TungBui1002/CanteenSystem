using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_LeaderOrders")]
    public class LeaderOrder
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("employee_id")]
        [StringLength(20)]
        public string EmployeeId { get; set; }

        [Required]
        [Column("date")]
        [Display(Name = "Ngày")]
        public DateTime Date { get; set; }

        [Required]
        [Column("meal_id")]
        public int MealId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("status")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }  // Đặt, Chưa đặt

        [Required]
        [Range(0, double.MaxValue)]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("cre_date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("modi_date")]
        public DateTime? UpdatedAt { get; set; }

        [Column("creator")]
        [StringLength(50)]
        public string Creator { get; set; }

        [Column("modifier")]
        [StringLength(50)]
        public string Modifier { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Leader Leader { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
    }
}