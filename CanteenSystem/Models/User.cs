using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        [Display(Name = "ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(20)]
        [Index(IsUnique = true)]
        [Column("username")]  
        [Display(Name = "Tên đăng nhập (mã bộ phận)")]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [Column("password")]
        public string Password { get; set; }  // BCrypt hash

        [Required]
        [StringLength(100)]
        [Column("fullname")]
        [Display(Name = "Họ tên")]
        public string Fullname { get; set; }

        [Required]
        [StringLength(10)]
        [Column("role")]
        public string Role { get; set; } = "User";  // "Admin" hoặc "User"

        [Column("cre_date")]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("modi_date")]
        public DateTime? UpdatedAt { get; set; }

        [Column("creator")]
        [StringLength(50)]
        public string Creator { get; set; }

        [Column("modifier")]
        [StringLength(50)]
        public string Modifier { get; set; }

        // Navigation properties
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
    }
}