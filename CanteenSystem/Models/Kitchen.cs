using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Kitchens")]
    public class Kitchen
    {
        [Key]
        [Column("kitchen_id")]
        public int KitchenId { get; set; }

        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)]
        [Column("kitchen_name")]
        [Display(Name = "Tên nhà ăn")]
        public string KitchenName { get; set; }

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
    }
}