using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public interface IEmployeeRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task CommitTransaction(IDbContextTransaction transaction);
        Task RollbackTransaction(IDbContextTransaction transaction);
        Task DisposeTransaction(IDbContextTransaction transaction);
        Task<IEnumerable<TokenData>> ValidateMobile_Pass(MobileCreds mobileCreds);
        Task<IEnumerable<CommonProcOutputFields.IsValidToken>> IsTokenValid(string TokenNo, string LoginId);
        Task<IEnumerable<LoginId_TokenData>> Save_Token_UserCreds_and_ReturnToken(MobileCreds model, string TokenNo);
        Task<IEnumerable<CommonProcOutputFields.DashBoardList>> DisplayDashboardList(string TokenNo, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Ddl_Value_Nm>> GetMonthYr_EmpInfo();
        Task<IEnumerable<CommonProcOutputFields.Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.Resp_AttDtl_EmpInfo>> GetEmpAttendanceDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.Resp_AttSumm_EmpInfo>> GetEmpAttDtl_Summ(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(string EmpId, LoginIdNum loginIdNum);
        Task<IEnumerable<CommonProcOutputFields.OutSingleString>> GetLeaveDays(string EmpId, GetLeaveDays getLeaveDays);
        Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> GetLeaveNames(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_name>> GetLeaveReason(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LvDelayReason>> GetLeaveDelayReason(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> EmpApp_GetLeaveRelieverNm(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LvEntryList>> EmpApp_GetLeaveEntryList(string EmpId, string LoginId, string Flag);
        Task<IEnumerable<CommonProcOutputFields.Resp_HeaderEntryList>> EmpApp_GetHeaderList(string EmpId, string LoginId, string Flag);
        Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_SaveLeaveEntryList(string EmpId, SaveLeaveEntry saveLeaveEntry);
        Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> EmpApp_GetShiftWeekList(string EmpId, string LoginId);
        Task<DataTable> GetEmpShiftReport(string EmpId, string LoginId, string DtRange);
        Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionViewer>> EmpApp_GetDrPrescriptionViewer(string EmpId, string LoginId, string PrefixText, List<string> Wards, List<string> Floors, List<string> Beds);
        Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionViewer>> SortDr_PrecriptionViewer(string EmpId, string LoginId, string SortType);
        Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionMedicines>> GetDrPrescriptionMedicines(string EmpId, string LoginId, string MstId);
        Task<IEnumerable<Organizations>> GetOrganizations(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Floors>> GetFloors(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Wards>> GetWards(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Beds>> GetBeds(string EmpId, string LoginId);
        Task<IEnumerable<DoctorNotification>> GetDrNotifications(string loginId, string EmpId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LV_OT_RolesList>> EmpApp_Get_LV_OT_RolesList(string EmpId, string LoginId, string RoleNm, string Flag);
        //Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_Upd_LV_OT_Entry(string EmpId, Upd_Lv_OT_entry upd_Lv_OT_entry, IDbContextTransaction transaction);
        Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_Upd_LV_OT_Entry(string EmpId, Upd_Lv_OT_entry upd_Lv_OT_entry);
        Task<IEnumerable<CommonProcOutputFields.Resp_name>> GetLeaveRejectReason(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LV_OT_RolesRights>> EmpApp_Get_LV_OT_Role_Rights(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.ModuleScreenRights>> GetModuleRights(string EmpId, string LoginId, string ModuleName); 
        Task<IEnumerable<CommonProcOutputFields.ModuleScreenRights>> GetEmpAppScreenRights(string EmpId, string LoginId, string ModuleName);

        //Task<IEnumerable<CommonProcOutputFields.PatientList>> SearchPatientList(string TokenNo, string prefixText);
        Task<IEnumerable<CommonProcOutputFields.PatientList>> GetFilteredPatientData(string DrId, string LoginId, string PrefixText, List<string> Orgs, List<string> Floors, List<string> Wards);
        Task<IEnumerable<CommonProcOutputFields.PatientList>> SortDeptPatientList(string EmpId, string LoginId, string SortType);
        Task<DataSet> GetPatientLabReports(string DrId, string IpdNo, string UHID);
        Task<DataSet> GetPatientSummaryLabData(string DrId, string IpdNo, string UHID);
        Task<IEnumerable<EMPNotificationList>> GetEMPNotificationsList(string loginId, string EmpId, int? days, string? tag, string? fromDate, string? toDate);
        Task<IEnumerable<IsValidData>> UpdateNotification_Read(string loginId, string NotificationId);
        Task<IEnumerable<IsValidData>> Save_DoctorVoiceNote(VoiceNoteFields voiceNoteFields);
        Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> EmpApp_GetExternalLabName(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_name>> EmpApp_InvReq_GetServiceGrp(string EmpId, string LoginId, string SearchText);
        Task<IEnumerable<CommonProcOutputFields.Resp_txt_name_value>> EmpApp_InvReq_SearchService(string EmpId, string LoginId, string SearchText);
        Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_Get_Query>> EmpApp_InvReq_Get_Query(InvReq_Get_Query invReq_Get_Query);
        Task<RequestSheetIPD> SaveRequestSheetIPD_Dapper(RequestSheetIPD model);
        Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> SaveRequestSheetDetailsIPD_Dapper(List<RequestSheetDetailsIPD> list);
        Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_Get_HistData>> EmpApp_InvReq_Get_HIstoryData(InvReq_Get_Query invReq_Get_Query);
        Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_SelReq_HistDetail>> EmpApp_InvReq_SelReq_HistoryDetail(InvReq_Get_Query invReq_Get_Query);
        Task<IEnumerable<CommonProcOutputFields.Resp_id_int_name>> EmpApp_InvReq_SearchDrName(string EmpId, string LoginId, string SearchText, string Srv);
        Task<IEnumerable<CommonProcOutputFields.RespWebCreds>> Validate_Web_Creds(WebEmpMobileCreds mobileCreds);
        Task EmpApp_Delete_InvReq_Detail(InvReq_Del_ReqDtl invReq_Del_ReqDtl);
    }
}
