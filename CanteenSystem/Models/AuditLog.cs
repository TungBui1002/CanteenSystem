using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_Audit_Logs")]
    public class AuditLog
    {
        [Key]
        [Column("log_id")]
        public long LogId { get; set; }  // BIGINT → long

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("action")]
        public string Action { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Column("details")]
        public string Details { get; set; }

        [StringLength(45)]
        [Column("ip_address")]
        [Display(Name = "IP Address")]
        public string IpAddress { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}