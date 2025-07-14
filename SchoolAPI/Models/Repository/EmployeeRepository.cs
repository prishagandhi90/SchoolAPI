using Dapper;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using RestSharp;
using System.Data;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Shared;
using static Dapper.SqlMapper;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public AppDbContext AppDbContextAdm { get; }
        private readonly IDBMethods _dbMethods;
        private IDbContextTransaction _transaction;
        private readonly IConfiguration _configuration;

        public EmployeeRepository(AppDbContext appDbContext, IDBMethods dbm, IConfiguration configuration)
        {
            AppDbContextAdm = appDbContext;
            _dbMethods = dbm ?? throw new ArgumentNullException(nameof(dbm));
            _configuration = configuration;
        }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            _transaction = await AppDbContextAdm.Database.BeginTransactionAsync();
            return _transaction;
        }

        // Commit Transaction Method
        public async Task CommitTransaction(IDbContextTransaction transaction)
        {
            try
            {
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle exception while committing
                throw new InvalidOperationException("Error committing transaction: " + ex.Message);
            }
        }

        // Rollback Transaction Method
        public async Task RollbackTransaction(IDbContextTransaction transaction)
        {
            try
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle exception while rolling back
                throw new InvalidOperationException("Error rolling back transaction: " + ex.Message);
            }
        }

        public async Task DisposeTransaction(IDbContextTransaction transaction)
        {
            try
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync(); // Ensure proper disposal
                }
            }
            catch (Exception ex)
            {
                // Handle exception while disposing the transaction
                throw new InvalidOperationException("Error disposing transaction: " + ex.Message);
            }
        }

        #region Login Authentication and Auto Login

        public async Task<IEnumerable<CommonProcOutputFields.TokenData>> ValidateMobile_Pass(MobileCreds mobileCreds)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_Validate_Emp_Mobile_Pass @p_Mobile = '" + mobileCreds.MobileNo + "', @p_Password = '" + mobileCreds.Password + "' ";
                //var IsValidData = await AppDbContextAdm.TokenData.FromSqlRaw(sqlStr).ToListAsync();
                //var IsValidData = await AppDbContextAdm.TokenData
                //                                                .FromSqlRaw("EXEC dbo.EmpApp_Validate_Emp_Mobile_Pass @p_Mobile = {0}, @p_Password = {1}",
                //                                                    mobileCreds.MobileNo,
                //                                                    mobileCreds.Password)
                //                                                .ToListAsync();

                #endregion

                var mobileParam = new SqlParameter("@p_Mobile", mobileCreds.MobileNo ?? (object)DBNull.Value);
                var passwordParam = new SqlParameter("@p_Password", mobileCreds.Password ?? (object)DBNull.Value);

                var IsValidData = await AppDbContextAdm.TokenData
                                            .FromSqlRaw("EXEC dbo.EmpApp_Validate_Emp_Mobile_Pass @p_Mobile, @p_Password",
                                                mobileParam, passwordParam)
                                            .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "select * from dbo.EmpApp_IsTokenValid('" + TokenNo + "', '" + LoginId + "') ";
                //var GetToken = await AppDbContextAdm.IsValidToken.FromSqlRaw(sqlStr).ToListAsync();
                //var GetToken = await AppDbContextAdm.IsValidToken
                //                                                .FromSqlRaw("SELECT * FROM dbo.EmpApp_IsTokenValid({0}, {1})", TokenNo, LoginId)
                //                                                .ToListAsync();

                #endregion

                var tokenParam = new SqlParameter("@TokenNo", TokenNo ?? (object)DBNull.Value);
                var loginParam = new SqlParameter("@LoginId", LoginId ?? (object)DBNull.Value);

                var GetToken = await AppDbContextAdm.IsValidToken
                    .FromSqlRaw("SELECT * FROM dbo.EmpApp_IsTokenValid(@TokenNo, @LoginId)", tokenParam, loginParam)
                    .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "exec dbo.DrApp_Save_Token_UserCreds_ReturnToken @p_Mobile_No = '" + model.MobileNo + "', " +
                //                "@p_TokenNo = '" + TokenNo + "', @p_DeviceType = '" + model.DeviceType + "'," +
                //                "@p_DeviceName = '" + model.DeviceName + "', @p_OSType = '" + model.OSType + "'," +
                //                "@p_DeviceId = '" + model.DeviceToken + "', @p_UserType = 'EMP', " +
                //                "@p_FirebaseId = '" + model.FirebaseId + "' ";
                //var GetToken = await AppDbContextAdm.LoginId_TokenData.FromSqlRaw(sqlStr).ToListAsync();
                //var GetToken = await AppDbContextAdm.LoginId_TokenData
                //                                                    .FromSqlRaw(
                //                                                        @"EXEC dbo.DrApp_Save_Token_UserCreds_ReturnToken 
                //                                                            @p_Mobile_No = {0}, 
                //                                                            @p_TokenNo = {1}, 
                //                                                            @p_DeviceType = {2}, 
                //                                                            @p_DeviceName = {3}, 
                //                                                            @p_OSType = {4}, 
                //                                                            @p_DeviceId = {5}, 
                //                                                            @p_UserType = {6}, 
                //                                                            @p_FirebaseId = {7}",
                //                                                        model.MobileNo,
                //                                                        TokenNo,
                //                                                        model.DeviceType,
                //                                                        model.DeviceName,
                //                                                        model.OSType,
                //                                                        model.DeviceToken,
                //                                                        "EMP", // hardcoded string is okay
                //                                                        model.FirebaseId
                //                                                    )
                //                                                    .ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_Mobile_No", model.MobileNo ?? (object)DBNull.Value),
                                        new SqlParameter("@p_TokenNo", TokenNo ?? (object)DBNull.Value),
                                        new SqlParameter("@p_DeviceType", model.DeviceType ?? (object)DBNull.Value),
                                        new SqlParameter("@p_DeviceName", model.DeviceName ?? (object)DBNull.Value),
                                        new SqlParameter("@p_OSType", model.OSType ?? (object)DBNull.Value),
                                        new SqlParameter("@p_DeviceId", model.DeviceToken ?? (object)DBNull.Value),
                                        new SqlParameter("@p_UserType", "EMP"),
                                        new SqlParameter("@p_FirebaseId", model.FirebaseId ?? (object)DBNull.Value),
                                    };

                var GetToken = await AppDbContextAdm.LoginId_TokenData
                                        .FromSqlRaw("EXEC dbo.DrApp_Save_Token_UserCreds_ReturnToken " +
                                                    "@p_Mobile_No, @p_TokenNo, @p_DeviceType, @p_DeviceName, " +
                                                    "@p_OSType, @p_DeviceId, @p_UserType, @p_FirebaseId", parameters)
                                        .ToListAsync();

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

        public async Task<IEnumerable<CommonProcOutputFields.IsValidData>> UpdateFirebaseId(string LoginId, string FirebaseId)
        {
            try
            {
                #region commented Old Working codes

                //string sqlStr = "exec dbo.Display_Emp_DashboardList @p_TokenNo = '" + TokenNo + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.DashboardList.FromSqlRaw(sqlStr).ToListAsync();

                //var DashboardData = await AppDbContextAdm.DashboardList
                //                                                    .FromSqlRaw(
                //                                                        "EXEC dbo.Display_Emp_DashboardList @p_TokenNo = {0}, @p_LoginId = {1}",
                //                                                        TokenNo,
                //                                                        LoginId
                //                                                    )
                //                                                    .ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value),
                                        new SqlParameter("@p_FirebaseId", FirebaseId ?? (object)DBNull.Value),
                                    };

                var DashboardData = await AppDbContextAdm.IsValidData
                                            .FromSqlRaw("EXEC dbo.UpdateFirebaseId @p_LoginId, @p_FirebaseId", parameters)
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {
                return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.DashBoardList>> DisplayDashboardList(string TokenNo, string LoginId)
        {
            try
            {
                #region commented Old Working codes

                //string sqlStr = "exec dbo.Display_Emp_DashboardList @p_TokenNo = '" + TokenNo + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.DashboardList.FromSqlRaw(sqlStr).ToListAsync();

                //var DashboardData = await AppDbContextAdm.DashboardList
                //                                                    .FromSqlRaw(
                //                                                        "EXEC dbo.Display_Emp_DashboardList @p_TokenNo = {0}, @p_LoginId = {1}",
                //                                                        TokenNo,
                //                                                        LoginId
                //                                                    )
                //                                                    .ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_TokenNo", TokenNo ?? (object)DBNull.Value),
                                        new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value),
                                    };

                var DashboardData = await AppDbContextAdm.DashboardList
                                            .FromSqlRaw("EXEC dbo.Display_Emp_DashboardList @p_TokenNo, @p_LoginId", parameters)
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {
                return (IEnumerable<DashBoardList>)Enumerable.Empty<string>();
            }
            return (IEnumerable<DashBoardList>)Enumerable.Empty<string>();
        }

        #endregion

        #region Payroll Module


        #region Attendance Summary and Detail and Mispunch logic

        public async Task<IEnumerable<CommonProcOutputFields.Ddl_Value_Nm>> GetMonthYr_EmpInfo()
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec [webpacepayroll].dbo.GetQuery 'Edit_Att_Mnth_yr', '', '', '' ";
                //string sqlStr = "exec dbo.GetMonthYear_EmpInfo ";
                //var DashboardData = await AppDbContextAdm.Ddl_Value_Nm.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Ddl_Value_Nm
                                                                    .FromSqlRaw("EXEC dbo.GetMonthYear_EmpInfo")
                                                                    .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "exec dbo.GetEmpMisPunchDtl_Summ @p_EmpId = '" + EmpId + "', " +
                //                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_MispunchDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId);
                var monYrParam = new SqlParameter("@p_MonYr", mispunchDtl_EmpInfo.MonthYr ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_MispunchDtl_EmpInfo
                                                                                .FromSqlRaw("EXEC dbo.GetEmpMisPunchDtl_Summ @p_EmpId, @p_MonYr", empIdParam, monYrParam)
                                                                                .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.GetEmpAttDtl_Detail @p_EmpId = '" + EmpId + "', " +
                //                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_AttDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();
                //string sqlStr = "exec dbo.GetEmpAttDtl_Detail @p_EmpId = '" + EmpId + "', " +
                //"@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_AttDtl_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId);
                var monYrParam = new SqlParameter("@p_MonYr", mispunchDtl_EmpInfo.MonthYr ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_AttDtl_EmpInfo
                    .FromSqlRaw("EXEC dbo.GetEmpAttDtl_Detail @p_EmpId, @p_MonYr", empIdParam, monYrParam)
                    .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetEmpAttDtl_Summ @p_EmpId = '" + EmpId + "', " +
                //                "@p_MonYr = '" + mispunchDtl_EmpInfo.MonthYr + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_AttSumm_EmpInfo.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId);
                var monYrParam = new SqlParameter("@p_MonYr", mispunchDtl_EmpInfo.MonthYr ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_AttSumm_EmpInfo
                    .FromSqlRaw("EXEC dbo.EmpApp_GetEmpAttDtl_Summ @p_EmpId, @p_MonYr", empIdParam, monYrParam)
                    .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_AttSumm_EmpInfo>)Enumerable.Empty<string>();
        }

        #endregion

        #region Emp Summary Dashboard
        public async Task<IEnumerable<CommonProcOutputFields.ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(string EmpId, LoginIdNum loginIdNum)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetEmpSumm_DashData @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + loginIdNum.LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.EmpSummary_Dashboard.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId);
                var loginIdParam = new SqlParameter("@p_LoginId", loginIdNum.LoginId ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.EmpSummary_Dashboard
                    .FromSqlRaw("EXEC dbo.EmpApp_GetEmpSumm_DashData @p_EmpId, @p_LoginId", empIdParam, loginIdParam)
                    .ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<ret_EmpSummary_Dashboard>)Enumerable.Empty<string>();
        }

        #endregion

        #region Leave and Overtime Entries
        public async Task<IEnumerable<CommonProcOutputFields.OutSingleString>> GetLeaveDays(string EmpId, GetLeaveDays getLeaveDays)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveDays @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + getLeaveDays.LoginId + "', " +
                //                "@p_ToDt = '" + getLeaveDays.LeaveDate + "', " +
                //                "@p_LeaveType = '" + getLeaveDays.LeaveType + "' ";
                //var DashboardData = await AppDbContextAdm.OutSingleString.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", getLeaveDays.LoginId ?? (object)DBNull.Value);
                var toDateParam = new SqlParameter("@p_ToDt", getLeaveDays.LeaveDate ?? (object)DBNull.Value);
                var leaveTypeParam = new SqlParameter("@p_LeaveType", getLeaveDays.LeaveType ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.OutSingleString
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveDays @p_EmpId, @p_LoginId, @p_ToDt, @p_LeaveType",
                        empIdParam, loginIdParam, toDateParam, leaveTypeParam)
                    .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveNames @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_Value_Name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_Value_Name
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveNames @p_EmpId, @p_LoginId", empIdParam, loginIdParam)
                    .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveReason @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_Name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_Name
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveReason @p_EmpId, @p_LoginId", empIdParam, loginIdParam)
                    .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveDelayReason @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_LvDelayReason.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_LvDelayReason
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveDelayReason @p_EmpId, @p_LoginId", empIdParam, loginIdParam)
                    .ToListAsync();

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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveRelieverNm @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_id_name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);

                var DashboardData = await AppDbContextAdm.Resp_id_name
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveRelieverNm @p_EmpId, @p_LoginId", empIdParam, loginIdParam)
                    .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLeaveEntryList @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "', @p_Flag = '" + Flag + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_LvEntryList.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);
                var flagParam = new SqlParameter("@p_Flag", Flag ?? (object)DBNull.Value);

                // Secure query call
                var DashboardData = await AppDbContextAdm.Resp_LvEntryList
                    .FromSqlRaw("EXEC dbo.EmpApp_GetLeaveEntryList @p_EmpId, @p_LoginId, @p_Flag",
                        empIdParam, loginIdParam, flagParam)
                    .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetHeaderList @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "', @p_Flag = '" + Flag + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_HeaderEntryList.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var empIdParam = new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value);
                var loginIdParam = new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value);
                var flagParam = new SqlParameter("@p_Flag", Flag ?? (object)DBNull.Value);

                // Safe execution with parameters
                var DashboardData = await AppDbContextAdm.Resp_HeaderEntryList
                    .FromSqlRaw("EXEC dbo.EmpApp_GetHeaderList @p_EmpId, @p_LoginId, @p_Flag",
                        empIdParam, loginIdParam, flagParam)
                    .ToListAsync();
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
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_SaveLeaveEntry @p_EmpId = '" + EmpId + "', @p_LoginId = '" + saveLeaveEntry.LoginId + "', " +
                //                "@p_entrytype = '" + saveLeaveEntry.EntryType + "', @p_leaveshortname = '" + saveLeaveEntry.LeaveShortName + "', " +
                //                "@p_leavefullname = '" + saveLeaveEntry.LeaveFullName + "', " +
                //                "@p_fromdate = '" + Convert.ToDateTime(saveLeaveEntry.FromDate).ToString("MM/dd/yyyy hh:mm:ss tt") + "', " +
                //                "@p_todate = '" + Convert.ToDateTime(saveLeaveEntry.ToDate).ToString("MM/dd/yyyy hh:mm:ss tt") + "', " +
                //                "@p_reason = '" + saveLeaveEntry.Reason + "', @p_note = '" + saveLeaveEntry.Note + "', " +
                //                "@p_leavedays = '" + saveLeaveEntry.LeaveDays + "', @p_overtimeminutes = '" + saveLeaveEntry.OverTimeMinutes + "', " +
                //                "@p_usr_nm = '" + saveLeaveEntry.Usr_Nm + "', @p_reliever_empcode = '" + saveLeaveEntry.Reliever_Empcode + "', " +
                //                "@p_DelayLVNote = '" + saveLeaveEntry.DelayLVNote + "', @p_LeaveDivision = '" + saveLeaveEntry.LeaveDivision + "', " +
                //                "@p_Flag = '" + saveLeaveEntry.Flag + "' ";
                //var DashboardData = await AppDbContextAdm.SavedYesNo.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value),
                                        new SqlParameter("@p_LoginId", saveLeaveEntry.LoginId ?? (object)DBNull.Value),
                                        new SqlParameter("@p_entrytype", saveLeaveEntry.EntryType ?? (object)DBNull.Value),
                                        new SqlParameter("@p_leaveshortname", saveLeaveEntry.LeaveShortName ?? (object)DBNull.Value),
                                        new SqlParameter("@p_leavefullname", saveLeaveEntry.LeaveFullName ?? (object)DBNull.Value),
                                        new SqlParameter("@p_fromdate", saveLeaveEntry.FromDate ?? (object)DBNull.Value),
                                        new SqlParameter("@p_todate", saveLeaveEntry.ToDate ?? (object)DBNull.Value),
                                        new SqlParameter("@p_reason", saveLeaveEntry.Reason ?? (object)DBNull.Value),
                                        new SqlParameter("@p_note", saveLeaveEntry.Note ?? (object)DBNull.Value),
                                        new SqlParameter("@p_leavedays", saveLeaveEntry.LeaveDays ?? (object)DBNull.Value),
                                        new SqlParameter("@p_overtimeminutes", saveLeaveEntry.OverTimeMinutes ?? (object)DBNull.Value),
                                        new SqlParameter("@p_usr_nm", saveLeaveEntry.Usr_Nm ?? (object)DBNull.Value),
                                        new SqlParameter("@p_reliever_empcode", saveLeaveEntry.Reliever_Empcode ?? (object)DBNull.Value),
                                        new SqlParameter("@p_DelayLVNote", saveLeaveEntry.DelayLVNote ?? (object)DBNull.Value),
                                        new SqlParameter("@p_LeaveDivision", saveLeaveEntry.LeaveDivision ?? (object)DBNull.Value),
                                        new SqlParameter("@p_Flag", saveLeaveEntry.Flag ?? (object)DBNull.Value)
                                    };

                var DashboardData = await AppDbContextAdm.SavedYesNo
                    .FromSqlRaw("EXEC dbo.EmpApp_SaveLeaveEntry @p_EmpId, @p_LoginId, @p_entrytype, @p_leaveshortname, @p_leavefullname, " +
                                "@p_fromdate, @p_todate, @p_reason, @p_note, @p_leavedays, @p_overtimeminutes, @p_usr_nm, " +
                                "@p_reliever_empcode, @p_DelayLVNote, @p_LeaveDivision, @p_Flag", parameters)
                    .ToListAsync();
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

        #endregion

        #region Duty Schedule
        public async Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> EmpApp_GetShiftWeekList(string EmpId, string LoginId)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetShiftWeekList @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_Value_Name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_EmpId", EmpId ?? (object)DBNull.Value),
                                        new SqlParameter("@p_LoginId", LoginId ?? (object)DBNull.Value)
                                    };

                var DashboardData = await AppDbContextAdm.Resp_Value_Name
                    .FromSqlRaw("EXEC dbo.EmpApp_GetShiftWeekList @p_EmpId, @p_LoginId", parameters)
                    .ToListAsync();
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
                DataTable objresutl = new DataTable();
                try
                {
                    string sqlStr = "exec dbo.EmpApp_GetEmpShiftReport @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_DtRange = '" + DtRange + "' ";
                    //var DashboardData = await AppDbContextAdm.DrDynamicDt.FromSqlRaw(sqlStr).ToListAsync();
                    //return DashboardData;
                    objresutl = _dbMethods.GetDataTable(sqlStr);

                    return objresutl;
                }
                catch (Exception ex)
                { }
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

        #endregion

        #region LV/OT Approval System
        public async Task<IEnumerable<CommonProcOutputFields.Resp_LV_OT_RolesList>> EmpApp_Get_LV_OT_RolesList(string EmpId, string LoginId, string RoleNm, string Flag)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_Get_LV_OT_Roles @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', " +
                                "@p_Role = '" + RoleNm + "', @p_Flag = '" + Flag + "' ";
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

        //public async Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_Upd_LV_OT_Entry(string EmpId, Upd_Lv_OT_entry upd_Lv_OT_entry, IDbContextTransaction transaction)
        public async Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_Upd_LV_OT_Entry(string EmpId, Upd_Lv_OT_entry upd_Lv_OT_entry)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_upd_leave_detail @p_LoginId = '" + upd_Lv_OT_entry.LoginId + "', @p_EmpId = '" + EmpId + "', " +
                                "@p_Flag = '" + upd_Lv_OT_entry.Flag + "', @p_leavedetailid = '" + upd_Lv_OT_entry.LeaveDetailId + "', " +
                                "@p_action = '" + upd_Lv_OT_entry.Action + "', @p_reason = '" + upd_Lv_OT_entry.Reason + "', " +
                                "@p_usr_nm = '" + upd_Lv_OT_entry.UserName + "', @p_note = '" + upd_Lv_OT_entry.Note + "' ";


                var savedYN = await AppDbContextAdm.SavedYesNo.FromSqlRaw(sqlStr).ToListAsync();
                //var savedYN = await AppDbContextAdm.Database.ExecuteSqlRawAsync(sqlStr, transaction.GetDbTransaction());
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

        #endregion


        #endregion

        #region Pharmacy Module

        #region Prescription Viewer & Prescription Medicines
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
                string sqlStr = "exec dbo.EmpApp_GetWards @p_EmpId = '" + EmpId + "', " +
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
                string sqlStr = "exec dbo.EmpApp_GetFloors @p_EmpId = '" + EmpId + "', " +
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


        #endregion

        #endregion

        public async Task<IEnumerable<DoctorNotification>> GetDrNotifications(string loginId, string EmpId)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.GetDrNotifications @p_LoginId = '" + loginId + "', @p_DrId = '" + EmpId + "' ";
                //var DrNotification = await AppDbContextAdm.DrNotification.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_LoginId", loginId),
                                        new SqlParameter("@p_DrId", EmpId)
                                    };

                var DrNotification = await AppDbContextAdm.DrNotification
                    .FromSqlRaw("exec dbo.GetDrNotifications @p_LoginId, @p_DrId", parameters)
                    .ToListAsync();
                return DrNotification;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<DoctorNotification>)Enumerable.Empty<string>();
        }

        #region Module & Screen Rights

        public async Task<IEnumerable<CommonProcOutputFields.ModuleScreenRights>> GetModuleRights(string EmpId, string LoginId, string ModuleName)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetModuleRights @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "', @p_ModuleName = '" + ModuleName + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_ModuleScreenRights.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var parameters = new[]
                                    {
                                        new SqlParameter("@p_EmpId", EmpId),
                                        new SqlParameter("@p_LoginId", LoginId),
                                        new SqlParameter("@p_ModuleName", ModuleName)
                                    };

                var DashboardData = await AppDbContextAdm.Resp_ModuleScreenRights
                    .FromSqlRaw("exec dbo.EmpApp_GetModuleRights @p_EmpId, @p_LoginId, @p_ModuleName", parameters)
                    .ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<ModuleScreenRights>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.ModuleScreenRights>> GetEmpAppScreenRights(string EmpId, string LoginId, string ModuleName)
        {
            try
            {
                #region commeneted old working code

                //string sqlStr = "exec dbo.EmpApp_GetEmpAppScreenRights @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "', @p_ModuleName = '" + ModuleName + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_ModuleScreenRights.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_ModuleScreenRights
                                            .FromSqlInterpolated($"exec dbo.EmpApp_GetEmpAppScreenRights @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_ModuleName = {ModuleName}")
                                            .ToListAsync();
                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<ModuleScreenRights>)Enumerable.Empty<string>();
        }

        #endregion

        #region IPD Module


        #region Admitted Patients List
        public async Task<IEnumerable<CommonProcOutputFields.Organizations>> GetOrganizations(string EmpId, string LoginId)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetOrganizations @p_EmpId = '" + EmpId + "', " +
                //                "@p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Organizations.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Organizations
                                            .FromSqlInterpolated($"exec dbo.EmpApp_GetOrganizations @p_EmpId = {EmpId}, @p_LoginId = {LoginId}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Organizations>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.PatientList>> GetFilteredPatientData(string DrId, string LoginId, string PrefixText, List<string> Orgs, List<string> Floors, List<string> Wards)
        {
            try
            {
                string Orgs_commaSep = string.Join(",", Orgs);
                string Floors_commaSep = string.Join(",", Floors);
                string Wards_commaSep = string.Join(",", Wards);

                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetDeptPatientList @p_EmpId = '" + DrId + "', @p_LoginId = '" + LoginId + "', " +
                //                "@p_PrefixText = '" + PrefixText + "', @p_Orgs = '" + Orgs_commaSep + "', " +
                //                "@p_Floors = '" + Floors_commaSep + "', @p_Wards = '" + Wards_commaSep + "' ";
                //var DashboardData = await AppDbContextAdm.PatientList.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.PatientList
                                            .FromSqlInterpolated($"exec dbo.EmpApp_GetDeptPatientList @p_EmpId = {DrId}, @p_LoginId = {LoginId}, @p_PrefixText = {PrefixText}, @p_Orgs = {Orgs_commaSep}, @p_Floors = {Floors_commaSep}, @p_Wards = {Wards_commaSep}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<PatientList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.PatientList>> SortDeptPatientList(string EmpId, string LoginId, string SortType)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_SortDeptPatientList @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', " +
                //                "@p_SortType = '" + SortType + "' ";
                //var DashboardData = await AppDbContextAdm.PatientList.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.PatientList
                                            .FromSqlInterpolated($"exec dbo.EmpApp_SortDeptPatientList @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_SortType = {SortType}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<PatientList>)Enumerable.Empty<string>();
        }

        #endregion

        #region Patient Lab reports & Lab Summary

        public async Task<DataSet> GetPatientLabReports(string DrId, string IpdNo, string UHID)
        {
            try
            {
                string sqlStr = "exec dbo.DrApp_GetPatientLabReports @p_UHID = '" + UHID + "', @p_IpdNo = '" + IpdNo + "' ";
                DataSet objresutl = new DataSet();

                try
                {
                    objresutl = _dbMethods.GetDataSet(sqlStr);

                    return objresutl;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                }
                return objresutl;
            }
            catch (Exception ex)
            {

            }
            return new DataSet();
        }

        public async Task<DataSet> GetPatientSummaryLabData(string DrId, string IpdNo, string UHID)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_GetPatientLabSummaryData @p_UHID = '" + UHID + "', @p_IpdNo = '" + IpdNo + "' ";
                DataSet objresutl = new DataSet();

                try
                {
                    objresutl = _dbMethods.GetDataSet(sqlStr);

                    return objresutl;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                }
                return objresutl;
            }
            catch (Exception ex)
            {

            }
            return new DataSet();
        }

        #endregion

        #region Investigation Requisition
        public async Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> EmpApp_GetExternalLabName(string EmpId, string LoginId)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetExternalLabNm @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_id_name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_id_name
                                            .FromSqlInterpolated($"exec dbo.EmpApp_GetExternalLabNm @p_EmpId = {EmpId}, @p_LoginId = {LoginId}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_id_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_name>> EmpApp_InvReq_GetServiceGrp(string EmpId, string LoginId, string SearchText)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_GetServiceGrp @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_SearchText = '" + SearchText + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_Name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_Name
                                            .FromSqlInterpolated($"exec dbo.EmpApp_InvReq_GetServiceGrp @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_SearchText = {SearchText}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_txt_name_value>> EmpApp_InvReq_SearchService(string EmpId, string LoginId, string SearchText)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_SearchService @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_SearchText = '" + SearchText + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_txt_name_val.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_txt_name_val
                                            .FromSqlInterpolated($"exec dbo.EmpApp_InvReq_SearchService @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_SearchText = {SearchText}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_txt_name_value>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_id_int_name>> EmpApp_InvReq_SearchDrName(string EmpId, string LoginId, string SearchText, string Srv)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_SearchDrName @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_SearchText = '" + SearchText + "', @p_Srv = '" + Srv + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_id_int_name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_id_int_name
                                            .FromSqlInterpolated($"exec dbo.EmpApp_InvReq_SearchDrName @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_SearchText = {SearchText}, @p_Srv = {Srv}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_id_int_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_Get_Query>> EmpApp_InvReq_Get_Query(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_Get_Query @p_EmpId = '" + invReq_Get_Query.EmpId + "', @p_LoginId = '" + invReq_Get_Query.LoginId + "', " +
                //                "@P_TYPE = '" + invReq_Get_Query.TYPE + "', @P_VAL_1 = '" + invReq_Get_Query.Top10_40 + "', @P_VAL_2 = '" + invReq_Get_Query.IPD + "', " +
                //                "@P_VAL_3 = '" + invReq_Get_Query.SrchService + "', @P_VAL_4 = '" + invReq_Get_Query.InvType + "', @P_VAL_5 = '" + invReq_Get_Query.SrvGrp + "', " +
                //                "@P_VAL_6 = '" + invReq_Get_Query.ExtLabNm + "', @P_VAL_7 = '" + invReq_Get_Query.Val7 + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_InvReq_Get_Qry.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_InvReq_Get_Qry
                                            .FromSqlInterpolated($@"
                                                exec dbo.EmpApp_InvReq_Get_Query 
                                                    @p_EmpId = {invReq_Get_Query.EmpId}, 
                                                    @p_LoginId = {invReq_Get_Query.LoginId}, 
                                                    @P_TYPE = {invReq_Get_Query.TYPE}, 
                                                    @P_VAL_1 = {invReq_Get_Query.Top10_40}, 
                                                    @P_VAL_2 = {invReq_Get_Query.IPD}, 
                                                    @P_VAL_3 = {invReq_Get_Query.SrchService}, 
                                                    @P_VAL_4 = {invReq_Get_Query.InvType}, 
                                                    @P_VAL_5 = {invReq_Get_Query.SrvGrp}, 
                                                    @P_VAL_6 = {invReq_Get_Query.ExtLabNm}, 
                                                    @P_VAL_7 = {invReq_Get_Query.Val7}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_InvReq_Get_Query>)Enumerable.Empty<string>();
        }

        public async Task<RequestSheetIPD> SaveRequestSheetIPD_Dapper(RequestSheetIPD model)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("VHMobileDBConnection")))
                {
                    var dp = new DynamicParameters();
                    dp.Add("@p_action", model.Action);
                    dp.Add("@p_ipdno", model.IPDNo);
                    dp.Add("@p_uhid", model.UHIDNo);
                    dp.Add("@p_dt", model.Dt);
                    dp.Add("@p_req_typ", model.ReqType);
                    dp.Add("@p_rmk", model.Remark);
                    dp.Add("@p_usr_nm", model.Username);
                    dp.Add("@p_emerg", model.IsEmergency);
                    dp.Add("@p_clinic_rmk", model.ClinicRemark);
                    dp.Add("@p_inv_priority", model.InvestPriority);

                    dp.Add("@o_idn", model.ReqId, DbType.Int32, ParameterDirection.InputOutput);
                    dp.Add("@o_dr_inst_id", model.Dr_Inst_Id, DbType.Int32, ParameterDirection.InputOutput);
                    dp.Add("@o_bill_detail_id", model.Bill_Detail_Id, DbType.Int32, ParameterDirection.InputOutput);

                    await connection.ExecuteAsync("WEBPACEDATA2019.dbo.SaveRequestSheetIPD", dp, commandType: CommandType.StoredProcedure);

                    model.ReqId = dp.Get<int>("@o_idn");
                    model.Dr_Inst_Id = dp.Get<int>("@o_dr_inst_id");
                    model.Bill_Detail_Id = dp.Get<int>("@o_bill_detail_id");

                    return model;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving RequestSheetIPD: " + ex.Message);
            }
        }

        public async Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> SaveRequestSheetDetailsIPD_Dapper(List<RequestSheetDetailsIPD> list)
        {
            var response = new List<CommonProcOutputFields.SavedYesNo>();
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("VHMobileDBConnection")))
                {
                    foreach (var item in list.Where(x => x.RowState != DataRowState.Unchanged))
                    {
                        var dp = new DynamicParameters();
                        dp.Add("@p_action", item.Action);
                        dp.Add("@p_req_id", item.MReqId);
                        dp.Add("@p_testname", item.ServiceName);
                        dp.Add("@p_testcode", item.ServiceId);
                        dp.Add("@p_reqby", item.Username);
                        dp.Add("@p_req_typ", item.ReqTyp);
                        dp.Add("@p_dr_inst_id", item.Dr_Inst_Id);
                        dp.Add("@p_bill_detail_id", item.Bill_Detail_Id);
                        dp.Add("@p_UHID", item.UHIDNo);
                        dp.Add("@p_ipd", item.IPDNo);
                        dp.Add("@p_RepDrId", item.DrID);
                        dp.Add("@p_RepDrName", item.DrNAME);

                        await connection.ExecuteAsync("WEBPACEDATA2019.dbo.SaveRequestSheetDetailsIPD", dp, commandType: CommandType.StoredProcedure);
                    }

                    response.Add(new CommonProcOutputFields.SavedYesNo { SavedYN = "Y" });
                }
            }
            catch (Exception ex)
            {
                response.Add(new CommonProcOutputFields.SavedYesNo { SavedYN = "Error: " + ex.Message });
            }

            return response;
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_Get_HistData>> EmpApp_InvReq_Get_HIstoryData(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_Get_HIstoryData @p_EmpId = '" + invReq_Get_Query.EmpId + "', @p_LoginId = '" + invReq_Get_Query.LoginId + "', " +
                //                "@P_TYPE = 'GET_INV_HIS_MASTER', @P_VAL_1 = '" + invReq_Get_Query.Top10_40 + "', @P_VAL_2 = '" + invReq_Get_Query.Top10_40 + "', " +
                //                "@P_VAL_3 = '" + invReq_Get_Query.SrchService + "', @P_VAL_4 = '" + invReq_Get_Query.InvType + "', @P_VAL_5 = '" + invReq_Get_Query.SrvGrp + "', " +
                //                "@P_VAL_6 = '" + invReq_Get_Query.ExtLabNm + "', @P_VAL_7 = '" + invReq_Get_Query.Val7 + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_InvReq_Get_HistData.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_InvReq_Get_HistData
                                            .FromSqlInterpolated($@"
                                                exec dbo.EmpApp_InvReq_Get_HIstoryData 
                                                    @p_EmpId = {invReq_Get_Query.EmpId}, 
                                                    @p_LoginId = {invReq_Get_Query.LoginId}, 
                                                    @P_TYPE = {"GET_INV_HIS_MASTER"}, 
                                                    @P_VAL_1 = {invReq_Get_Query.Top10_40}, 
                                                    @P_VAL_2 = {invReq_Get_Query.Top10_40}, 
                                                    @P_VAL_3 = {invReq_Get_Query.SrchService}, 
                                                    @P_VAL_4 = {invReq_Get_Query.InvType}, 
                                                    @P_VAL_5 = {invReq_Get_Query.SrvGrp}, 
                                                    @P_VAL_6 = {invReq_Get_Query.ExtLabNm}, 
                                                    @P_VAL_7 = {invReq_Get_Query.Val7}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_InvReq_Get_HistData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_InvReq_SelReq_HistDetail>> EmpApp_InvReq_SelReq_HistoryDetail(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_SelReq_HistoryDetail @p_EmpId = '" + invReq_Get_Query.EmpId + "', @p_LoginId = '" + invReq_Get_Query.LoginId + "', " +
                //                "@P_TYPE = 'GET_INV_HIS_DETAIL', @P_VAL_1 = '" + invReq_Get_Query.Top10_40 + "', @P_VAL_2 = '" + invReq_Get_Query.Top10_40 + "', " +
                //                "@P_VAL_3 = '" + invReq_Get_Query.SrchService + "', @P_VAL_4 = '" + invReq_Get_Query.InvType + "', @P_VAL_5 = '" + invReq_Get_Query.SrvGrp + "', " +
                //                "@P_VAL_6 = '" + invReq_Get_Query.ExtLabNm + "', @P_VAL_7 = '" + invReq_Get_Query.Val7 + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_InvReq_SelReq_HistDetail.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_InvReq_SelReq_HistDetail
                                            .FromSqlInterpolated($@"
                                                exec dbo.EmpApp_InvReq_SelReq_HistoryDetail 
                                                    @p_EmpId = {invReq_Get_Query.EmpId}, 
                                                    @p_LoginId = {invReq_Get_Query.LoginId}, 
                                                    @P_TYPE = {"GET_INV_HIS_DETAIL"}, 
                                                    @P_VAL_1 = {invReq_Get_Query.Top10_40}, 
                                                    @P_VAL_2 = {invReq_Get_Query.Top10_40}, 
                                                    @P_VAL_3 = {invReq_Get_Query.SrchService}, 
                                                    @P_VAL_4 = {invReq_Get_Query.InvType}, 
                                                    @P_VAL_5 = {invReq_Get_Query.SrvGrp}, 
                                                    @P_VAL_6 = {invReq_Get_Query.ExtLabNm}, 
                                                    @P_VAL_7 = {invReq_Get_Query.Val7}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_InvReq_SelReq_HistDetail>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.RespWebCreds>> Validate_Web_Creds(WebEmpMobileCreds mobileCreds)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_Validate_Web_Creds @p_LoginId = '" + mobileCreds.LoginId + "', @p_Mobile = '" + mobileCreds.MobileNo + "', @p_Password = '" + mobileCreds.Password + "' ";
                //var IsValidData = await AppDbContextAdm.RespWebCreds.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var IsValidData = await AppDbContextAdm.RespWebCreds
                                        .FromSqlInterpolated($@"
                                            exec dbo.EmpApp_Validate_Web_Creds 
                                                @p_LoginId = {mobileCreds.LoginId}, 
                                                @p_Mobile = {mobileCreds.MobileNo}, 
                                                @p_Password = {mobileCreds.Password}, 
                                                @p_FormScreenName = {mobileCreds.FormScreen}")
                                        .ToListAsync();

                return IsValidData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<RespWebCreds>)Enumerable.Empty<string>();
        }

        public async Task EmpApp_Delete_InvReq_Detail(InvReq_Del_ReqDtl invReq_Del_ReqDtl)
        {
            try
            {
                //string sqlStr = "exec [Webpacedata2019].dbo.ValidationForDeleteInvReq @p_LoginId = '" + invReq_Del_ReqDtl.LoginId + "', " +
                //                "@p_req_sht_dtl_id = '" + invReq_Del_ReqDtl.req_sht_dtl_id + "', @p_usr_nm = '" + invReq_Del_ReqDtl.UserName + "', " +
                //                "@p_form_nm = '" + invReq_Del_ReqDtl.FormName + "' ";
                ////var DashboardData = await AppDbContextAdm.Resp_InvReq_Get_HistData.FromSqlRaw(sqlStr).ToListAsync();
                //await AppDbContextAdm.Database.ExecuteSqlRawAsync(sqlStr);
                await AppDbContextAdm.Database.ExecuteSqlRawAsync(
                    "exec [Webpacedata2019].dbo.ValidationForDeleteInvReq @p_req_sht_dtl_id, @p_usr_nm, @p_form_nm",
                    new SqlParameter("@p_req_sht_dtl_id", invReq_Del_ReqDtl.req_sht_dtl_id),
                    new SqlParameter("@p_usr_nm", invReq_Del_ReqDtl.UserName),
                    new SqlParameter("@p_form_nm", invReq_Del_ReqDtl.FormName)
                );
            }
            catch (SqlException sqlEx)
            {
                // You can also log this using ILogger if available
                throw new Exception("SQL Error occurred while deleting InvReq detail: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // General catch
                throw new Exception("An error occurred while deleting InvReq detail: " + ex.Message, ex);
            }
        }

        #endregion

        #region Medication Sheet


        public async Task<IEnumerable<CommonProcOutputFields.Ddl_Value_Nm>> EmpApp_GetAddMedicationDropdownData(string EmpId, string LoginId, string flag)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetExternalLabNm @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_id_name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Ddl_Value_Nm
                                            .FromSqlInterpolated($"exec dbo.EMPApp_Getdata_DrPres_Medicationsheet @p_LoginId = {LoginId}, @p_EmpId = {EmpId}, @p_flag = {flag} ")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Ddl_Value_Nm>)Enumerable.Empty<string>();
        }

        public async Task<int> EmpApp_GetAdmissionIdFrmIPD(string EmpId, string LoginId, string IpdNo)
        {
            try
            {
                var resultList = await AppDbContextAdm.Resp_Id
                    .FromSqlInterpolated($"exec dbo.EmpApp_GetAdmissionIdFrmIPD @p_LoginId = {LoginId}, @p_EmpId = {EmpId}, @p_IpdNo = {IpdNo}")
                    .ToListAsync();

                // If result found, get the Value field as int
                if (resultList != null && resultList.Any())
                {
                    var admissionIdStr = resultList.FirstOrDefault()?.Id.ToString();
                    if (int.TryParse(admissionIdStr, out int admissionId))
                    {
                        return admissionId;
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log exception
                Console.WriteLine("Error fetching AdmissionId from IPDNo: " + ex.Message);
            }

            return 0; // fallback if nothing found or error occurred
        }


        public async Task<IEnumerable<CommonProcOutputFields.Resp_DRTreatMaster>> EmpApp_GetDrTreatmentMaster(string EmpId, string LoginId, string IpdNo, string TreatTyp, string UserName)
        {
            try
            {
                //var DashboardData = await AppDbContextAdm.Resp_DRTreatMaster
                //                            .FromSqlInterpolated($@"exec dbo.EmpApp_GetDrTreatmentMaster @p_LoginId = {LoginId}, @p_EmpId = {EmpId},
                //                                                  @p_IpdNo = {IpdNo}, @p_TreatTyp = {TreatTyp}, @p_UserName = {UserName} ")
                //                            .ToListAsync();

                //return DashboardData;

                // Step 1: Call main treatment master
                var masterList = await AppDbContextAdm.Resp_DRTreatMaster
                                        .FromSqlInterpolated($@"EXEC dbo.EmpApp_GetDrTreatmentMaster 
                                                         @p_LoginId = {LoginId}, 
                                                         @p_EmpId = {EmpId},
                                                         @p_IpdNo = {IpdNo}, 
                                                         @p_TreatTyp = {TreatTyp}, 
                                                         @p_UserName = {UserName}")
                                        .ToListAsync();

                // Step 2: For each master row, fetch its detail
                foreach (var master in masterList)
                {
                    var details = await AppDbContextAdm.Resp_DRTreatDetail
                                        .FromSqlInterpolated($@"EXEC dbo.EmpApp_GetDrTreatmentDetail 
                                                         @p_MstId = {master.DRMstId}")
                                        .ToListAsync();

                    master.Detail = details;

                    // Optional formatting
                    master.FormateData();
                    foreach (var d in details)
                    {
                        d.FormateData();
                    }
                }

                return masterList;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_DRTreatMaster>)Enumerable.Empty<string>();
        }

        public async Task<Resp_DRTreatMaster> EmpApp_SaveDrTreatmentMaster(Resp_DRTreatMaster entity, string empId)
        {
            try
            {
                var dp = new DynamicParameters();
                dp.Add("@p_doc_treat_mst_idn", entity.DRMstId, DbType.Int32, ParameterDirection.InputOutput);
                dp.Add("@p_admission_idn", entity.AdmissionId, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_dte", entity.Date, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_rmk", entity.Remark, DbType.String, ParameterDirection.Input);
                dp.Add("@p_indoor_record_type", entity.IndoorRecordType?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_usr_nm", entity.UserName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_trm_nm", entity.TerminalName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_action", entity.Action, DbType.String, ParameterDirection.Input);
                dp.Add("@p_special_order", entity.SpecialOrder, DbType.String, ParameterDirection.Input);
                dp.Add("@p_provisional_diagnosis", entity.ProvisionalDiagnosis, DbType.String, ParameterDirection.Input);
                dp.Add("@p_wght", entity.Weight, DbType.String, ParameterDirection.Input);
                dp.Add("@p_TemplateName", entity.TemplateName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_PrescriptionType", entity.PrescriptionType, DbType.String, ParameterDirection.Input);
                dp.Add("@p_Precedence", entity.Precedence, DbType.String, ParameterDirection.Input);
                dp.Add("@p_PatientName", entity.PatientName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dob", entity.DOB, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_Age", entity.Age, DbType.String, ParameterDirection.Input);
                dp.Add("@p_CommunicationNumber", entity.CommunicationNumber, DbType.String, ParameterDirection.Input);
                dp.Add("@p_ConsDrId", entity.ConsDr?.Id, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_frm_emergency", entity.FrmEmerg, DbType.String, ParameterDirection.Input);

                using (var conn = AppDbContextAdm.Database.GetDbConnection())
                {
                    await conn.ExecuteAsync("WEBPACEDATA2019.dbo.pop_doc_treat_mst", dp, commandType: CommandType.StoredProcedure);
                    entity.DRMstId = dp.Get<int>("@p_doc_treat_mst_idn");
                }

                entity.FormateData();
                return entity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Resp_DRTreatDetail> EmpApp_SaveAddMedicinesSheet(Resp_DRTreatDetail Entity, string empId)
        {
            try
            {
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var dose1InIst = Entity.Dose1.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose1.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose2InIst = Entity.Dose2.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose2.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose3InIst = Entity.Dose3.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose3.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose4InIst = Entity.Dose4.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose4.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose5InIst = Entity.Dose5.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose5.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose6InIst = Entity.Dose6.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose6.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose7InIst = Entity.Dose7.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose7.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose8InIst = Entity.Dose8.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose8.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose9InIst = Entity.Dose9.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose9.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var dose10InIst = Entity.Dose10.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.Dose10.Value.ToUniversalTime(), istZone) : (DateTime?)null;
                var stopTimeInIst = Entity.StopTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(Entity.StopTime.Value.ToUniversalTime(), istZone) : (DateTime?)null;

                var dp = new DynamicParameters();
                dp.Add("@p_doc_treat_dtl_idn", Entity.DRDtlId, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_doc_treat_dtl_mst_idn", Entity.DRMstId, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_medicine_type", Entity.MedicineType.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_ttl_days", Entity.Days, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_item_nm", Entity.ItemName?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_item_nm_mnl", Entity.ItemNameMnl, DbType.String, ParameterDirection.Input);

                dp.Add("@p_qty", Entity.Qty, DbType.Int32, ParameterDirection.Input);
                dp.Add("@p_dose", Entity.Dose, DbType.String, ParameterDirection.Input);
                dp.Add("@p_route_typ", Entity?.Route?.Name, DbType.String, ParameterDirection.Input);

                dp.Add("@p_frequency_1", Entity.Frequency1?.Name != null ? Convert.ToDecimal(Entity.Frequency1.Name) : (decimal?)null, DbType.Decimal, ParameterDirection.Input);
                dp.Add("@p_frequency_2", Entity.Frequency2?.Name != null ? Convert.ToDecimal(Entity.Frequency2.Name) : (decimal?)null, DbType.Decimal, ParameterDirection.Input);
                dp.Add("@p_frequency_3", Entity.Frequency3?.Name != null ? Convert.ToDecimal(Entity.Frequency3.Name) : (decimal?)null, DbType.Decimal, ParameterDirection.Input);
                dp.Add("@p_frequency_4", Entity.Frequency4?.Name != null ? Convert.ToDecimal(Entity.Frequency4.Name) : (decimal?)null, DbType.Decimal, ParameterDirection.Input);
                dp.Add("@p_rmk", Entity.Remark, DbType.String, ParameterDirection.Input);

                dp.Add("@p_dose_1", dose1InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_1", Entity.DoseGivenBy1?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_2", dose2InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_2", Entity.DoseGivenBy2?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_3", dose3InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_3", Entity.DoseGivenBy3?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_4", dose4InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_4", Entity.DoseGivenBy4?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_5", dose5InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_5", Entity.DoseGivenBy5?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_6", dose6InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_6", Entity.DoseGivenBy6?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_7", dose7InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_7", Entity.DoseGivenBy7?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_8", dose8InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_8", Entity.DoseGivenBy8?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_9", dose9InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_9", Entity.DoseGivenBy9?.Name, DbType.String, ParameterDirection.Input);
                dp.Add("@p_dose_10", dose10InIst, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_dose_given_by_10", Entity.DoseGivenBy10?.Name, DbType.String, ParameterDirection.Input);

                dp.Add("@p_usr_nm", Entity.UserName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_trm_nm", Entity.TerminalName, DbType.String, ParameterDirection.Input);
                dp.Add("@p_instruction_typ", Entity.Instruction_typ?.Name, DbType.String, ParameterDirection.Input);
                if (Entity.StopTime != null)
                    dp.Add("@p_stop_time", stopTimeInIst, DbType.DateTime, ParameterDirection.Input);
                else
                    dp.Add("@p_stop_time", null, DbType.DateTime, ParameterDirection.Input);
                dp.Add("@p_flow_rate", Entity.FlowRate, DbType.String, ParameterDirection.Input);

                dp.Add("@p_action", Entity.Action, DbType.String, ParameterDirection.Input);

                dp.Add("@p_inact_dt", null, DbType.String, ParameterDirection.Input);
                dp.Add("@p_inact_by", null, DbType.String, ParameterDirection.Input);

                using (var conn = AppDbContextAdm.Database.GetDbConnection())
                {
                    await conn.ExecuteAsync("WEBPACEDATA2019.dbo.pop_doc_treat_dtl", dp, commandType: CommandType.StoredProcedure);
                }

                return Entity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_txt_name_value>> EmpApp_MedicationSheet_SearchMedicines(string EmpId, string LoginId, string SearchText)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_InvReq_SearchService @p_EmpId = '" + EmpId + "', @p_LoginId = '" + LoginId + "', @p_SearchText = '" + SearchText + "' ";
                //var DashboardData = await AppDbContextAdm.Resp_txt_name_val.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var DashboardData = await AppDbContextAdm.Resp_txt_name_val
                                            .FromSqlInterpolated($"exec dbo.EmpApp_Medsheet_SearchFormularyMedicines @p_EmpId = {EmpId}, @p_LoginId = {LoginId}, @p_SearchText = {SearchText}")
                                            .ToListAsync();

                return DashboardData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_txt_name_value>)Enumerable.Empty<string>();
        }

        public async Task<bool> DeleteDoctorTreatmentDetailAsync(int mstId, int dtlId, string userName)
        {
            try
            {
                var dp = new DynamicParameters();
                dp.Add("@p_doc_treat_dtl_idn", dtlId, DbType.Int32);
                dp.Add("@p_doc_treat_dtl_mst_idn", mstId, DbType.Int32);

                // Set all other parameters to null
                dp.Add("@p_medicine_type", null);
                dp.Add("@p_ttl_days", null);
                dp.Add("@p_item_nm", null);
                dp.Add("@p_item_nm_mnl", null);
                dp.Add("@p_qty", null);
                dp.Add("@p_dose", null);
                dp.Add("@p_route_typ", null);
                dp.Add("@p_frequency_1", null);
                dp.Add("@p_frequency_2", null);
                dp.Add("@p_frequency_3", null);
                dp.Add("@p_frequency_4", null);
                dp.Add("@p_rmk", null);

                for (int i = 1; i <= 10; i++)
                {
                    dp.Add($"@p_dose_{i}", null, DbType.DateTime);
                    dp.Add($"@p_dose_given_by_{i}", null);
                }

                dp.Add("@p_usr_nm", userName);
                dp.Add("@p_trm_nm", "");
                dp.Add("@p_instruction_typ", null);
                dp.Add("@p_stop_time", null);
                dp.Add("@p_flow_rate", null);
                dp.Add("@p_action", "Delete");
                dp.Add("@p_inact_dt", null);
                dp.Add("@p_inact_by", userName);

                using (var conn = AppDbContextAdm.Database.GetDbConnection())
                {
                    await conn.ExecuteAsync("WEBPACEDATA2019.dbo.pop_doc_treat_dtl", dp, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch
            {
                // Rethrow to controller
                throw;
            }
        }


        #endregion

        #region Dietician Checklist

        public async Task<IEnumerable<Resp_DieticianChecklist>> EMPApp_Getdata_DieticianChecklist(string EmpId, string LoginId, string PrefixText, List<string> Wards, List<string> Floors, List<string> Beds)
        {
            try
            {
                string Wards_commaSep = string.Join(",", Wards);
                string Floors_commaSep = string.Join(",", Floors);
                string Beds_commaSep = string.Join(",", Beds);
                var result = await AppDbContextAdm.Resp_DieticianChecklist
                                    .FromSqlInterpolated($@"EXEC dbo.EMPApp_Getdata_DieticianChecklist 
                                                @p_LoginId = {LoginId}, 
                                                @p_EmpId = {EmpId},
                                                @p_PrefixText = {PrefixText},
                                                @p_Wards = {Wards_commaSep}, 
                                                @p_Floors = {Floors_commaSep},
                                                @p_Beds = {Beds_commaSep}")
                                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Logging kar lena production me
            }

            return Enumerable.Empty<Resp_DieticianChecklist>();
        }

        public async Task<IEnumerable<Resp_WardWiseChecklistCount>> EMPApp_GetWardNm_Cnt_DieticianChecklist(string EmpId, string LoginId)
        {
            try
            {
                var result = await AppDbContextAdm.Resp_WardWiseChecklistCount
                                    .FromSqlInterpolated($@"EXEC dbo.EMPApp_GetWardNm_Cnt_DieticianChecklist 
                                                @p_LoginId = {LoginId}, 
                                                @p_EmpId = {EmpId}")
                                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Logging kar lena production me
            }

            return Enumerable.Empty<Resp_WardWiseChecklistCount>();
        }

        public async Task<DietChecklistMaster> EmpApp_SaveDietChecklistMaster(DietChecklistMaster entity)
        {
            try
            {
                if (entity.Id > 0)
                {
                    var dp = new DynamicParameters();
                    dp.Add("@p_id", entity.Id, DbType.Int32, ParameterDirection.Input);
                    dp.Add("@p_diagnosis", entity.Diagnosis, DbType.String, ParameterDirection.Input);
                    dp.Add("@p_diet", entity.Diet?.Name, DbType.String, ParameterDirection.Input);
                    dp.Add("@p_remark", entity.Remark, DbType.String, ParameterDirection.Input);
                    dp.Add("@p_username", entity.Username, DbType.String, ParameterDirection.Input);
                    dp.Add("@p_rel_fd_rmrk", entity.RelFood_Remark, DbType.String, ParameterDirection.Input);

                    using (var conn = AppDbContextAdm.Database.GetDbConnection())
                    {
                        await conn.ExecuteAsync("WEBPACEDATA2019.dbo.SaveDietChecklist", dp, commandType: CommandType.StoredProcedure);
                    }

                    return entity;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #endregion

        #endregion

        #region Notification Provision

        public async Task<IEnumerable<EMPNotificationList>> GetEMPNotificationsList(string loginId, string EmpId, int? days, string? tag, string? fromDate, string? toDate)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EMPApp_GetEMPNotificationsList @p_LoginId = '" + loginId + "', @p_EmpId = '" + EmpId + "', " +
                //                "@p_Days = " + days + ", @p_Tag = '" + tag + "', @p_FromDate = '" + fromDate + "', @p_ToDate = '" + toDate + "' ";
                //var EmpNotificationList = await AppDbContextAdm.EMPNotifyList.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var EmpNotificationList = await AppDbContextAdm.EMPNotifyList
                                                .FromSqlInterpolated($@"
                                                    exec dbo.EMPApp_GetEMPNotificationsList 
                                                        @p_LoginId = {loginId}, 
                                                        @p_EmpId = {EmpId}, 
                                                        @p_Days = {days}, 
                                                        @p_Tag = {tag}, 
                                                        @p_FromDate = {fromDate}, 
                                                        @p_ToDate = {toDate}")
                                                .ToListAsync();

                return EmpNotificationList;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<EMPNotificationList>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<IsValidData>> UpdateNotification_Read(string loginId, string NotificationId)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EMPApp_UpdateNotification_Read @p_LoginId = '" + loginId + "', @p_NotificationId = '" + NotificationId + "' ";
                //var EmpNotificationList = await AppDbContextAdm.IsValidData.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var EmpNotificationList = await AppDbContextAdm.IsValidData
                                                .FromSqlInterpolated($@"exec dbo.EMPApp_UpdateNotification_Read @p_LoginId = {loginId}, @p_NotificationId = {NotificationId}")
                                                .ToListAsync();

                return EmpNotificationList;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<IsValidData>> Save_DoctorVoiceNote(VoiceNoteFields voiceNoteFields)
        {
            try
            {
                #region Commented old working code

                //string sqlStr = "exec dbo.EmpApp_Save_DoctorVoiceNote @p_UHID = '" + voiceNoteFields.UHID + "', @p_IpdNo = '" + voiceNoteFields.IPDNo + "', " +
                //                "@p_PatientName = '" + voiceNoteFields.PatientName + "', @p_VoiceFileName = '" + voiceNoteFields.VoiceFileName + "', " +
                //                "@p_DoctorName = '" + voiceNoteFields.DoctorName + "', @p_LoginId = '" + voiceNoteFields.LoginId + "', " +
                //                "@p_CreatedUser = '" + voiceNoteFields.EmpID + "', @p_TranslatedText = '" + voiceNoteFields.TranslatedText + "' ";
                //var EmpNotificationList = await AppDbContextAdm.IsValidData.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var EmpNotificationList = await AppDbContextAdm.IsValidData
                                                    .FromSqlInterpolated($@"
                                                        exec dbo.EmpApp_Save_DoctorVoiceNote 
                                                        @p_UHID = {voiceNoteFields.UHID}, 
                                                        @p_IpdNo = {voiceNoteFields.IPDNo}, 
                                                        @p_PatientName = {voiceNoteFields.PatientName}, 
                                                        @p_VoiceFileName = {voiceNoteFields.VoiceFileName}, 
                                                        @p_DoctorName = {voiceNoteFields.DoctorName}, 
                                                        @p_LoginId = {voiceNoteFields.LoginId}, 
                                                        @p_CreatedUser = {voiceNoteFields.EmpID}, 
                                                        @p_TranslatedText = {voiceNoteFields.TranslatedText}")
                                                    .ToListAsync();

                return EmpNotificationList;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        #endregion

    }
}
