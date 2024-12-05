using Microsoft.EntityFrameworkCore;
using static VHEmpAPI.Shared.CommonProcOutputFields;
using System.ComponentModel.DataAnnotations.Schema;
using VHEmpAPI.Shared;

namespace VHEmpAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

       
        [NotMapped]
        public DbSet<IsValidData> IsValidData { get; set; }

        [NotMapped]
        public DbSet<OTP> OTP { get; set; }

        [NotMapped]
        public DbSet<TokenData> TokenData { get; set; }

        [NotMapped]
        public DbSet<LoginId_TokenData> LoginId_TokenData { get; set; }

        [NotMapped]
        public DbSet<IsValidToken> IsValidToken { get; set; }

        [NotMapped]
        public DbSet<DashBoardList>? DashboardList { get; set; }
        
        [NotMapped]
        public DbSet<Ddl_Value_Nm>? Ddl_Value_Nm { get; set; }

        [NotMapped]
        public DbSet<Resp_MispunchDtl_EmpInfo>? Resp_MispunchDtl_EmpInfo { get; set; }

        [NotMapped]
        public DbSet<Resp_AttDtl_EmpInfo>? Resp_AttDtl_EmpInfo { get; set; }

        [NotMapped]
        public DbSet<Resp_AttSumm_EmpInfo>? Resp_AttSumm_EmpInfo { get; set; }

        [NotMapped]
        public DbSet<ret_EmpSummary_Dashboard>? EmpSummary_Dashboard { get; set; }

        [NotMapped]
        public DbSet<OutSingleString>? OutSingleString { get; set; }

        [NotMapped]
        public DbSet<Resp_value_name>? Resp_Value_Name { get; set; }

        [NotMapped]
        public DbSet<Resp_name>? Resp_Name { get; set; }

        [NotMapped]
        public DbSet<Resp_LvDelayReason>? Resp_LvDelayReason { get; set; }

        [NotMapped]
        public DbSet<Resp_id_name>? Resp_id_name { get; set; }
        
        [NotMapped]
        public DbSet<Resp_LvEntryList>? Resp_LvEntryList { get; set; }

        [NotMapped]
        public DbSet<Resp_HeaderEntryList>? Resp_HeaderEntryList { get; set; }

        [NotMapped]
        public DbSet<SavedYesNo>? SavedYesNo { get; set; }

        [NotMapped]
        public DbSet<Resp_Dr_PrecriptionViewer>? Resp_Dr_PrecriptionViewer { get; set; }

        [NotMapped]
        public DbSet<Resp_Dr_PrecriptionMedicines>? Resp_Dr_PrecriptionMedicines { get; set; }

        [NotMapped]
        public DbSet<Wards> Wards { get; set; }

        [NotMapped]
        public DbSet<Floors> Floors { get; set; }

        [NotMapped]
        public DbSet<Beds> Beds { get; set; }

        [NotMapped]
        public DbSet<DoctorNotification> DrNotification { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Ignore<Organization>();
            //modelBuilder.Ignore<Floor>();
            //modelBuilder.Ignore<Ward>();
        }
    }
}
