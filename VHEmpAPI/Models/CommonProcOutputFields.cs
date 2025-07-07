using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VHEmpAPI.Shared.CommonProcOutputFields;

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
        public class LoginId_EmpId_ModuleNm
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? ModuleName { get; set; }
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
            public string? IsPharmacyUser { get; set; }
            public int? NotificationCount { get; set; }
            public string? IsPasswordSet { get; set; }
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
        public class Resp_id_int_name
        {
            public int? Id { get; set; }
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
        public class Resp_MedSheet_Del
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public int mstId { get; set; }
            public int dtlId { get; set; }
            public string UserName { get; set; }
        }

        

        [Keyless]
        public class LoginId_EmpId_Lv_OT_Flag
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? Role { get; set; }
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
            public string? LeaveDivision { get; set; }
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
            public string? Org { get; set; }
            public string? FromEmergency { get; set; }
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
            public string? Rack { get; set; }
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

        [Keyless]
        public class Resp_LV_OT_RolesList
        {
            public int? LeaveId { get; set; }
            public string? TypeValue { get; set; }
            public string? TypeName { get; set; }
            public string? LeaveShortName { get; set; }
            public string? LeaveFullName { get; set; }
            public string? EmployeeCodeValue { get; set; }
            public string? EmployeeCodeName { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            public decimal? OverTimeMinutes { get; set; }
            public string? LeaveDays { get; set; }
            public string? Reason { get; set; }
            public string? Note { get; set; }
            public string? InchargeAction { get; set; }
            public string? HodAction { get; set; }
            public string? DeptInc { get; set; }
            public string? DeptHOD { get; set; }
            public string? InchargeNote { get; set; }
            public string? HoDNote { get; set; }
            public string? HRNote { get; set; }
            public string? EnterDate { get; set; }
            public string? RelieverEmpName { get; set; }
            public string? LateReasonName { get; set; }
            public string? OTHours { get; set; }
            public string? EmpTel { get; set; }
            public string? PunchTime { get; set; }
            public string? ShiftTime { get; set; }
            //public string? DefaultRole { get; set; }
            //public string? InchargeYN { get; set; }
            //public string? HODYN { get; set; }
            //public string? HRYN { get; set; }
        }

        [Keyless]
        public class LoginId_Lst_LVOT
        {
            public string? LoginId { get; set; }
            public List<Resp_LV_OT_RolesList>? List_Resp_LV_OT_Roles { get; set; }
        }

        [Keyless]
        public class Resp_LV_OT_RolesRights
        {
            public string? DefaultRole { get; set; }
            public string? InchargeYN { get; set; }
            public string? HODYN { get; set; }
            public string? HRYN { get; set; }
        }

        [Keyless]
        public class Upd_Lv_OT_entry
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? Flag { get; set; }
            public string? LeaveDetailId { get; set; }
            public string? Action { get; set; }
            public string? Reason { get; set; }
            public string? UserName { get; set; }
            public string? Note { get; set; }

        }

        [Keyless]
        public class ModuleScreenRights
        {
            public int? Id { get; set; }
            public string? ModuleName { get; set; }
            public string? ScreenName { get; set; }
            public int? ModuleSeq { get; set; }
            public int? ScreenSeq { get; set; }
            public string? ImagePath { get; set; } // Nullable, agar DB me ho
            public string? RightsYN { get; set; }  // "Y" or "N" type
            public string? InactiveYN { get; set; } // "Y" or "N" type
        }

        [Keyless]
        public class FilteredPatientData
        {
            public string? LoginId { get; set; }
            public string? PrefixText { get; set; } = "";
            public List<string>? Orgs { get; set; }
            public List<string>? Floors { get; set; }
            public List<string>? Wards { get; set; }
        }

        [Keyless]
        public class SortDrPatientData
        {
            public string? LoginId { get; set; }
            public string? SortType { get; set; } = "";
        }

        [Keyless]
        public class GetDashboardFilters
        {
            public List<Organizations>? Orgs { get; set; }
            public List<Floors>? Floors { get; set; }
            public List<Wards>? Wards { get; set; }
        }

        [Keyless]
        public class Organizations
        {
            public string? IsValidToken { get; set; }
            public string? Organization { get; set; }
        }

        [Keyless]
        public class OrgDropdown
        {
            public int? OrgId { get; set; }
            public string? Organization { get; set; }
        }

        [Keyless]
        public class PatientList
        {
            public string? IsValidToken { get; set; }
            public string? PatientCategory { get; set; }
            public string? UHID { get; set; }
            public string? IPDNo { get; set; }
            public string? PatientName { get; set; }
            public string? BedNo { get; set; }
            public string? Ward { get; set; }
            public string? Floor { get; set; }
            public string? DOA { get; set; }
            public string? AdmType { get; set; }
            public string? TotalDays { get; set; }
            public string? ReferredDr { get; set; }
            public string? MobileNo { get; set; }
        }

        [Keyless]
        public class DrRadiologyData
        {
            public string? LoginId { get; set; }
            public string? IpdNo { get; set; }
            public string? UHID { get; set; }
        }

        [Keyless]
        public class EMPNotificationList
        {
            public Int64? Id { get; set; }
            public string? AppName { get; set; }
            public string? NotificationType { get; set; }
            public string? MessageType { get; set; }
            public string? Sender { get; set; }
            public string? MessageTitle { get; set; }
            public string? Message { get; set; }
            public string? FileYN { get; set; }
            public string? CreatedDate { get; set; }
            public string? ScheduleDate { get; set; }
            public string? InactiveDate { get; set; }
            public string? SendToAll { get; set; }
            public string? CreatedBy { get; set; }
            public string? BoldYN { get; set; }
        }

        [Keyless]
        public class LoginId_FileIndex
        {
            public string? LoginId { get; set; }
            public string? NotificationId { get; set; }
            public Int32? Index { get; set; }
        }

        [Keyless]
        public class LoginId__AdmPatients_Days_Tag
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public Int32? Days { get; set; }
            public string? Tag { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
        }

        public class IssueReportDto
        {
            public string? ScreenName { get; set; }
            public string? ErrorMessage { get; set; }
            public string? LoginID { get; set; }
            public string? TokenNo { get; set; }
            public string? EmpID { get; set; }
            public string? DeviceInfo { get; set; }
        }

        [Keyless]
        public class ForceUpdateYN
        {
            public string? VersionName { get; set; }
            public string? ForceUpdYN { get; set; }
        }

        [Keyless]
        public class VoiceNoteFields
        {
            public string? LoginId { get; set; }
            public string? EmpID { get; set; }
            public string? UHID { get; set; }
            public string? IPDNo { get; set; }
            public string? PatientName { get; set; }
            public string? VoiceFileName { get; set; }
            public string? DoctorName { get; set; }
            public string? CreatedUser { get; set; }
            public string? TranslatedText { get; set; }
        }

        [Keyless]
        public class Resp_txt_name_value
        {
            public string? Txt { get; set; }
            public string? Name { get; set; }
            public string? Value { get; set; }
        }

        [Keyless]
        public class InvReq_Get_Query
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? TYPE { get; set; }
            public string? Top10_40 { get; set; }
            public string? IPD { get; set; }
            public string? SrchService { get; set; }
            public string? InvType { get; set; }
            public string? SrvGrp { get; set; }
            public string? ExtLabNm { get; set; }
            public string? Val7 { get; set; }
        }

        [Keyless]
        public class Resp_InvReq_Get_Query
        {
            public string? Name { get; set; }
            public int? id { get; set; }
            public int? cnt { get; set; }
            public string? Value { get; set; }
            public string? Txt { get; set; }
            public string? sup_name { get; set; }
        }

        [Keyless]
        public class SaveInvReqIPD
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? Action { get; set; }
            public string? IPDNo { get; set; }
            public string? UHID { get; set; }
            public DateTime? DT { get; set; }
            public string? ReqTyp { get; set; }
            public string? Rmk { get; set; }
            public string? UsrNm { get; set; }
            public string? Emerg { get; set; }
            public string? ClinicRmk { get; set; }
            public string? InvPriority { get; set; }
        }

        public class RequestSheetIPD
        {
            public string? UHIDNo { get; set; }
            public string? IPDNo { get; set; }
            public string? ReqType { get; set; }
            public string? Remark { get; set; }
            public string? Username { get; set; }
            public DateTime? Dt { get; set; }
            public string? Action { get; set; }
            public string? IsEmergency { get; set; }
            public string? ClinicRemark { get; set; }
            public string? InvestPriority { get; set; }

            public int? ReqId { get; set; }
            public int? Dr_Inst_Id { get; set; }
            public int? Bill_Detail_Id { get; set; }

            public List<RequestSheetDetailsIPD>? LabDetail { get; set; }
            public List<RequestSheetDetailsIPD>? RadioDetail { get; set; }
            public List<RequestSheetDetailsIPD>? OtherDetail { get; set; }
            public DataRowState RowState { get; set; } = DataRowState.Unchanged;
        }

        public class RequestSheetDetailsIPD
        {
            public int? MReqId { get; set; }
            public string? ServiceName { get; set; }
            public int? ServiceId { get; set; }
            public string? Username { get; set; }
            public string? InvSrc { get; set; }
            public string? ReqTyp { get; set; }
            public string? UHIDNo { get; set; }
            public string? IPDNo { get; set; }
            public int? DrID { get; set; }
            public string? DrNAME { get; set; }

            public int? Dr_Inst_Id { get; set; }
            public int? Bill_Detail_Id { get; set; }
            public DataRowState RowState { get; set; } = DataRowState.Unchanged;
            public string Action { get; set; }
        }

        [Keyless]
        public class Resp_InvReq_Get_HistData
        {
            public int? RequisitionNo { get; set; }
            public string? Date { get; set; }
            public string? InvestigationType { get; set; }
            public string? InvestigationPriority { get; set; }
            public string? EntryDate { get; set; }
            public string? User { get; set; }
        }

        [Keyless]
        public class Resp_InvReq_SelReq_HistDetail
        {
            public int? id { get; set; }
            public int? RequestID { get; set; }
            public string? ServiceName { get; set; }
            public string? ReqTyp { get; set; }
            public string? ServiceGroup { get; set; }
            public string? InvestigationSource { get; set; }
            public int? testid { get; set; }
            public string? Status { get; set; }
            public int? labflg { get; set; }
        }

        [Keyless]
        public class InvReq_Del_ReqDtl
        {
            public string? LoginId { get; set; }
            public Int64? req_sht_dtl_id { get; set; }
            public string? UserName { get; set; }
            public string? FormName { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_SrchDr
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? SearchText { get; set; }
            public string? Srv { get; set; }
        }

        [Keyless]
        public class WebEmpMobileCreds
        {
            public string? LoginId { get; set; }
            public string? MobileNo { get; set; }
            public string? Password { get; set; }
            public string? FormScreen { get; set; }
        }

        [Keyless]
        public class RespWebCreds
        {
            public string? IsValidCreds { get; set; }
            public string? Message { get; set; }
            public string? WebEmpName { get; set; }
            public string? WebEmpId { get; set; }
        }

        [Keyless]
        public class LoginId_FirebaseId
        {
            public string? LoginId { get; set; }
            public string? FirebaseId { get; set; }
        }

        [Keyless]
        public class ListOfItem
        {
            public long? Id { get; set; }
            public string? Name { get; set; }
            public string? Value { get; set; }
            public int? Sort { get; set; }
            public string? Txt { get; set; }
            public long? ParentId { get; set; }
            public string? sup_name { get; set; }
            public DateTime? DateValue { get; set; }
        }

        [Keyless]
        public class DrTreatmentMaster
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? IpdNo { get; set; }
            public string? TreatTyp { get; set; }
            public string? UserName { get; set; }
        }

        [Keyless]
        public class Resp_DRTreatMaster
        {
            public int? AdmissionId { get; set; }
            public int? DRMstId { get; set; }
            public string? IRT { get; set; }
            public DateTime? Date { get; set; }
            public string? Remark { get; set; }
            public int? SRNo { get; set; }
            public DateTime? SysDate { get; set; }
            public string? UserName { get; set; }
            public string? TerminalName { get; set; }
            public string? SpecialOrder { get; set; }
            public string? ProvisionalDiagnosis { get; set; }
            public string? Weight { get; set; }
            public string? TemplateName { get; set; }
            public string? PrescriptionType { get; set; }
            public string? Precedence { get; set; }
            public string? StatusTyp { get; set; }
            public string? IsAlw { get; set; }
            public string? Age { get; set; }
            public string? PatientName { get; set; }
            public string? CommunicationNumber { get; set; }
            public string? ConsDrName { get; set; }
            public int? ConsDrId { get; set; }
            public DateTime? DOB { get; set; }
            public string? FrmEmerg { get; set; }
            public DataRowState? RowState { get; set; } = DataRowState.Unchanged;
            public List<Resp_DRTreatDetail?> Detail { get; set; }
            [NotMapped]
            public ListOfItem? IndoorRecordType { get; set; }
            public string? Action { get; set; }
            [NotMapped]
            public ListOfItem? ConsDr { get; set; }
            public bool? IsValid { get; set; } = true;
            public int? IudId { get; set; }
            public string? GridName { get; set; } = "DrTMaster";
            public Guid? GUID { get; set; } = Guid.NewGuid();
            public int? TmplId { get; set; }
            public string? TmplName { get; set; }
            public void FormateData()
            {
                IndoorRecordType = new ListOfItem
                {
                    Name = IRT
                };
                ConsDr = new ListOfItem
                {
                    Name = ConsDrName,
                    Id = ConsDrId
                };
            }
        }

        [Keyless]
        public class Resp_DRTreatDetail
        {
            public int? DRDtlId { get; set; }
            public int? DRMstId { get; set; }
            [NotMapped]
            public ListOfItem? MedicineType { get; set; }
            public int? Days { get; set; }
            [NotMapped]
            public ListOfItem? ItemName { get; set; }
            public string? ItemNameMnl { get; set; }
            public int? Qty { get; set; }
            public string? Dose { get; set; }
            [NotMapped]
            public ListOfItem? Route { get; set; }
            [NotMapped]
            public ListOfItem? Frequency1 { get; set; }
            [NotMapped]
            public ListOfItem? Frequency2 { get; set; }
            [NotMapped]
            public ListOfItem? Frequency3 { get; set; }
            [NotMapped]
            public ListOfItem? Frequency4 { get; set; }
            public string? Remark { get; set; }
            public DateTime? Dose1 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy1 { get; set; }
            public DateTime? Dose2 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy2 { get; set; }
            public DateTime? Dose3 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy3 { get; set; }
            public DateTime? Dose4 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy4 { get; set; }
            public DateTime? Dose5 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy5 { get; set; }
            public DateTime? Dose6 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy6 { get; set; }
            public DateTime? Dose7 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy7 { get; set; }
            public DateTime? Dose8 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy8 { get; set; }
            public DateTime? Dose9 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy9 { get; set; }
            public DateTime? Dose10 { get; set; }
            [NotMapped]
            public ListOfItem? DoseGivenBy10 { get; set; }
            public string? UserName { get; set; }
            public DateTime? SysDate { get; set; }
            public string? TerminalName { get; set; }
            [NotMapped]
            public ListOfItem? Instruction_typ { get; set; }
            public string? Action { get; set; }
            public DateTime? StopTime { get; set; }
            public string? FlowRate { get; set; }
            public bool? IsValid { get; set; } = true;
            public int? IudId { get; set; }
            public DataRowState? RowState { get; set; } = DataRowState.Unchanged;
            public Guid? GUID { get; set; } = Guid.NewGuid();
            public string? GridName { get; set; } = "DrTDetail";
            public string? Freq1 { get; set; }
            public string? Freq2 { get; set; }
            public string? Freq3 { get; set; }
            public string? Freq4 { get; set; }
            public string? RouteName { get; set; }
            public string? MedicationName { get; set; }
            public string? DGivenBy1 { get; set; }
            public string? DGivenBy2 { get; set; }
            public string? DGivenBy3 { get; set; }
            public string? DGivenBy4 { get; set; }
            public string? DGivenBy5 { get; set; }
            public string? DGivenBy6 { get; set; }
            public string? DGivenBy7 { get; set; }
            public string? DGivenBy8 { get; set; }
            public string? DGivenBy9 { get; set; }
            public string? DGivenBy10 { get; set; }
            public string? ItemTxt { get; set; }
            public string? Item { get; set; }
            public string? InstType { get; set; }
            public string? FlowRt { get; set; }

            public void FormateData()
            {
                Frequency1 = new ListOfItem
                {
                    Name = Freq1
                };
                Frequency2 = new ListOfItem
                {
                    Name = Freq2
                };
                Frequency3 = new ListOfItem
                {
                    Name = Freq3
                };
                Frequency4 = new ListOfItem
                {
                    Name = Freq4
                };
                MedicineType = new ListOfItem
                {
                    Name = MedicationName
                };
                Route = new ListOfItem
                {
                    Name = RouteName
                };
                DoseGivenBy1 = new ListOfItem
                {
                    Name = DGivenBy1
                };
                DoseGivenBy2 = new ListOfItem
                {
                    Name = DGivenBy2
                };
                DoseGivenBy3 = new ListOfItem
                {
                    Name = DGivenBy3
                };
                DoseGivenBy4 = new ListOfItem
                {
                    Name = DGivenBy4
                };
                DoseGivenBy5 = new ListOfItem
                {
                    Name = DGivenBy5
                };
                DoseGivenBy6 = new ListOfItem
                {
                    Name = DGivenBy6
                };
                DoseGivenBy7 = new ListOfItem
                {
                    Name = DGivenBy7
                };
                DoseGivenBy8 = new ListOfItem
                {
                    Name = DGivenBy8
                };
                DoseGivenBy9 = new ListOfItem
                {
                    Name = DGivenBy9
                };
                DoseGivenBy10 = new ListOfItem
                {
                    Name = DGivenBy10
                };
                ItemName = new ListOfItem
                {
                    Name = Item,
                    Txt = ItemTxt
                };
                Instruction_typ = new ListOfItem
                {
                    Name = InstType
                };
                //FlowRate = new ListOfItem
                //{
                //    Name = FlowRt
                //};
            }
        }
    }

    [Keyless]
    public class AdmissionId
    {
        public string? LoginId { get; set; }
        public string? EmpId { get; set; }
        public string? IpdNo { get; set; }
    }

    [Keyless]
    public class Resp_Id
    {
        public int? Id { get; set; }
    }

    [Keyless]
    public class Resp_DieticianChecklist
    {
        public int? Id { get; set; }
        public string? PatientName { get; set; }
        public string? UHIDNo { get; set; }
        public string? IPDNo { get; set; }
        public string? BEDNo { get; set; }
        public string? WardName { get; set; }
        public int? GrpSequence { get; set; }
        public int? Sequence { get; set; }
        public DateTime? DOA { get; set; }
        public string? Doctor { get; set; }
        public int? FloorNo { get; set; }
        public string? Diagnosis { get; set; }
        public string? Remark { get; set; }
        public string? Username { get; set; }
        public DateTime? SysDate { get; set; }
        public string? DietPlan { get; set; }
        public string? RelFood_Remark { get; set; }

    }

    [Keyless]
    public class Resp_WardWiseChecklistCount
    {
        public string? WardName { get; set; }
        public string? ShortWardName { get; set; }
        public int? Grp_Seq { get; set; }
        public int? Seq { get; set; }
        public int? WardCount { get; set; }
    }

    [Keyless]
    public class DieticianChecklist_I
    {
        public string? LoginId { get; set; }
        public string? EmpId { get; set; }
        public string? PrefixText { get; set; } = "";
        public List<string>? Wards { get; set; }
        public List<string>? Floors { get; set; }
        public List<string>? Beds { get; set; }
    }

    [Keyless]
    public class DietChecklistMaster
    {
        public int? Id { get; set; }
        public string? Diagnosis { get; set; }
        public ListOfItem? Diet { get; set; }
        public string? Remark { get; set; }
        public string? RelFood_Remark { get; set; }
        public string? Username { get; set; }

        // Extra properties for UI display only (NotMapped equivalent in API layer)
        public string? PatientName { get; set; }
        public string? UHIDNo { get; set; }
        public string? IPDNo { get; set; }
        public string? BEDNo { get; set; }
        public string? WardName { get; set; }
        public DateTime? DOA { get; set; }
        public string? Doctor { get; set; }
        public string? FloorNo { get; set; }
        public DateTime? SysDate { get; set; }
        public string? DietPlan { get; set; }

        public void FormateData()
        {
            Diet = new ListOfItem
            {
                Name = DietPlan
            };
        }
    }

}
