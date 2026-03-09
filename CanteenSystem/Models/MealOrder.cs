using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_MealOrders")]
    public class MealOrder
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [Column("date")]
        [Display(Name = "Ngày")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(20)]
        [Column("shift")]
        [Display(Name = "Ca làm việc")]
        public string Shift { get; set; }  // Day, Overtime, Night

        [Required]
        [Column("time")]
        [Display(Name = "Giờ")]
        public TimeSpan Time { get; set; }

        [Required]
        [Column("meal_id")]
        public int MealId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("personnel_type")]
        [Display(Name = "Loại nhân sự")]
        public string PersonnelType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column("price")]
        public decimal Price { get; set; }

        [Required]
        [Column("kitchen_id")]
        public int KitchenId { get; set; }

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
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }

        [ForeignKey("KitchenId")]
        public virtual Kitchen Kitchen { get; set; }
    }
}