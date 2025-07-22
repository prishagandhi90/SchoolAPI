using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using static StudentAPI.Shared.StudentModel;

namespace StudentAPI.Models
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options)
        {

        }
       
        [NotMapped]
        public DbSet<IsValidData> IsValidData { get; set; }

        [NotMapped]
        public DbSet<OTP> OTP { get; set; }

        [NotMapped]
        public DbSet<TokenData> TokenData { get; set; }

        [NotMapped]
        public DbSet<RegistrationModel> RegistrationData { get; set; }
        

        [NotMapped]
        public DbSet<LoginId_TokenData> LoginId_TokenData { get; set; }

        [NotMapped]
        public DbSet<IsValidToken> IsValidToken { get; set; }

        [NotMapped]
        public DbSet<DashBoardList>? DashboardList { get; set; }
        
        [NotMapped]
        public DbSet<Ddl_Value_Nm>? Ddl_Value_Nm { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Ignore<Organization>();
            //modelBuilder.Ignore<Floor>();
            //modelBuilder.Ignore<Ward>();
        }
    }
}
