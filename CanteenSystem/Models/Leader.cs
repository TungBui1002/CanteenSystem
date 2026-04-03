using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Leader")]
    public class Leader
    {
        [Key]
        [Column("employee_id")]
        [Display(Name = "Mã số thẻ")]
        [StringLength(20)]
        public string EmployeeId { get; set; }

        [StringLength(50)]
        [Column("cost_center")]
        [Display(Name = "Trung tâm chịu phí")]
        public string CostCenter { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [StringLength(50)]
        [Column("rank")]
        [Display(Name = "Chức vụ")]
        public string Rank { get; set; }

        [Required]
        [StringLength(100)]
        [Column("full_name")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

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

        public virtual ICollection<LeaderOrder> LeaderOrders { get; set; }
    }
}