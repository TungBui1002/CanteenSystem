using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Departments")]
    public class Department
    {
        [Key]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(30)]
        [Index(IsUnique = true)]
        [Column("department_code")]
        [Display(Name = "Mã bộ phận")]
        public string DepartmentCode { get; set; }

        [Required]
        [StringLength(200)]
        [Column("department_name")]
        [Display(Name = "Tên bộ phận")]
        public string DepartmentName { get; set; }

        [StringLength(50)]
        [Column("cost_center")]
        [Display(Name = "Cost Center")]
        public string CostCenter { get; set; }

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
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<MealOrder> MealOrders { get; set; }
        public virtual ICollection<Leader> Leaders { get; set; }
    }
}