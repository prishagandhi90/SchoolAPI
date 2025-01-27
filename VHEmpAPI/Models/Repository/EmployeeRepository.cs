using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Data;
using System.Net;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public AppDbContext AppDbContextAdm { get; }
        private readonly IDBMethods _dbMethods;

        public EmployeeRepository(AppDbContext appDbContext, IDBMethods dbm)
        {
            AppDbContextAdm = appDbContext;
            _dbMethods = dbm ?? throw new ArgumentNullException(nameof(dbm));
        }

        public async Task<IEnumerable<CommonProcOutputFields.TokenData>> ValidateMobile_Pass(MobileCreds mobileCreds)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_Validate_Emp_Mobile_Pass @p_Mobile = '" + mobileCreds.MobileNo + "', @p_Password = '" + mobileCreds.Password + "' ";
                var IsValidData = await AppDbContextAdm.TokenData.FromSqlRaw(sqlStr).ToListAsync();
                return IsValidData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<TokenData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.IsValidToken>> IsTokenValid(string TokenNo, string LoginId)
        {
            try
            {
                string sqlStr = "select * from dbo.EmpApp_IsTokenValid('" + TokenNo + "', '" + LoginId + "') ";
                var GetToken = await AppDbContextAdm.IsValidToken.FromSqlRaw(sqlStr).ToListAsync();
                return GetToken;
            }
            catch (TaskCanceledException ex)
            {

            }
            finally
            {
                AppDbContextAdm.Database.CloseConnection();
            }

            return (IEnumerable<IsValidToken>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<LoginId_TokenData>> Save_Token_UserCreds_and_ReturnToken(MobileCreds model, string TokenNo)
        {
            try
            {
                string sqlStr = "exec dbo.DrApp_Save_Token_UserCreds_ReturnToken @p_Mobile_No = '" + model.MobileNo + "', " +
                                "@p_TokenNo = '" + TokenNo + "', @p_DeviceType = '" + model.DeviceType + "'," +
                                "@p_DeviceName = '" + model.DeviceName + "', @p_OSType = '" + model.OSType + "'," +
                                "@p_DeviceId = '" + model.DeviceToken + "', @p_UserType = 'EMP', " +
                                "@p_FirebaseId = '"+ model.FirebaseId +"' ";
                var GetToken = await AppDbContextAdm.LoginId_TokenData.FromSqlRaw(sqlStr).ToListAsync();
                return GetToken;
            }
            catch (TaskCanceledException ex)
            {

            }
            finally
            {
                AppDbContextAdm.Database.CloseConnection();
            }

            return (IEnumerable<LoginId_TokenData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.DashBoardList>> DisplayDashboardList(string TokenNo, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.Display_Emp_DashboardList @p_TokenNo = '" + TokenNo + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.DashboardList.FromSqlRaw(sqlStr).ToListAsync();
                var DashboardData = await AppDbContextAdm.DashboardList.FromSqlRaw(sqlStr).ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {
                return (IEnumerable<DashBoardList>)Enumerable.Empty<string>();
            }
            return (IEnumerable<DashBoardList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Ddl_Value_Nm>> GetMonthYr_EmpInfo()
        {
            try
            {
                //string sqlStr = "exec [webpacepayroll].dbo.GetQuery 'Edit_Att_Mnth_yr', '', '', '' ";
                string sqlStr = "exec dbo.GetMonthYear_EmpInfo ";
                var DashboardData = await AppDbContextAdm.Ddl_Value_Nm.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Ddl_Value_Nm>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string sqlStr = "exec dbo.GetEmpMisPunchDtl_Summ @p_EmpId = '" + EmpId + "', " +
                                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                var DashboardData = await AppDbContextAdm.Resp_MispunchDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_MispunchDtl_EmpInfo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_AttDtl_EmpInfo>> GetEmpAttendanceDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string sqlStr = "exec dbo.GetEmpAttDtl_Detail @p_EmpId = '" + EmpId + "', " +
                                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                var DashboardData = await AppDbContextAdm.Resp_AttDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_AttDtl_EmpInfo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_AttSumm_EmpInfo>> GetEmpAttDtl_Summ(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetEmpAttDtl_Summ @p_EmpId = '" + EmpId + "', " +
                                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                var DashboardData = await AppDbContextAdm.Resp_AttSumm_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_AttSumm_EmpInfo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(string EmpId, LoginIdNum loginIdNum)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetEmpSumm_DashData @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + loginIdNum.LoginId + "' ";
                var DashboardData = await AppDbContextAdm.EmpSummary_Dashboard.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<ret_EmpSummary_Dashboard>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.OutSingleString>> GetLeaveDays(string EmpId, GetLeaveDays getLeaveDays)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveDays @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + getLeaveDays.LoginId + "', " +
                                "@p_ToDt = '" + getLeaveDays.LeaveDate + "', " +
                                "@p_LeaveType = '" + getLeaveDays.LeaveType + "' ";
                var DashboardData = await AppDbContextAdm.OutSingleString.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<OutSingleString>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> GetLeaveNames(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveNames @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_Value_Name.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_value_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_name>> GetLeaveReason(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveReason @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_Name.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_LvDelayReason>> GetLeaveDelayReason(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveDelayReason @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_LvDelayReason.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_LvDelayReason>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> EmpApp_GetLeaveRelieverNm(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveRelieverNm @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_id_name.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_id_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_LvEntryList>> EmpApp_GetLeaveEntryList(string EmpId, string LoginId, string Flag)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveEntryList @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "', @p_Flag = '" + Flag + "' ";
                var DashboardData = await AppDbContextAdm.Resp_LvEntryList.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_LvEntryList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_HeaderEntryList>> EmpApp_GetHeaderList(string EmpId, string LoginId, string Flag)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetHeaderList @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "', @p_Flag = '" + Flag + "' ";
                var DashboardData = await AppDbContextAdm.Resp_HeaderEntryList.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_HeaderEntryList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_SaveLeaveEntryList(string EmpId, SaveLeaveEntry saveLeaveEntry)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_SaveLeaveEntry @p_EmpId = '" + EmpId + "', @p_LoginId = '" + saveLeaveEntry.LoginId + "', " +
                                "@p_entrytype = '" + saveLeaveEntry.EntryType + "', @p_leaveshortname = '" + saveLeaveEntry.LeaveShortName + "', " +
                                "@p_leavefullname = '" + saveLeaveEntry.LeaveFullName + "', " +
                                "@p_fromdate = '" + Convert.ToDateTime(saveLeaveEntry.FromDate).ToString("MM/dd/yyyy hh:mm:ss tt") + "', " +
                                "@p_todate = '" + Convert.ToDateTime(saveLeaveEntry.ToDate).ToString("MM/dd/yyyy hh:mm:ss tt") + "', " +
                                "@p_reason = '" + saveLeaveEntry.Reason + "', @p_note = '" + saveLeaveEntry.Note + "', " +
                                "@p_leavedays = '" + saveLeaveEntry.LeaveDays + "', @p_overtimeminutes = '" + saveLeaveEntry.OverTimeMinutes + "', " +
                                "@p_usr_nm = '" + saveLeaveEntry.Usr_Nm + "', @p_reliever_empcode = '" + saveLeaveEntry.Reliever_Empcode + "', " +
                                "@p_DelayLVNote = '" + saveLeaveEntry.DelayLVNote + "', @p_Flag = '" + saveLeaveEntry.Flag + "' ";
                var DashboardData = await AppDbContextAdm.SavedYesNo.FromSqlRaw(sqlStr).ToListAsync();
                return new List<CommonProcOutputFields.SavedYesNo>
                {
                    new CommonProcOutputFields.SavedYesNo { SavedYN = "Y" }
                };
                //return DashboardData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // You can log this or save it in DB

                return new List<CommonProcOutputFields.SavedYesNo>
                {
                    new CommonProcOutputFields.SavedYesNo { SavedYN = "Error: " + ex.Message }
                };
            }
            return (IEnumerable<SavedYesNo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> EmpApp_GetShiftWeekList(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetShiftWeekList @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_Value_Name.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_value_name>)Enumerable.Empty<string>();
        }
        
        public async Task<DataTable> GetEmpShiftReport(string EmpId, string LoginId, string DtRange)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetEmpShiftReport @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_DtRange = '"+ DtRange + "' ";
                //var DashboardData = await AppDbContextAdm.DrDynamicDt.FromSqlRaw(sqlStr).ToListAsync();
                //return DashboardData;

                DataTable objresutl = new DataTable();

                try
                {
                    objresutl = _dbMethods.GetDataTable(sqlStr);

                    return objresutl;
                }
                catch (Exception ex)
                {}
                finally
                {
                }
                return objresutl;
            }
            catch (Exception ex)
            {

            }
            return new DataTable();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionViewer>> EmpApp_GetDrPrescriptionViewer(string EmpId, string LoginId, string PrefixText, List<string> Wards, List<string> Floors, List<string> Beds)
        {
            try
            {
                string Wards_commaSep = string.Join(",", Wards);
                string Floors_commaSep = string.Join(",", Floors);
                string Beds_commaSep = string.Join(",", Beds);
                string sqlStr = "exec dbo.EmpApp_GetDrPrescriptionViewer @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', " +
                                "@p_PrefixText = '" + PrefixText + "', @p_Wards = '" + Wards_commaSep + "', " +
                                "@p_Floors = '" + Floors_commaSep + "', @p_Beds = '" + Beds_commaSep + "' ";
                var Dr_PrecViewerData = await AppDbContextAdm.Resp_Dr_PrecriptionViewer.FromSqlRaw(sqlStr).ToListAsync();
                return Dr_PrecViewerData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_Dr_PrecriptionViewer>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionViewer>> SortDr_PrecriptionViewer(string EmpId, string LoginId, string SortType)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_SortDrPrescriptionViewer @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', " +
                                "@p_SortType = '" + SortType + "' ";
                var DashboardData = await AppDbContextAdm.Resp_Dr_PrecriptionViewer.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_Dr_PrecriptionViewer>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_Dr_PrecriptionMedicines>> GetDrPrescriptionMedicines(string EmpId, string LoginId, string MstId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetDrPrescriptionMedicines @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "', @p_mst_id = '" + MstId + "' ";
                var Dr_PrecViewerMed = await AppDbContextAdm.Resp_Dr_PrecriptionMedicines.FromSqlRaw(sqlStr).ToListAsync();
                return Dr_PrecViewerMed;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_Dr_PrecriptionMedicines>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Wards>> GetWards(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.DrApp_GetWards @p_DrId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Wards.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Wards>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Floors>> GetFloors(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.DrApp_GetFloors @p_DrId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Floors.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Floors>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Beds>> GetBeds(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.DrApp_GetBeds @p_DrId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Beds.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Beds>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<DoctorNotification>> GetDrNotifications(string loginId, string EmpId)
        {
            try
            {
                string sqlStr = "exec dbo.GetDrNotifications @p_LoginId = '" + loginId + "', @p_DrId = '" + EmpId + "' ";
                var DrNotification = await AppDbContextAdm.DrNotification.FromSqlRaw(sqlStr).ToListAsync();
                return DrNotification;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<DoctorNotification>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_LV_OT_RolesList>> EmpApp_Get_LV_OT_RolesList(string EmpId, string LoginId, string RoleNm, string Flag)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_Get_LV_OT_Roles @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', " +
                                "@p_Role = '"+ RoleNm +"', @p_Flag = '" + Flag + "' ";
                var Lv_Ot_Roles_Lst = await AppDbContextAdm.Resp_LV_OT_RolesList.FromSqlRaw(sqlStr).ToListAsync();
                return Lv_Ot_Roles_Lst;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_LV_OT_RolesList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_LV_OT_RolesRights>> EmpApp_Get_LV_OT_Role_Rights(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_Get_LV_OT_Role_Rights @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "' ";
                var Lv_Ot_Roles_Lst = await AppDbContextAdm.Resp_LV_OT_RolesRights.FromSqlRaw(sqlStr).ToListAsync();
                return Lv_Ot_Roles_Lst;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_LV_OT_RolesRights>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_Upd_LV_OT_Entry(string EmpId, Upd_Lv_OT_entry upd_Lv_OT_entry)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_upd_leave_detail @p_LoginId = '" + upd_Lv_OT_entry.LoginId + "', @p_EmpId = '" + EmpId + "', " +
                                "@p_Flag = '" + upd_Lv_OT_entry.Flag + "', @p_leavedetailid = '" + upd_Lv_OT_entry.LeaveDetailId + "', " +
                                "@p_action = '" + upd_Lv_OT_entry.Action + "', @p_reason = '" + upd_Lv_OT_entry.Reason + "', " +
                                "@p_usr_nm = '" + upd_Lv_OT_entry.UserName + "', @p_note = '" + upd_Lv_OT_entry.Note + "' ";
                var savedYN = await AppDbContextAdm.SavedYesNo.FromSqlRaw(sqlStr).ToListAsync();
                return new List<CommonProcOutputFields.SavedYesNo>
                {
                    new CommonProcOutputFields.SavedYesNo { SavedYN = "Y" }
                };
                //return DashboardData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // You can log this or save it in DB

                return new List<CommonProcOutputFields.SavedYesNo>
                {
                    new CommonProcOutputFields.SavedYesNo { SavedYN = "Error: " + ex.Message }
                };
            }
            return (IEnumerable<SavedYesNo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_name>> GetLeaveRejectReason(string EmpId, string LoginId)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetLeaveRejectReason @p_EmpId = '" + EmpId + "', " +
                                "@p_LoginId = '" + LoginId + "' ";
                var DashboardData = await AppDbContextAdm.Resp_Name.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_name>)Enumerable.Empty<string>();
        }

    }

}
