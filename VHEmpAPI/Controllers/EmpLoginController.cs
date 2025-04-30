using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models.Repository;
using VHMobileAPI.Models;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpLoginController : ControllerBase
    {
        private readonly IEmpLoginRepository? empLoginRepository;
        private readonly IJwtAuth jwtAuth;
        public string Message = "";

        public EmpLoginController(IEmpLoginRepository empLoginRepository, IJwtAuth jwtAuth)
        {
            this.empLoginRepository = empLoginRepository;
            this.jwtAuth = jwtAuth;
        }

        [HttpPost("ValidateMobileNo")]
        public async Task<ActionResult<IsValidData>> ValidateMobileNo([FromBody] MobileNum mobileNum)
        {
            try
            {
                string PatientMobileNo = WebUtility.UrlDecode(mobileNum.MobileNo);
                var result = await empLoginRepository.ValidateMobileNo(PatientMobileNo);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {

            }
        }

        //[HttpPost("send")]
        //public async Task<IActionResult> SendMessage(string phoneNo, string message)
        //{
        //    string url = "http://api.bulksmsgateway.in/sendmessage.php";

        //    // Encode parameters
        //    message = HttpUtility.UrlEncode(message);
        //    string user = HttpUtility.UrlEncode("Venush");
        //    string password = HttpUtility.UrlEncode("vh*1600VH@");
        //    string sender = HttpUtility.UrlEncode("VENHOS");
        //    string type = HttpUtility.UrlEncode("3");
        //    string templateId = HttpUtility.UrlEncode("1407172449585399052");

        //    // Create query string
        //    string query = $"?user={user}&password={password}&sender={sender}" +
        //                   $"&mobile={HttpUtility.UrlEncode(phoneNo)}&type={type}" +
        //                   $"&message={message}&template_id={templateId}";

        //    try
        //    {
        //        // Send POST request
        //        HttpResponseMessage response = await _httpClient.PostAsync(url + query, null);
        //        response.EnsureSuccessStatusCode(); // Throw exception if not successful

        //        // Read response content
        //        string result = await response.Content.ReadAsStringAsync();
        //        return Ok(result); // Return 200 OK with the response content
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Handle any HTTP-specific errors
        //        return StatusCode(500, $"HTTP Request Error: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle other exceptions
        //        return StatusCode(500, $"Server Error: {ex.Message}");
        //    }
        //}

        [HttpPost("SendEmpMobileOTP")]
        public async Task<ActionResult<LoginOTP>> SendEmpMobileOTP([FromBody] MobileNum mobileNum)
        {
            try
            {
                string PatientMobileNo = WebUtility.UrlDecode(mobileNum.MobileNo);
                OTP respOTP = new OTP();
                respOTP.MobileNo = PatientMobileNo;
                respOTP.OTPNo = "";
                var result = await empLoginRepository.SendEmpMobileOTP(respOTP);

                if (result == null || result.Count() == 0)
                {
                    return Ok(new { StatusCode = 400, Message = "Bad Request", Data = new { } });
                }

                if (result.FirstOrDefault().OTPNo == "")
                {
                    return Ok(new { StatusCode = 400, IsSuccess = "false", Message = result != null ? result.FirstOrDefault().Message.ToString() : "", Data = new { } });
                }

                var response = Ok(result);
                if (response.StatusCode == 200)
                {
                    //   if (respOTP.MobileNo == "9429728770"
                    //|| respOTP.MobileNo == "9905475111"
                    //|| respOTP.MobileNo == "9316689895"
                    //|| respOTP.MobileNo == "8141656676"
                    //|| respOTP.MobileNo == "7779090003"
                    //|| respOTP.MobileNo == "7874941246"
                    //|| respOTP.MobileNo == "9879619705"
                    //|| respOTP.MobileNo == "9228221143"
                    //  )
                    //   {
                    //       Message = $"Greetings from Venus Hospital!\n{{0}} is your OTP to log in to VENUS HOSPITAL account and it is valid for {{1}} minutes. Do not share it with anyone for security reasons.\n\nStay Safe and Healthy!\nTeam Venus Hospital.";
                    //       int validityMinutes = 20;
                    //       string formattedMessage = string.Format(Message, result.FirstOrDefault().OTPNo, validityMinutes);
                    //       var sendMessage = await SendMessage(mobileNum.MobileNo, formattedMessage);
                    //   }

                    Message = result != null ? result.FirstOrDefault().Message.ToString() : "";
                    if (result.FirstOrDefault().OTPNo == "")
                    {
                        return Ok(new { StatusCode = 400, IsSuccess = "false", Message, Data = new { } });
                    }

                    return Ok(new { StatusCode = response.StatusCode, IsSuccess = "true", Message, Data = result.FirstOrDefault() });
                }
                else if (response.StatusCode != 200)
                {
                    return Ok(new { StatusCode = response.StatusCode, IsSuccess = "false", Message = "Bad Request", Data = new { } });
                }

                return Ok(new { StatusCode = response.StatusCode, IsSuccess = "true", Message, Data = result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {

            }
        }

        [HttpPost("GenerateNewPassword")]
        public async Task<ActionResult<dynamic>> GenerateNewPassword([FromBody] MobileCreds mobileCreds)
        {
            try
            {
                string Mobile = WebUtility.UrlDecode(mobileCreds.MobileNo);
                string Pass = WebUtility.UrlDecode(mobileCreds.Password);
                OTP respOTP = new OTP();


                string encodedPassword = "", decodedPassword = "";
                encodedPassword = EncodeDecode.EncodePasswordToBase64(Pass);

                //decodedPassword = EncodeDecode.DecodeFrom64(encodedPassword);

                mobileCreds.Password = encodedPassword;

                var result = await empLoginRepository.GenerateNewPassword(mobileCreds.MobileNo, mobileCreds.Password);
                if (result == null)
                    return NotFound();

                if (result.FirstOrDefault().IsValid != "true")
                {
                    return Ok(new { StatusCode = 400, IsSuccess = "false", Message = result.FirstOrDefault().Message, Data = new { } });
                }

                return Ok(new { StatusCode = Response.StatusCode, IsSuccess = "true", Message = result.FirstOrDefault().Message, Data = result.FirstOrDefault() });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());

            }
            finally
            {

            }
        }

        [HttpPost("GetLoginUserNames")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLoginUserNames(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                var result = await empLoginRepository.GetLoginUserNames(loginId_EmpId.LoginId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetLoginAsUserCreds")]
        //[Authorize]
        public async Task<ActionResult<dynamic>> GetLoginAsUserCreds(LoginAs_AdmMob_UsrNm loginAs_AdmMob_UsrNm)
        {
            try
            {
                var result = await empLoginRepository.GetLoginAsUserCreds(loginAs_AdmMob_UsrNm.AdminMobileNo, loginAs_AdmMob_UsrNm.UserName);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("PostIssue")]
        //[Authorize]
        public async Task<IActionResult> PostIssue([FromBody] IssueReportDto issue)
        {
            try
            {
                if (issue == null || string.IsNullOrWhiteSpace(issue.ScreenName))
                {
                    return BadRequest("Invalid data");
                }

                var result = await empLoginRepository.ReportIssueAsync(issue);

                if (result == null)
                    return NotFound();

                if (result)
                    return Ok(new { message = "Issue reported successfully" });

                return StatusCode(500, "Failed to report issue");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("ForceUpdateYN")]
        public async Task<ActionResult<ForceUpdateYN>> ForceUpdateYN()
        {
            try
            {
                var result = await empLoginRepository.ForceUpdateYN();
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {

            }
        }
    }
}
