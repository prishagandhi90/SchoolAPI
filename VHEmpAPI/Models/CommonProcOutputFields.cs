using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VHEmpAPI.Shared
{
    public class CommonProcOutputFields
    {
        [Keyless]
        public class MobileNum
        {
            public string? MobileNo { get; set; }
        }

        [Keyless]
        public class LoginIdNum
        {
            public string? LoginId { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
        }

        [Keyless]
        public class IsValidData
        {
            public string? IsValid { get; set; }
            public string? Message { get; set; }
        }

        [Keyless]
        public class IsValidToken
        {
            public string? IsValid { get; set; }
            public string? UserId { get; set; }
        }

        [Keyless]
        public class OTP
        {
            public string? MobileNo { get; set; }
            public string? OTPNo { get; set; }
            public string? Message { get; set; }
        }

        [Keyless]
        public class MobileCreds
        {
            public string? MobileNo { get; set; }
            public string? Password { get; set; }
            public string? OTP { get; set; }
            public string? DeviceType { get; set; }
            public string? DeviceName { get; set; }
            public string? OSType { get; set; }
            public string? DeviceToken { get; set; }
            public string? FirebaseId { get; set; }
        }

        [Keyless]
        public class LoginId_TokenData
        {
            public string? LoginId { get; set; }
            public string? TokenNo { get; set; }
            public string? IsValidCreds { get; set; }
        }

        [Keyless]
        public class TokenData
        {
            public string? TokenNo { get; set; }
            public string? IsValidCreds { get; set; }
        }

        [Keyless]
        public class DashBoardList
        {
            public string? is_valid_token { get; set; }
            public Int64? login_id { get; set; }
            public string? token { get; set; }
            public int? EmployeeId { get; set; }
            public string? EmployeeName { get; set; }
            public string? MobileNumber { get; set; }
            public string? EmailAddress { get; set; }
            public string? DOB { get; set; }
            public string? Address { get; set; }
            public string? EmpCode { get; set; }
            public string? Department { get; set; }
            public string? Designation { get; set; }
            public string? Emp_Type { get; set; }
            public string? IsSuperAdmin { get; set; }
        }

        [Keyless]
        public class Ddl_Value_Nm
        {
            //public string? IsValidToken { get; set; }
            public string? Value { get; set; }
            public string? Name { get; set; }
        }

        //[Keyless]
        //public class LoginAPIs
        //{
        //    public List<UserName>? Company { get; set; }
        //    public List<UserName>? Year { get; set; }
        //}


        [Keyless]
        public class LoginOTP
        {
            public string? statusCode { get; set; }
            public string? Message { get; set; }
            public OTP? Data { get; set; }
        }

        [Keyless]
        public class MonthYr_EmpInfo
        {
            public string? MonthYr { get; set; }
        }

        [Keyless]
        public class ret_EmpSummary_Dashboard
        {
            public string? EmployeeName { get; set; }
            public string? EmployeeCode { get; set; }
            public string? Department { get; set; }
            //public string? PunchDate { get; set; }
            public string? InPunchTime { get; set; }
            public string? OutPunchTime { get; set; }
            //public string? Direction { get; set; }
            //public string? TotEGMin { get; set; }
            //public string? TotLateMin { get; set; }
            public string? TotLCEGMin { get; set; }
            public string? cnt { get; set; }
        }

        [Keyless]
        public class MispunchDtl_EmpInfo
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? MonthYr { get; set; }
        }

        [Keyless]
        public class Resp_AttSumm_EmpInfo
        {
            public decimal? TOT_P { get; set; }
            public decimal? TOT_A { get; set; }
            public decimal? TOT_DAYS { get; set; }
            public decimal? P { get; set; }
            public decimal? A { get; set; }
            public decimal? WO { get; set; }
            public decimal? CO { get; set; }
            public decimal? PL { get; set; }
            public decimal? SL { get; set; }
            public decimal? CL { get; set; }
            public decimal? HO { get; set; }
            public decimal? ML { get; set; }
            public decimal? CH { get; set; }
            public decimal? LC_EG_MIN { get; set; }
            public decimal? LC_EG_CNT { get; set; }
            public decimal? N_OT_HRS { get; set; }
            public decimal? C_OT_HRS { get; set; }
            public decimal? TTL_OT_HRS { get; set; }
            public string? DUTY_HRS { get; set; }
            public string? DUTY_ST { get; set; }
        }

        [Keyless]
        public class Resp_AttDtl_EmpInfo
        {
            public string? MNTH_YR { get; set; }
            public int? EMPL_CD { get; set; }
            public string? ATT_DATE { get; set; }
            public string? IN_ { get; set; }
            public string? OUT { get; set; }
            public string? PUNCH { get; set; }
            public string? SHIFT { get; set; }
            public string? LV { get; set; }
            public string? ST { get; set; }
            public string? OT_ENT_MIN { get; set; }
            public string? OT_MIN { get; set; }
            public string? LC { get; set; }
            public string? EG { get; set; }
            public string? LC_EG_MIN { get; set; }
            public string? RedYN_LC_EG_MIN { get; set; }
            public string? RedYN_IN_TM { get; set; }
            public string? RedYN_OUT_TM { get; set; }
            //public string? ACT_EARLY_GOING { get; set; }
            //public decimal? act_late_early_min { get; set; }
            //public decimal? USR_LATE_EARLY_MIN { get; set; }
            //public string? PUNCH_TIME { get; set; }
            //public string? LEAVE_TYP { get; set; }
            //public string? Remark { get; set; }
            //public decimal? OT_ENT_MIN { get; set; }
        }

        [Keyless]
        public class Resp_MispunchDtl_EmpInfo
        {
            public string? EmpCode { get; set; }
            public string? EmpName { get; set; }
            public string? Department { get; set; }
            public string? Designation { get; set; }
            public string? Emp_Type { get; set; }
            public string? Dt { get; set; }
            public string? Mis_Punch { get; set; }
            public string? Punch_Time { get; set; }
            public string? ShiftTime { get; set; }
            public string? Note { get; set; }
        }

        [Keyless]
        public class GetLeaveDays
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? LeaveType { get; set; }
            public string? LeaveDate { get; set; }
        }

        [Keyless]
        public class OutSingleString
        {
            public string? OutValue { get; set; }
        }

        [Keyless]
        public class Resp_value_name
        {
            public string? Value { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_id_name
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_name
        {
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_LvDelayReason
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_LvEntryList
        {
            public int? LeaveId { get; set; }
            public string? TypeValue { get; set; }
            public string? TypeName { get; set; }
            public string? LeaveFullName { get; set; }

            public string? EmployeeCodeValue { get; set; }
            public string? EmployeeCodeName { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            public decimal? OverTimeMinutes { get; set; }
            public string? LeaveDays { get; set; }
            public string? Reason { get; set; }
            public string? HRAction { get; set; }
            public string? InchargeAction { get; set; }
            public string? HodAction { get; set; }
            public string? HRReason { get; set; }
            public string? InchargeReason { get; set; }
            public string? HodReason { get; set; }
            public string? EmpType { get; set; }
            public string? Department { get; set; }
            public string? SubDept { get; set; }
            public string? Note { get; set; }
            public string? DeptInc { get; set; }
            public string? DeptHOD { get; set; }
            public string? SubDeptInc { get; set; }
            public string? SubDeptHOD { get; set; }
            public string? InchargeNote { get; set; }
            public string? HoDNote { get; set; }
            public string? HRNote { get; set; }
            public string? EnterDate { get; set; }
            public string? RelieverEmpCode { get; set; }
            public string? RelieverEmpName { get; set; }
            public string? LateReasonId { get; set; }
            public string? LateReasonName { get; set; }
            public string? EmpEmail { get; set; }
            public string? OTHours { get; set; }
            public string? EmpTel { get; set; }
            public string? ShiftTime { get; set; }
            public string? InchargeDate { get; set; }
            public string? HodDate { get; set; }
            public string? HRDate { get; set; }

        }

         [Keyless]
        public class Resp_HeaderEntryList
        {
            public string? Department { get; set; }
            public string? SubDept { get; set; }
            public string? DeptInc { get; set; }
            public string? DeptHOD { get; set; }
            public string? SubDeptInc { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_Flag
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? Flag { get; set; }
        }

        [Keyless]
        public class SaveLeaveEntry
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? EntryType { get; set; }
            public string? LeaveShortName { get; set; }
            public string? LeaveFullName { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            public string? Reason { get; set; }
            public string? Note{ get; set; }
            public decimal? LeaveDays { get; set; }
            public decimal? OverTimeMinutes { get; set; }
            public string? Usr_Nm { get; set; }
            public string? Reliever_Empcode { get; set; }
            public string? DelayLVNote {  get; set; }
            public string? Flag { get; set; }
        }

        [Keyless]
        public class SavedYesNo
        {
            public string? SavedYN { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_DtRange
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? DtRange { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_PresMed
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? MstId { get; set; }
        }

        [Keyless]
        public class SortDr_PrecriptionViewer
        {
            public string? LoginId { get; set; }
            public string? SortType { get; set; } = "";
        }

        [Keyless]
        public class Resp_Dr_PrecriptionViewer
        {
            public Int32? SRNo { get; set; }
            public Int32? TokenNo { get; set; }
            public string? Priority { get; set; }
            public string? RxType { get; set; }
            public string? PatientName { get; set; }
            public string? RxStatus { get; set; }
            public string? UHID { get; set; }
            public string? IPD { get; set; }
            public string? Ward { get; set; }
            public string? Bed { get; set; }
            public string? LastUser { get; set; }
            public string? Timeofsavingbill { get; set; }
            public Int32? MstId { get; set; }
            public Int32? AdmissionId { get; set; }
            public string? IndoorRecordType { get; set; }
            public string? IsPharmaUsr { get; set; }
            public Int32? stsrt { get; set; }
            public string? PrintStatus { get; set; }
            public string? PrintUserName { get; set; }
            public string? PrintDateTime { get; set; }
            public string? RxCtgr { get; set; }
            public string? Doctor { get; set; }
            public string? MOP { get; set; }
            public string? dte { get; set; }
            public string? Intercom { get; set; }
        }

        [Keyless]
        public class Resp_Dr_PrecriptionMedicines
        {
            public Int64? Sr { get; set; }
            public string? Form_Brand { get; set; }
            public string? GenericName { get; set; }
            public string? rmk { get; set; }
            public string? instruction_typ { get; set; }
            public string? Freq { get; set; }
            public Int32? qty { get; set; }
            public string? medicine_type { get; set; }
            public string? pkg { get; set; }
        }

        [Keyless]
        public class Wards
        {
            public string? IsValidToken { get; set; }
            public int? WardId { get; set; }
            public string? WardName { get; set; }
        }

        [Keyless]
        public class Floors
        {
            public string? IsValidToken { get; set; }
            public int? FloorId { get; set; }
            public string? FloorName { get; set; }
        }

        [Keyless]
        public class Beds
        {
            public int? BedId { get; set; }
            public string? BedName { get; set; }
        }

        [Keyless]
        public class GetPharmaDashboardFilters
        {
            public List<Wards>? Wards { get; set; }
            public List<Floors>? Floors { get; set; }
            public List<Beds>? Beds { get; set; }
        }

        [Keyless]
        public class FilteredPharmaPrecriptionData
        {
            public string? LoginId { get; set; }
            public string? PrefixText { get; set; } = "";
            public List<string>? Wards { get; set; } 
            public List<string>? Floors { get; set; }
            public List<string>? Beds { get; set; }
        }

        [Keyless]
        public class DoctorNotification
        {
            public Int64? Id { get; set; }
            public string? MessageTitle { get; set; }
            public string? Message { get; set; }
            public string? Status { get; set; }
            public DateTime? SentDate { get; set; }
        }

        [Keyless]
        public class LoginAs_AdmMob_UsrNm
        {
            public string? AdminMobileNo { get; set; }
            public string? UserName { get; set; }
        }

        [Keyless]
        public class Resp_LoginAs_Creds
        {
            public string? LoginId { get; set; }
            public string? TokenNo { get; set; }
            public string? EmpId { get; set; }
        }
    }
}
