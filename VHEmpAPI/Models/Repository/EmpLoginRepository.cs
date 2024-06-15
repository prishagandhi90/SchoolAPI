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

        public async Task<IEnumerable<CommonProcOutputFields.IsValidData>> ValidateMobileNo(string MobileNo)
        {
            try
            {
                string sqlStr = "exec dbo.Validate_Dr_Mobile @p_Mobile = '" + MobileNo + "' ";
                var IsValidNo = await AppDbContextAdm.IsValidData.FromSqlRaw(sqlStr).ToListAsync();
                return IsValidNo;
            }
            catch (Exception ex)
            {

            }
            return (IEnumerable<IsValidData>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CommonProcOutputFields.OTP>> SendEmpMobileOTP(OTP respOTP)
        {
            try
            {
                string sqlStr = "exec dbo.Send_EMP_MobileOTP @p_MobileNo = '"+ respOTP.MobileNo +"' ";
                var otp = await AppDbContextAdm.OTP.FromSqlRaw(sqlStr).ToListAsync();
                string SMSOtp = "", Message = "";
                if (otp != null && otp.Count() > 0)
                {
                    SMSOtp = otp.Select(x => x.OTPNo).ToList()[0].ToString();
                }

                //Message = "Mobile number verification code is " + SMSOtp + ". Kindly share it with our hospital staff to complete the registration / admission / billing process. Stay Safe and Healthy. Team Venus Hospital, Surat.";

                //var client = new RestClient("http://login.bulksmsgateway.in/sendmessage.php");
                //var request = new RestRequest();
                //request.AddQueryParameter("user", "Venush");
                //request.AddQueryParameter("password", "vh*1600VH@");
                //request.AddQueryParameter("sender", "VENHOS");
                //request.AddQueryParameter("mobile", respOTP.MobileNo);
                //request.AddQueryParameter("type", "3");
                //request.AddQueryParameter("message", Message);
                //request.AddQueryParameter("template_id", "1407166029670465803");

                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //RestResponse response = await client.GetAsync(request);
                //if (response.StatusCode == HttpStatusCode.OK)
                //{
                //    var SMSResult = JsonConvert.DeserializeObject<OTP>(response.Content);
                //    if (SMSResult.Status.ToLower() == "failed")
                //    {
                //        List<OTP> list = new List<OTP>();
                //        respOTP.Status = "failed";
                //        respOTP.OTPNo = "";
                //        list.Add(respOTP);
                //        return list;
                //    }
                //    return otp;
                //}
                //else
                //{
                //    return otp;
                //    throw new Exception(response?.ErrorMessage);
                //}
                return otp;
            }
            catch (Exception ex)
            {
                return (IEnumerable<OTP>)Enumerable.Empty<string>();
            }
            return (IEnumerable<OTP>)Enumerable.Empty<string>();
        }
    }
}
