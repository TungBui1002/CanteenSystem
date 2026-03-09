using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Meals")]
    public class Meal
    {
        [Key]
        [Column("meal_id")]
        public int MealId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("meal_name")]
        [Display(Name = "Tên món")]
        public string MealName { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column("price")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        [Column("applicable_for")]
        [Display(Name = "Áp dụng cho")]
        public string ApplicableFor { get; set; }  // "Department" hoặc "Leader"

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
        public virtual ICollection<MealOrder> MealOrders { get; set; }
        public virtual ICollection<LeaderOrder> LeaderOrders { get; set; }
    }
}