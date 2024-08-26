using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public AppDbContext AppDbContextAdm { get; }
        public EmployeeRepository(AppDbContext appDbContext)
        {
            AppDbContextAdm = appDbContext;
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
                                "@p_DeviceToken = '" + model.DeviceToken + "', @p_UserType = 'EMP' ";
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
                string sqlStr = "exec dbo.GetEmpMisPunchDtl_Summ @p_EmpId = '"+ EmpId + "', " +
                                "@p_MonYr = '"+ mispunchDtl_EmpInfo.MonthYr +"' ";
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
                                "@p_ToDt = '"+ getLeaveDays.LeaveDate + "', " +
                                "@p_LeaveType = '"+ getLeaveDays.LeaveType +"' ";
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

    }
}
