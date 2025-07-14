using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public class EmpLoginRepository : IEmpLoginRepository
    {
        public AppDbContext AppDbContextAdm { get; }
        public EmpLoginRepository(AppDbContext appDbContext)
        {
            AppDbContextAdm = appDbContext;
        }

        #region Validate Mobileno and OTP related

        public async Task<IEnumerable<CommonProcOutputFields.IsValidData>> ValidateMobileNo(string MobileNo)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_Validate_Emp_Mobile @p_Mobile = '" + MobileNo + "' ";
                //var IsValidNo = await AppDbContextAdm.IsValidData.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var IsValidNo = await AppDbContextAdm.IsValidData
                                        .FromSqlInterpolated($"exec dbo.EmpApp_Validate_Emp_Mobile @p_Mobile = {(object?)MobileNo ?? DBNull.Value}")
                                        .ToListAsync();

                return IsValidNo;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        //public async Task<IEnumerable<CommonProcOutputFields.OTP>> SendEmpMobileOTP(OTP respOTP)
        //{
        //    try
        //    {
        //        string sqlStr = "exec dbo.Send_EMP_MobileOTP @p_MobileNo = '"+ respOTP.MobileNo +"' ";
        //        var otp = await AppDbContextAdm.OTP.FromSqlRaw(sqlStr).ToListAsync();
        //        string SMSOtp = "", Message = "";
        //        if (otp != null && otp.Count() > 0)
        //        {
        //            SMSOtp = otp.Select(x => x.OTPNo).ToList()[0].ToString();
        //        }

        //        //Message = "Mobile number verification code is " + SMSOtp + ". Kindly share it with our hospital staff to complete the registration / admission / billing process. Stay Safe and Healthy. Team Venus Hospital, Surat.";

        //        //var client = new RestClient("http://login.bulksmsgateway.in/sendmessage.php");
        //        //var request = new RestRequest();
        //        //request.AddQueryParameter("user", "Venush");
        //        //request.AddQueryParameter("password", "vh*1600VH@");
        //        //request.AddQueryParameter("sender", "VENHOS");
        //        //request.AddQueryParameter("mobile", respOTP.MobileNo);
        //        //request.AddQueryParameter("type", "3");
        //        //request.AddQueryParameter("message", Message);
        //        //request.AddQueryParameter("template_id", "1407166029670465803");

        //        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        //RestResponse response = await client.GetAsync(request);
        //        //if (response.StatusCode == HttpStatusCode.OK)
        //        //{
        //        //    var SMSResult = JsonConvert.DeserializeObject<OTP>(response.Content);
        //        //    if (SMSResult.Status.ToLower() == "failed")
        //        //    {
        //        //        List<OTP> list = new List<OTP>();
        //        //        respOTP.Status = "failed";
        //        //        respOTP.OTPNo = "";
        //        //        list.Add(respOTP);
        //        //        return list;
        //        //    }
        //        //    return otp;
        //        //}
        //        //else
        //        //{
        //        //    return otp;
        //        //    throw new Exception(response?.ErrorMessage);
        //        //}
        //        return otp;
        //    }
        //    catch (Exception ex)
        //    {
        //        return (IEnumerable<OTP>)Enumerable.Empty<string>();
        //    }
        //    return (IEnumerable<OTP>)Enumerable.Empty<string>();
        //}

        public async Task<IEnumerable<CommonProcOutputFields.OTP>> SendEmpMobileOTP(OTP respOTP)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.Send_EMP_MobileOTP @p_MobileNo = '" + respOTP.MobileNo + "' ";
                //var otp = await AppDbContextAdm.OTP.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var otp = await AppDbContextAdm.OTP
                                    .FromSqlInterpolated($"exec dbo.Send_EMP_MobileOTP @p_MobileNo = {respOTP.MobileNo}")
                                    .ToListAsync();

                string SMSOtp = "";
                string Message = "";
                if (otp != null && otp.Count() > 0)
                {
                    SMSOtp = otp.Select(x => x.OTPNo).ToList()[0].ToString();
                    respOTP.MobileNo = otp.Select(x => x.MobileNo).ToList()[0].ToString();
                }

                //if (1==2 && !String.IsNullOrEmpty(SMSOtp))
                if (!String.IsNullOrEmpty(SMSOtp))
                {
                    #region commented some mobile no if condition

                    //if (respOTP.MobileNo == "9429728770")
                    //if (respOTP.MobileNo == "9429728770"
                    // || respOTP.MobileNo == "9905475111"
                    // || respOTP.MobileNo == "9316689895"
                    // || respOTP.MobileNo == "8141656676"
                    // || respOTP.MobileNo == "7779090003"
                    // || respOTP.MobileNo == "7874941246"
                    // || respOTP.MobileNo == "9879619705"
                    // || respOTP.MobileNo == "9228221143"
                    // || respOTP.MobileNo == "9925740695"
                    // || respOTP.MobileNo == "9726094066"
                    //   )
                    //{
                    
                    #endregion

                    Message = $"Greetings from Venus Hospital!\n{{0}} is your OTP to log in to VENUS HOSPITAL account and it is valid for {{1}} minutes. Do not share it with anyone for security reasons.\n\nStay Safe and Healthy!\nTeam Venus Hospital.";
                    int validityMinutes = 20;
                    string formattedMessage = string.Format(Message, SMSOtp, validityMinutes);

                    var client = new RestClient("http://login.bulksmsgateway.in/sendmessage.php");
                    //var client = new RestClient("http://api.bulksmsgateway.in/sendmessage.php");
                    var request = new RestRequest();
                    request.AddQueryParameter("user", "Venush");
                    request.AddQueryParameter("password", "vh*1600VH@");
                    request.AddQueryParameter("sender", "VENHOS");
                    request.AddQueryParameter("mobile", respOTP.MobileNo);
                    request.AddQueryParameter("type", "3");
                    request.AddQueryParameter("message", formattedMessage);
                    request.AddQueryParameter("template_id", "1407172449585399052");

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    RestResponse response = await client.GetAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var SMSResult = JsonConvert.DeserializeObject<OTP>(response.Content);
                        //if (SMSResult.Status.ToLower() == "failed")
                        //{
                        //    List<OTP> list = new List<OTP>();
                        //    respOTP.Status = "failed";
                        //    respOTP.OTPNo = "";
                        //    list.Add(respOTP);
                        //    return list;
                        //}
                        return otp;
                    }
                    else
                    {
                        return otp;
                        throw new Exception(response?.ErrorMessage);
                    }

                    #region old template
                    //Below Old working template
                    //Message = "Mobile number verification code is " + SMSOtp + ". Kindly share it with our hospital staff to complete the registration / admission / billing process. Stay Safe and Healthy. Team Venus Hospital, Surat.";
                    //var client = new RestClient("http://login.bulksmsgateway.in/sendmessage.php");
                    //var request = new RestRequest();
                    //request.AddQueryParameter("user", "Venush");
                    //request.AddQueryParameter("password", "vh*1600VH@");
                    //request.AddQueryParameter("sender", "VENHOS");
                    //request.AddQueryParameter("mobile", respOTP.MobileNo);
                    //request.AddQueryParameter("type", "3");
                    //request.AddQueryParameter("message", formattedMessage);
                    //request.AddQueryParameter("template_id", "1407166029670465803");

                    //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //RestResponse response = await client.GetAsync(request);
                    //if (response.StatusCode == HttpStatusCode.OK)
                    //{
                    //    var SMSResult = JsonConvert.DeserializeObject<OTP>(response.Content);
                    //    //if (SMSResult.Status.ToLower() == "failed")
                    //    //{
                    //    //    List<OTP> list = new List<OTP>();
                    //    //    respOTP.Status = "failed";
                    //    //    respOTP.OTPNo = "";
                    //    //    list.Add(respOTP);
                    //    //    return list;
                    //    //}
                    //    return otp;
                    //}
                    //else
                    //{
                    //    return otp;
                    //    throw new Exception(response?.ErrorMessage);
                    //}
                    #endregion

                    //}

                }
                return otp;
            }
            catch (Exception ex)
            {
                return (IEnumerable<OTP>)Enumerable.Empty<string>();
            }
            return (IEnumerable<OTP>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.IsValidData>> GenerateNewPassword(string MobileNo, string Password)
        {
            try
            {
                string sqlStr = "exec dbo.EmpApp_SaveMobilePassword @p_Mobile = '" + MobileNo + "', " +
                                "@p_Password = '" + Password + "' ";
                var IsValidNo = await AppDbContextAdm.IsValidData.FromSqlRaw(sqlStr).ToListAsync();
                return IsValidNo;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        #endregion

        #region Get Login Users and Validate their Credentials

        public async Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> GetLoginUserNames(string LoginId)
        {
            try
            {
                #region commented old woring code

                //string sqlStr = "exec dbo.EmpApp_GetLoginUserNames @p_LoginId = '" + LoginId + "' ";
                //var UsersData = await AppDbContextAdm.Resp_id_name.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var UsersData = await AppDbContextAdm.Resp_id_name
                                        .FromSqlInterpolated($"exec dbo.EmpApp_GetLoginUserNames @p_LoginId = {LoginId}")
                                        .ToListAsync();

                return UsersData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_id_name>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.Resp_LoginAs_Creds>> GetLoginAsUserCreds(string AdminMobileNo, string UserNm)
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_GetLoginAsUserCreds @p_AdminMobileNo = '" + AdminMobileNo + "', " +
                //                "@p_UserNm = '" + UserNm + "' ";
                //var CredsData = await AppDbContextAdm.Resp_LoginAs_Creds.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var CredsData = await AppDbContextAdm.Resp_LoginAs_Creds
                                        .FromSqlInterpolated($"exec dbo.EmpApp_GetLoginAsUserCreds @p_AdminMobileNo = {AdminMobileNo}, @p_UserNm = {UserNm}")
                                        .ToListAsync();

                return CredsData;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<Resp_LoginAs_Creds>)Enumerable.Empty<string>();
        }

        #endregion

        #region Log Issues

        public async Task<bool> ReportIssueAsync(IssueReportDto issue)
        {
            try
            {
                string safeErrorMessage = issue.ErrorMessage.Replace("'", "''");

                #region commented old working code

                //string sql = $"EXEC dbo.EmpApp_ReportIssue " +
                //             $"@p_ScreenName = '{issue.ScreenName}', " +
                //             $"@p_ErrorMessage = '{safeErrorMessage}', " +
                //             $"@p_LoginID = '{issue.LoginID}', " +
                //             $"@p_TokenNo = '{issue.TokenNo}', " +
                //             $"@p_EmpID = '{issue.EmpID}', " +
                //             $"@p_DeviceInfo = '{issue.DeviceInfo}'";

                //await AppDbContextAdm.Database.ExecuteSqlRawAsync(sql);
                
                #endregion

                await AppDbContextAdm.Database.ExecuteSqlInterpolatedAsync($@"
                                            EXEC dbo.EmpApp_ReportIssue 
                                                @p_ScreenName = {issue.ScreenName}, 
                                                @p_ErrorMessage = {safeErrorMessage}, 
                                                @p_LoginID = {issue.LoginID}, 
                                                @p_TokenNo = {issue.TokenNo}, 
                                                @p_EmpID = {issue.EmpID}, 
                                                @p_DeviceInfo = {issue.DeviceInfo}
                                        ");

                return true;
            }
            catch (Exception ex)
            {
                // TODO: Log the exception if needed
                return false;
            }
        }

        #endregion

        #region Force Update Provision

        public async Task<IEnumerable<ForceUpdateYN>> ForceUpdateYN()
        {
            try
            {
                #region commented old working code

                //string sqlStr = "exec dbo.EmpApp_ForceUpdateYN";
                //var IsValidNo = await AppDbContextAdm.ForceUpdateYN.FromSqlRaw(sqlStr).ToListAsync();

                #endregion

                var IsValidNo = await AppDbContextAdm.ForceUpdateYN.FromSqlInterpolated($"EXEC dbo.EmpApp_ForceUpdateYN").ToListAsync();
                return IsValidNo;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<ForceUpdateYN>)Enumerable.Empty<string>();
        }

        #endregion

    }
}
