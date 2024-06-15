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
                string sqlStr = "select dbo.IsTokenValid('" + TokenNo + "', '" + LoginId + "') ";
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
                string sqlStr = "exec dbo.Save_Token_UserCreds_ReturnToken @p_Mobile_No = '" + model.MobileNo + "', " +
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

        public async Task<IEnumerable<CommonProcOutputFields.MonthYr_EmpInfo>> GetMonthYr_EmpInfo()
        {
            try
            {
                string sqlStr = "exec [webpacepayroll].dbo.GetQuery 'Edit_Att_Mnth_yr', '', '', '' ";
                var DashboardData = await AppDbContextAdm.MonthYr_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<MonthYr_EmpInfo>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(string tokenNo, MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string sqlStr = "exec dbo.GetEmpMisPunchDtl_Summ @p_TokenNo = '"+ tokenNo + "', " +
                                "@p_EmpId = '"+ mispunchDtl_EmpInfo.EmpId + "', @p_MonYr = '"+ mispunchDtl_EmpInfo.MonthYr +"' ";
                var DashboardData = await AppDbContextAdm.Resp_MispunchDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_MispunchDtl_EmpInfo>)Enumerable.Empty<string>();
        }

    }
}
