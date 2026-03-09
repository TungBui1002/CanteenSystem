using System.ComponentModel.DataAnnotations.Schema;

namespace CanteenSystem.Models
{
    [Table("ORD_UserDepartments")]
    public class UserDepartment
    {
        [Column("user_id")]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Column("department_id")]
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        public virtual User User { get; set; }
        public virtual Department Department { get; set; }
    }
}