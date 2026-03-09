using System.Data.Entity;
using CanteenSystem.Models;

namespace CanteenSystem.Data
{
    public class CanteenDbContext : DbContext
    {
        public CanteenDbContext() : base("name=CanteenDbContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserDepartment> UserDepartments { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Kitchen> Kitchens { get; set; }
        public DbSet<MealOrder> MealOrders { get; set; }
        public DbSet<Leader> Leaders { get; set; }
        public DbSet<LeaderOrder> LeaderOrders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Config composite key cho UserDepartment
            modelBuilder.Entity<UserDepartment>()
                .HasKey(ud => new { ud.UserId, ud.DepartmentId });

            // Tương tự cho các index khác nếu muốn, nhưng không bắt buộc vì SQL đã create
        }
    }
}