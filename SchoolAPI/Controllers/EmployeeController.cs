using Microsoft.AspNetCore.Mvc;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models.Repository;
using static VHEmpAPI.Shared.CommonProcOutputFields;
using VHMobileAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using VHEmpAPI.Shared;

namespace VHEmpAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository? employeeRepository;
        private readonly IJwtAuth jwtAuth;
        private readonly IConfiguration _configuration;
        public string Message = "";
        //private readonly string _basePath = @"C:\inetpub\wwwroot\VHMobileAPI\Uploads";
        //private readonly string _basePath = @"C:\inetpub\wwwroot\VHTestEmpAPI\Uploads";
        private readonly string _basePath;
        private readonly string _voicePath;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(IEmployeeRepository employeeRepository, IJwtAuth jwtAuth, IConfiguration configuration, IWebHostEnvironment env)
        {
            this.employeeRepository = employeeRepository;
            this.jwtAuth = jwtAuth;
            _configuration = configuration;
            _basePath = _configuration["FileUploadSettings:BasePath"] ?? @"C:\inetpub\wwwroot";
            //_basePath = @"\\192.168.1.36\vh_data\CustomNotification";
            _voicePath = _configuration["FileUploadSettings:VoicePath"] ?? @"C:\inetpub\wwwroot";
            _env = env;
        }

        [HttpPost("UploadPatientVoiceNote")]
        //public async Task<IActionResult> UploadPatientVoiceNote([FromForm] VoiceNoteFields voiceNoteFields, IFormFile voiceFile)
        public async Task<IActionResult> UploadPatientVoiceNote([FromForm] VoiceNoteFields voiceNoteFields, [FromForm] IFormFile voiceFile)
        {
            if (voiceFile == null || voiceFile.Length == 0)
                return BadRequest("No file uploaded.");

            if (String.IsNullOrEmpty(voiceNoteFields?.IPDNo))
                return BadRequest("IPDNo is compulsory.");

            if (String.IsNullOrEmpty(voiceNoteFields?.UHID))
                return BadRequest("UHID is compulsory.");

            string IPDNo = voiceNoteFields?.IPDNo.Replace("/", "_");
            string UHID = voiceNoteFields?.UHID.Replace("/", "_");

            // Optional: validate file type
            string[] allowedExtensions = { ".mp3", ".wav", ".m4a" };
            string extension = Path.GetExtension(voiceFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type.");
            }

            string abc = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            // Create patient-specific folder
            string PatientFolder = Path.Combine(_voicePath, IPDNo);
            //PatientFolder = _voicePath;
            if (!Directory.Exists(PatientFolder))
            {
                Directory.CreateDirectory(PatientFolder);
            }
            else
            {
                //// Delete all existing files in the folder
                //var files = Directory.GetFiles(PatientFolder);
                //foreach (var existingFile in files)
                //{
                //    System.IO.File.Delete(existingFile);
                //}
            }

            // Generate unique file name
            string fileName = $"{Path.GetFileName(voiceFile.FileName)}";
            voiceNoteFields.VoiceFileName = fileName;
            string filePath = Path.Combine(PatientFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await voiceFile.CopyToAsync(stream);
            }

            var IsValidMobile = await employeeRepository.Save_DoctorVoiceNote(voiceNoteFields);

            string IsValid = "";
            if (IsValidMobile != null && IsValidMobile.Count() > 0)
            {
                IsValid = IsValidMobile.Select(x => x.IsValid).ToList()[0].ToString();

                if (IsValid.ToUpper() != "TRUE")
                {
                    return Ok(new { statusCode = Ok(IsValidMobile).StatusCode, isSuccess = "false", message = "Invalid", data = new { } });
                }

                else if (IsValid.ToUpper() == "TRUE")
                {
                    return Ok(new { statusCode = Ok(IsValid).StatusCode, isSuccess = "true", message = "Successful", data = new { } });
                }
            }

            return Ok(new { Path = filePath });
        }

        #region Login authentication and AutoLogin to Dashboard

        [HttpPost("authentication")]
        public async Task<ActionResult<dynamic>> Authentication([FromBody] MobileCreds mobileCreds)
        {
            if (mobileCreds.OTP == "" && mobileCreds.Password == "")
            {
                return Ok(new { statusCode = 401, isSuccess = "false", message = "Both Password and OTP cannot be empty!", data = new { } });
            }

            if (mobileCreds.OTP != "" && mobileCreds.Password != "")
            {
                return Ok(new { statusCode = 401, isSuccess = "false", message = "Any one from Password or OTP should be blank!", data = new { } });
            }

            if (mobileCreds.MobileNo != "" && mobileCreds.OTP != "" && mobileCreds.Password == "")
            {
                //SaveTokens_UserCreds saveTokens_UserCreds = new SaveTokens_UserCreds();
                //saveTokens_UserCreds.MobileNo = mobileCreds.MobileNo;

                DashBoardList dashboardList = new DashBoardList();
                dashboardList = await Save_Get_Token(mobileCreds);
                if (dashboardList != null)
                {
                    if (dashboardList.is_valid_token != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }

                    return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardList });
                }

                return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "false", message = "Bad Request", data = new { } });
            }

            #region password logic commented

            else if (mobileCreds.MobileNo != "" && mobileCreds.OTP == "" && mobileCreds.Password != "")
            {
                TokenData tokenData = new TokenData();

                string encodedPassword = "";
                //mobileCreds.Password = EncodeDecode.DecodeFrom64(mobileCreds.Password);
                mobileCreds.Password = EncodeDecode.EncodePasswordToBase64(mobileCreds.Password);

                var IsValidMobile = await employeeRepository.ValidateMobile_Pass(mobileCreds);

                string IsValid = "", TokenYN = "N";
                if (IsValidMobile != null && IsValidMobile.Count() > 0)
                {
                    IsValid = IsValidMobile.Select(x => x.IsValidCreds).ToList()[0].ToString();
                    TokenYN = IsValidMobile.Select(x => x.TokenNo).ToList()[0].ToString();

                    if (IsValid.ToUpper() != "TRUE")
                    {
                        tokenData.IsValidCreds = IsValid;
                        return Ok(new { statusCode = Ok(IsValidMobile).StatusCode, isSuccess = "false", message = "Invalid MobileNo or Password", data = new { } });
                    }

                    else if (IsValid.ToUpper() == "TRUE")
                    {
                        DashBoardList dashboardList = new DashBoardList();
                        dashboardList = await Save_Get_Token(mobileCreds);
                        if (dashboardList != null)
                        {
                            if (dashboardList.is_valid_token != "Y")
                            {
                                return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                            }

                            return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardList });
                        }

                        return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "false", message = "Bad Request", data = new { } });
                    }
                }
            }

            #endregion

            return new EmptyResult();
        }

        [HttpPost("Save_Get_Token")]
        public async Task<DashBoardList> Save_Get_Token([FromBody] MobileCreds mobileCreds)
        {
            //TokenData tokenData = new TokenData();

            var Token = jwtAuth.Authentication();
            if (Token == null)
            {
                //tokenData.TokenNo = "";
                //return NotFound();
                return null;
            }

            //Call procedure with mobile, Token and OTPValidYN Y and save in table
            string IsValidToken = "", flag = "", LoginId = "";
            var ReturnToken = await employeeRepository.Save_Token_UserCreds_and_ReturnToken(mobileCreds, Token);
            if (ReturnToken != null && ReturnToken.Count() > 0)
            {
                IsValidToken = ReturnToken.Select(x => x.TokenNo).ToList()[0].ToString();
                LoginId = ReturnToken.Select(x => x.LoginId).ToList()[0].ToString();
                if (String.IsNullOrEmpty(IsValidToken) || IsValidToken == "N")
                {
                    //return NotFound();
                    return null;
                }
            }

            //var dashboardData = await DisplayDashboardList(Token, mobileCreds.MobileNo);
            //var dashboardData = await DisplayDashboardList(Token, LoginId);
            var dashboardData = await DisplayDashboardList(IsValidToken, LoginId, "");
            if (dashboardData != null)
            {
                return dashboardData;
            }

            return dashboardData;
        }

        [HttpPost("GetDashboardList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetDashboardList([FromBody] LoginId_FirebaseId loginIdNum)
        {
            var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string Token = WebUtility.UrlDecode(tokenNum);
            if (tokenNum != "")
            {
                var dashboardData = await DisplayDashboardList(tokenNum, loginIdNum.LoginId, loginIdNum.FirebaseId);
                if (dashboardData != null)
                {
                    if (dashboardData.is_valid_token != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }

                    return Ok(new { statusCode = Ok(dashboardData).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardData });
                }

                return Ok(new { statusCode = 400, isSuccess = "false", message = "Bad Request", data = new { } });
            }

            return new EmptyResult();
        }

        [HttpGet("DisplayDashboardList")]
        public async Task<DashBoardList> DisplayDashboardList(string Token, string LoginId, string FirebaseId)
        {
            try
            {
                string IsValid = "";
                string TokenNum = WebUtility.UrlDecode(Token);
                var isValidToken = await employeeRepository.IsTokenValid(TokenNum, LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return new DashBoardList { is_valid_token = "N" };
                    }
                }

                await employeeRepository.UpdateFirebaseId(LoginId, FirebaseId);

                var result = await employeeRepository.DisplayDashboardList(TokenNum, LoginId);
                if (result == null)
                {
                    return null;
                }

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
            }
        }

        #endregion

        #region Payroll Module


        #region Attendance Summary and Detail and Mispunch logic

        [HttpPost("GetMonthYr_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Ddl_Value_Nm>> GetMonthYr_EmpInfo(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetMonthYr_EmpInfo();
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

        [HttpPost("GetMisPunchDtl_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetMisPunchDtl_EmpInfo(EmpId, mispunchDtl_EmpInfo);
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

        [HttpPost("GetEmpAttendDtl_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_AttDtl_EmpInfo>> GetEmpAttendDtl_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpAttendanceDtl_EmpInfo(EmpId, mispunchDtl_EmpInfo);
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

        [HttpPost("GetEmpAttendSumm_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_AttSumm_EmpInfo>> GetEmpAttendSumm_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpAttDtl_Summ(EmpId, mispunchDtl_EmpInfo);
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

        #endregion

        #region Emp Summary Dashboard

        [HttpPost("GetEmpSummary_Dashboard")]
        [Authorize]
        public async Task<ActionResult<ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpSummary_Dashboard(EmpId, loginIdNum);
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

        #endregion

        #region Leave and Overtime Entries

        [HttpPost("GetAvlLvCount")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveDays(GetLeaveDays getLeaveDays)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, getLeaveDays.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveDays(EmpId, getLeaveDays);
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

        [HttpPost("GetLeaveNames")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveNames(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveNames(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("GetLeaveReason")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveReason(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveReason(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("GetLeaveDelayReason")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveDelayReason(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveDelayReason(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("EmpApp_GetLeaveRelieverNm")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetLeaveRelieverNm(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetLeaveRelieverNm(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("EmpApp_GetLeaveEntryList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetLeaveEntryList(LoginId_EmpId_Flag loginId_EmpId_Flag)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_Flag.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetLeaveEntryList(EmpId, loginId_EmpId_Flag.LoginId, loginId_EmpId_Flag.Flag);
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

        [HttpPost("EmpApp_GetHeaderList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetHeaderList(LoginId_EmpId_Flag loginId_EmpId_Flag)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_Flag.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetHeaderList(EmpId, loginId_EmpId_Flag.LoginId, loginId_EmpId_Flag.Flag);
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

        [HttpPost("EmpApp_SaveLeaveEntryList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_SaveLeaveEntryList(SaveLeaveEntry saveLeaveEntry)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, saveLeaveEntry.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_SaveLeaveEntryList(EmpId, saveLeaveEntry);
                // Check if result is null
                if (result == null)
                    return NotFound(new { statusCode = 404, IsSuccess = "false", Message = "No data found!" });

                // Check if result is empty or status code is not 200
                if (Ok(result).StatusCode != 200 || !result.Any())
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                // Get the SavedYN value from the result (assuming first entry has SavedYN)
                var output = result.FirstOrDefault()?.SavedYN;

                // Check if output is not "Y" (indicating an error or non-success response)
                if (output != "Y")
                {
                    // If \r\n exists in output, get the text before it
                    string finalMessage = output.Contains("\r\n") ? output.Substring(0, output.IndexOf("\r\n")) : output;

                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = finalMessage, data = new { } });
                }

                // Return success response if everything is fine
                return Ok(new { statusCode = 200, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return Ok(new { statusCode = 400, IsSuccess = "false", Message = ex.Message, data = new { } });
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        #endregion

        #region Duty Schedule

        [HttpPost("GetShiftWeekList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetShiftWeekList(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetShiftWeekList(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("GetEmpShiftReport")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetEmpShiftReport([FromBody] LoginId_EmpId_DtRange loginId_EmpId_DtRange)
        {
            //DataSet ds = new DataSet();
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_DtRange.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                DataTable result = await employeeRepository.GetEmpShiftReport(EmpId, loginId_EmpId_DtRange.LoginId, loginId_EmpId_DtRange.DtRange);
                if (result == null || result.Rows.Count == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                JArray JsonInnerObjList = new JArray();
                JObject OutputJsonResult = new JObject();

                // Loop through each row in DataTable
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    JObject innerJsonResult = new JObject
                    {
                        ["Code"] = result.Rows[i]["Code"].ToString(),
                        ["Name"] = result.Rows[i]["Name"].ToString(),
                        ["SubDepartment"] = result.Rows[i]["SubDepartment"].ToString(),
                    };

                    JArray DateColumnsValue = new JArray();
                    //JArray dateValues = new JArray();
                    string dayName = "", formattedDate = "", todayDate = "false";
                    foreach (DataColumn column in result.Columns)
                    {
                        // Skip "Code" and "Name" columns
                        if (column.ColumnName != "Code" && column.ColumnName != "Name" && column.ColumnName != "SubDepartment")
                        {
                            if (DateTime.TryParseExact(column.ColumnName, "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                //if (
                                //     (column.ColumnName.ToLower().Contains("dec") || column.ColumnName.ToLower().Contains("nov"))
                                //     && DateTime.Today.Month == 1
                                //   )
                                //{
                                //    // Adjust year to the previous year if December and current month is January
                                //    date = new DateTime(DateTime.Today.Year - 1, date.Month, date.Day);
                                //}

                                // Extract the day name
                                dayName = date.ToString("dddd", CultureInfo.InvariantCulture);

                                // Format the date to "dd MMM"
                                formattedDate = date.ToString("dd\nMMM");

                                if (date.Date <= DateTime.Today)
                                    todayDate = "true";
                                else
                                    todayDate = "false";
                            }
                            // Add date column name and its value
                            DateColumnsValue.Add(new JObject
                            {
                                ["Name"] = formattedDate,
                                ["Value"] = dayName + "\n" + result.Rows[i][column].ToString(),
                                ["ActiveYN"] = todayDate,
                            });
                        }

                        //if (column.ColumnName != "Code" && column.ColumnName != "Name")
                        //{
                        //    // Add date column name and its value
                        //    dateValues.Add(new JObject
                        //    {
                        //        ["Value"] = result.Rows[i][column].ToString()
                        //    });
                        //}
                    }

                    // Add dateValues array to innerJsonResult under "DateData"
                    innerJsonResult["DateColumnsValue"] = DateColumnsValue;
                    //innerJsonResult["DateValues"] = dateValues;
                    JsonInnerObjList.Add(innerJsonResult);
                }

                var FinalOutput = new JObject
                {
                    ["statusCode"] = Ok(result).StatusCode,
                    ["IsSuccess"] = "true",
                    ["Message"] = "Data fetched successfully",
                    ["data"] = JsonInnerObjList
                };

                return Ok(FinalOutput.ToString(Newtonsoft.Json.Formatting.None));

                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonDt });
                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonResult.ToString(Formatting.None) });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        #endregion

        #region LV/OT Approval System

        [HttpPost("EmpApp_Get_LV_OT_Roles")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_Get_LV_OT_Roles(LoginId_EmpId_Lv_OT_Flag loginId_EmpId_Lv_OT_Flag)
        {
            try
            {
                string IsValid = "", EmpId = "", DefaultRole = "", InchargeYN = "", HODYN = "", HRYN = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_Lv_OT_Flag.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_Get_LV_OT_RolesList(EmpId, loginId_EmpId_Lv_OT_Flag.LoginId, loginId_EmpId_Lv_OT_Flag.Role, loginId_EmpId_Lv_OT_Flag.Flag);
                if (result == null)
                    return NotFound();

                Resp_LV_OT_RolesRights result_rights = new Resp_LV_OT_RolesRights();
                result_rights = await EmpApp_Get_LV_OT_Role_Rights(loginId_EmpId_Lv_OT_Flag);
                if (Ok(result).StatusCode == 200 || result.Count() > 0)
                {
                    if (result_rights != null)
                    {
                        // You can return all the properties of result_rights
                        DefaultRole = result_rights.DefaultRole ?? "InCharge";
                        InchargeYN = result_rights.InchargeYN ?? "N";
                        HODYN = result_rights.HODYN ?? "N";
                        HRYN = result_rights.HRYN ?? "N";
                    }
                    else
                    {
                        // If result_rights is null, return default values for each property
                        DefaultRole = "";
                        InchargeYN = "N";
                        HODYN = "N";
                        HRYN = "N";
                    }
                }

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", DefaultRole = DefaultRole, InchargeYN = InchargeYN, HODYN = HODYN, HRYN = HRYN, data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", DefaultRole = DefaultRole, InchargeYN = InchargeYN, HODYN = HODYN, HRYN = HRYN, data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("EmpApp_Get_LV_OT_Role_Rights")]
        public async Task<Resp_LV_OT_RolesRights> EmpApp_Get_LV_OT_Role_Rights(LoginId_EmpId_Lv_OT_Flag loginId_EmpId_Lv_OT_Flag)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_Lv_OT_Flag.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return new Resp_LV_OT_RolesRights
                        {
                            DefaultRole = "",
                            InchargeYN = "N",
                            HODYN = "N",
                            HRYN = "N"
                        };
                    }
                }

                var result = await employeeRepository.EmpApp_Get_LV_OT_Role_Rights(EmpId, loginId_EmpId_Lv_OT_Flag.LoginId);
                if (result == null || !result.Any())
                {
                    return new Resp_LV_OT_RolesRights
                    {
                        DefaultRole = "",
                        InchargeYN = "N",
                        HODYN = "N",
                        HRYN = "N"
                    };
                }

                var result_rights = result.FirstOrDefault(); // Adjust according to how you want to handle multiple results

                return new Resp_LV_OT_RolesRights
                {
                    DefaultRole = result_rights?.DefaultRole ?? "",
                    InchargeYN = result_rights?.InchargeYN ?? "N",
                    HODYN = result_rights?.HODYN ?? "N",
                    HRYN = result_rights?.HRYN ?? "N"
                };

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data: " + ex.Message);
            }
            finally
            {
            }
        }

        [HttpPost("EmpApp_Appr_Rej_LV_OT_Entry")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_Appr_Rej_LV_OT_Entry(Upd_Lv_OT_entry upd_Lv_OT_entry)
        {
            //var transaction = await employeeRepository.BeginTransaction();
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, upd_Lv_OT_entry.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                //var result = await employeeRepository.EmpApp_Upd_LV_OT_Entry(EmpId, upd_Lv_OT_entry, transaction);
                var result = await employeeRepository.EmpApp_Upd_LV_OT_Entry(EmpId, upd_Lv_OT_entry);
                //if (result == null)
                //    return NotFound();

                if (result == null)
                {
                    // If processing fails for any record, roll back the transaction
                    //await employeeRepository.RollbackTransaction(transaction);
                    return Ok(new { statusCode = 400, isSuccess = "false", message = "Failed to process one or more records", data = new { } });
                }

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                //await employeeRepository.CommitTransaction(transaction);

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                //await employeeRepository.RollbackTransaction(transaction);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
                //if (transaction != null)
                //{
                //    await employeeRepository.DisposeTransaction(transaction);
                //}
            }
        }

        [HttpPost("EmpApp_Appr_Rej_LV_OT_Entry_List")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_Appr_Rej_LV_OT_Entry_List(LoginId_Lst_LVOT LVOTList)
        {
            //var transaction = await employeeRepository.BeginTransaction(); // Begin Transaction
            try
            {
                string IsValid = "", EmpId = "";

                // Validate token
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, LVOTList.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).FirstOrDefault()?.ToString();
                    EmpId = isValidToken.Select(x => x.UserId).FirstOrDefault()?.ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                if (LVOTList != null && LVOTList.List_Resp_LV_OT_Roles.Any())
                {
                    Upd_Lv_OT_entry upd_Lv_OT_entry = new Upd_Lv_OT_entry();
                    foreach (var modelRecord in LVOTList.List_Resp_LV_OT_Roles)
                    {
                        // Process each record in the list here
                        // You can implement the logic for each role based on the flag value
                        // Example: If flag is "Approve", you will approve the entry, else if "Reject", you will reject it.

                        // Assuming some processing method is available to handle individual records

                        upd_Lv_OT_entry = new Upd_Lv_OT_entry();
                        upd_Lv_OT_entry.LoginId = LVOTList.LoginId;
                        upd_Lv_OT_entry.EmpId = EmpId;
                        upd_Lv_OT_entry.Flag = "HOD";
                        upd_Lv_OT_entry.LeaveDetailId = modelRecord.LeaveId.ToString();
                        upd_Lv_OT_entry.Action = "Approved";
                        upd_Lv_OT_entry.Reason = modelRecord.Reason;
                        upd_Lv_OT_entry.UserName = "";
                        upd_Lv_OT_entry.Note = modelRecord.HoDNote;

                        //var result = await employeeRepository.EmpApp_Upd_LV_OT_Entry(EmpId, upd_Lv_OT_entry, transaction); // Process the individual role
                        var result = await employeeRepository.EmpApp_Upd_LV_OT_Entry(EmpId, upd_Lv_OT_entry); // Process the individual role

                        //if (result == null)
                        //{
                        //    // If processing fails for any record, roll back the transaction
                        //    await employeeRepository.RollbackTransaction(transaction);
                        //    return Ok(new { statusCode = 400, isSuccess = "false", message = "Failed to process one or more records", data = new { } });
                        //}
                    }
                }
                else
                {
                    //await employeeRepository.RollbackTransaction(transaction);
                    return Ok(new { statusCode = 400, isSuccess = "false", message = "No records to process", data = new { } });
                }

                // Commit the transaction if all records processed successfully
                //await employeeRepository.CommitTransaction(transaction);

                return Ok(new { statusCode = 200, isSuccess = "true", message = "Y", data = new { } });
            }
            catch (Exception ex)
            {
                // If exception occurs, rollback the transaction
                //await employeeRepository.RollbackTransaction(transaction);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
                //if (transaction != null)
                //{
                //    await employeeRepository.DisposeTransaction(transaction);
                //}
            }
        }

        [HttpPost("GetLeaveRejectReason")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveRejectReason(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveRejectReason(EmpId, loginId_EmpId.LoginId);
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


        #endregion


        #endregion

        #region Pharmacy Module


        #region Prescription Viewer

        [HttpPost("GetDrPrescriptionViewer")]
        [Authorize]
        public async Task<ActionResult<Resp_Dr_PrecriptionViewer>> GetDrPrescriptionViewer(FilteredPharmaPrecriptionData filteredPharmaPrecriptionData)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, filteredPharmaPrecriptionData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                //var result = await employeeRepository.EmpApp_GetDrPrescriptionViewer(EmpId, filteredPharmaPrecriptionData.LoginId);

                List<Resp_Dr_PrecriptionViewer> result = new List<Resp_Dr_PrecriptionViewer>();
                if (filteredPharmaPrecriptionData != null)
                    result = (List<Resp_Dr_PrecriptionViewer>)await employeeRepository.EmpApp_GetDrPrescriptionViewer(EmpId, filteredPharmaPrecriptionData.LoginId, filteredPharmaPrecriptionData.PrefixText, filteredPharmaPrecriptionData.Wards, filteredPharmaPrecriptionData.Floors, filteredPharmaPrecriptionData.Beds);
                else
                    result = (List<Resp_Dr_PrecriptionViewer>)await employeeRepository.EmpApp_GetDrPrescriptionViewer(EmpId, filteredPharmaPrecriptionData.LoginId, filteredPharmaPrecriptionData.PrefixText, new List<string>(), new List<string>(), new List<string>());

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

        [HttpPost("SortDr_PrecriptionViewer")]
        [Authorize]
        public async Task<ActionResult<Resp_Dr_PrecriptionViewer>> SortDr_PrecriptionViewer([FromBody] SortDr_PrecriptionViewer sortPatientData = null)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, sortPatientData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                List<Resp_Dr_PrecriptionViewer> result = new List<Resp_Dr_PrecriptionViewer>();
                if (sortPatientData != null)
                    result = (List<Resp_Dr_PrecriptionViewer>)await employeeRepository.SortDr_PrecriptionViewer(EmpId, sortPatientData.LoginId, sortPatientData.SortType);
                else
                    result = (List<Resp_Dr_PrecriptionViewer>)await employeeRepository.SortDr_PrecriptionViewer(EmpId, sortPatientData.LoginId, sortPatientData.SortType);

                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or Something went wrong!", data = new { } });

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

        [HttpPost("GetPharmaDashboardFilters")]
        [Authorize]
        public async Task<ActionResult<GetPharmaDashboardFilters>> GetDashboardFilters(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var Wards = await employeeRepository.GetWards(EmpId, loginIdNum.LoginId);
                var Floors = await employeeRepository.GetFloors(EmpId, loginIdNum.LoginId);
                var Beds = await employeeRepository.GetBeds(EmpId, loginIdNum.LoginId);

                var result = new GetPharmaDashboardFilters
                {
                    Wards = (List<Wards>)Wards,
                    Floors = (List<Floors>)Floors,
                    Beds = (List<Beds>)Beds,
                };

                if (result == null)
                    return NotFound();

                return Ok(new { statusCode = Ok(result).StatusCode, isSuccess = "true", message = "Success!", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetFloors")]
        [Authorize]
        public async Task<ActionResult<Floors>> GetFloors(LoginIdNum loginIdNum)
        {
            try
            {
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);
                var result = await employeeRepository.GetFloors(tokenNum, loginIdNum.LoginId);
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

        [HttpPost("GetWards")]
        [Authorize]
        public async Task<ActionResult<Wards>> GetWards(LoginIdNum loginIdNum)
        {
            try
            {
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);
                var result = await employeeRepository.GetWards(tokenNum, loginIdNum.LoginId);
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

        [HttpPost("GetBeds")]
        [Authorize]

        public async Task<ActionResult<Beds>> GetBeds(LoginIdNum loginIdNum)
        {
            try
            {
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);
                var result = await employeeRepository.GetWards(tokenNum, loginIdNum.LoginId);
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

        #endregion


        #region Prescription Medicines

        [HttpPost("GetDrPrescriptionMedicines")]
        [Authorize]
        public async Task<ActionResult<Resp_Dr_PrecriptionMedicines>> GetDrPrescriptionMedicines(LoginId_EmpId_PresMed loginId_PrecMed)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_PrecMed.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetDrPrescriptionMedicines(EmpId, loginId_PrecMed.LoginId, loginId_PrecMed.MstId);
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

        #endregion


        #endregion       

        [HttpPost("GetDrNotifications")]
        [Authorize]
        public async Task<ActionResult<DoctorNotification>> GetDrNotifications([FromBody] LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetDrNotifications(loginIdNum.LoginId, EmpId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or no data found!", data = new { } });

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



        #region Module & Screen Rights

        [HttpPost("GetModuleRights")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetModuleRights(LoginId_EmpId_ModuleNm loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetModuleRights(EmpId, loginId_EmpId.LoginId, loginId_EmpId.ModuleName);
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

        [HttpPost("GetEmpAppScreenRights")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetEmpAppScreenRights(LoginId_EmpId_ModuleNm loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpAppScreenRights(EmpId, loginId_EmpId.LoginId, loginId_EmpId.ModuleName);
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

        #endregion

        #region IPD Module


        #region Admitted Patients List

        [HttpPost("GetPatientDashboardFilters")]
        [Authorize]
        public async Task<ActionResult<GetDashboardFilters>> GetPatientDashboardFilters(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var Orgs = await employeeRepository.GetOrganizations(EmpId, loginIdNum.LoginId);
                var Floors = await employeeRepository.GetFloors(EmpId, loginIdNum.LoginId);
                var Wards = await employeeRepository.GetWards(EmpId, loginIdNum.LoginId);

                var result = new GetDashboardFilters
                {
                    Orgs = (List<Organizations>)Orgs,
                    Floors = (List<Floors>)Floors,
                    Wards = (List<Wards>)Wards,
                };

                if (result == null)
                    return NotFound();

                return Ok(new { statusCode = Ok(result).StatusCode, isSuccess = "true", message = "Success!", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetOrganizations")]
        [Authorize]
        public async Task<ActionResult<Organizations>> GetOrganizations(LoginIdNum loginIdNum)
        {
            try
            {
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);
                var result = await employeeRepository.GetOrganizations(tokenNum, loginIdNum.LoginId);
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


        [HttpPost("GetFilteredPatientData")]
        [Authorize]
        public async Task<ActionResult<PatientList>> GetFilteredPatientData([FromBody] FilteredPatientData filteredPatientData = null)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, filteredPatientData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                List<PatientList> result = new List<PatientList>();
                if (filteredPatientData != null)
                    result = (List<PatientList>)await employeeRepository.GetFilteredPatientData(EmpId, filteredPatientData.LoginId, filteredPatientData.PrefixText, filteredPatientData.Orgs, filteredPatientData.Floors, filteredPatientData.Wards);
                else
                    result = (List<PatientList>)await employeeRepository.GetFilteredPatientData(EmpId, filteredPatientData.LoginId, filteredPatientData.PrefixText, new List<string>(), new List<string>(), new List<string>());

                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or Something went wrong!", data = new { } });

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

        [HttpPost("SortDeptPatientList")]
        [Authorize]
        public async Task<ActionResult<PatientList>> SortDeptPatientList([FromBody] SortDrPatientData sortPatientData = null)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, sortPatientData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                List<PatientList> result = new List<PatientList>();
                if (sortPatientData != null)
                    result = (List<PatientList>)await employeeRepository.SortDeptPatientList(EmpId, sortPatientData.LoginId, sortPatientData.SortType);
                else
                    result = (List<PatientList>)await employeeRepository.SortDeptPatientList(EmpId, sortPatientData.LoginId, sortPatientData.SortType);

                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or Something went wrong!", data = new { } });

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

        #endregion

        #region Lab Reports

        [HttpPost("GetPatientLabReports")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetPatientLabReports([FromBody] DrRadiologyData drRadiologyData)
        {
            //DataSet ds = new DataSet();
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, drRadiologyData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                DataSet result = await employeeRepository.GetPatientLabReports(EmpId, drRadiologyData.IpdNo, drRadiologyData.UHID);
                if (result == null)
                    return NotFound();

                JArray JsonInnerObjList = new JArray();
                var jsonResult = new JObject();
                var innerjsonResult = new JObject();
                var OutputJsonResult = new JObject();

                //for (int i = 1; i < result.Tables.Count; i++)
                //{
                //    //string tableName = $"Table{i + 1}";
                //    string tableName = result.Tables[i].Rows.Count > 0 ? result.Tables[i].Rows[0]["FormatTest"].ToString() : $"Table{i + 1}";
                //    DataTable table = result.Tables[i];
                //    jsonResult[tableName] = JToken.FromObject(table);
                //    //ds.Tables.Add(table);
                //}

                //OutputJsonResult["statusCode"] = Ok(result).StatusCode;
                //OutputJsonResult["IsSuccess"] = "true";
                //OutputJsonResult["Message"] = "Data fetched successfully";
                //OutputJsonResult["data"] = jsonResult;

                for (int i = 1; i < result.Tables.Count; i++)
                {
                    //string tableName = $"Table{i + 1}";
                    string tableName = result.Tables[i].Rows.Count > 0 ? result.Tables[i].Rows[0]["FormatTest"].ToString() : $"Table{i + 1}";
                    DataTable table = result.Tables[i];

                    innerjsonResult = new JObject
                    {
                        ["report_name"] = tableName,
                        ["data"] = JToken.FromObject(table)
                    };
                    JsonInnerObjList.Add(innerjsonResult);
                    //ds.Tables.Add(table);
                }

                if (result.Tables.Count > 0)
                {
                    OutputJsonResult = new JObject
                    {
                        ["PatientName"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["PatientName"].ToString() : "",
                        ["BedNo"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["BedNo"].ToString() : "",
                        ["Data"] = JsonInnerObjList
                    };
                }

                var FinalOutput = new JObject
                {
                    ["statusCode"] = Ok(result).StatusCode,
                    ["IsSuccess"] = "true",
                    ["Message"] = "Data fetched successfully",
                    ["data"] = OutputJsonResult
                };

                //string jsonDt = JsonConvert.SerializeObject(result);
                if (Ok(result).StatusCode != 200 || result.Tables.Count == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(FinalOutput.ToString(Formatting.None));

                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonDt });
                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonResult.ToString(Formatting.None) });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        #endregion

        #region Lab Summary

        [HttpPost("GetPatientSummaryLabData")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetPatientSummaryLabData([FromBody] DrRadiologyData drRadiologyData)
        {
            //DataSet ds = new DataSet();
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, drRadiologyData.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                DataSet result = await employeeRepository.GetPatientSummaryLabData(EmpId, drRadiologyData.IpdNo, drRadiologyData.UHID);
                if (result == null)
                    return NotFound();

                JArray JsonInnerObjList = new JArray();
                var jsonResult = new JObject();
                var innerjsonResult = new JObject();
                var OutputJsonResult = new JObject();

                //for (int i = 1; i < result.Tables.Count; i++)
                //{
                //    //string tableName = $"Table{i + 1}";
                //    string tableName = result.Tables[i].Rows.Count > 0 ? result.Tables[i].Rows[0]["FormatTest"].ToString() : $"Table{i + 1}";
                //    DataTable table = result.Tables[i];
                //    jsonResult[tableName] = JToken.FromObject(table);
                //    //ds.Tables.Add(table);
                //}

                //OutputJsonResult["statusCode"] = Ok(result).StatusCode;
                //OutputJsonResult["IsSuccess"] = "true";
                //OutputJsonResult["Message"] = "Data fetched successfully";
                //OutputJsonResult["data"] = jsonResult;

                for (int i = 1; i < result.Tables.Count; i++)
                {
                    //string tableName = $"Table{i + 1}";
                    string tableName = result.Tables[i].Rows.Count > 0 ? result.Tables[i].Rows[0]["FormatTest"].ToString() : $"Table{i + 1}";
                    DataTable table = result.Tables[i];
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        string colName = table.Columns[j].ColumnName.ToLower(); // Convert to lowercase for comparison

                        if (colName == "day_0")
                            table.Columns[j].ColumnName = DateTime.Now.ToString("dd-MM-yyyy"); // Aaj ki date
                        else if (colName == "day_1")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                        else if (colName == "day_2")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-2).ToString("dd-MM-yyyy");
                        else if (colName == "day_3")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-3).ToString("dd-MM-yyyy");
                        else if (colName == "day_4")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-4).ToString("dd-MM-yyyy");
                        else if (colName == "day_5")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-5).ToString("dd-MM-yyyy");
                        else if (colName == "day_6")
                            table.Columns[j].ColumnName = DateTime.Now.AddDays(-6).ToString("dd-MM-yyyy");
                    }

                    innerjsonResult = new JObject
                    {
                        //["report_name"] = tableName,
                        ["PatientName"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["PatientName"].ToString() : "",
                        ["BedNo"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["BedNo"].ToString() : "",
                        ["LabData"] = JToken.FromObject(table)
                    };
                    JsonInnerObjList.Add(innerjsonResult);
                    //ds.Tables.Add(table);
                }

                //if (result.Tables.Count > 0)
                //{
                //    OutputJsonResult = new JObject
                //    {
                //        ["PatientName"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["PatientName"].ToString() : "",
                //        ["BedNo"] = result.Tables[0].Rows.Count > 0 ? result.Tables[0].Rows[0]["BedNo"].ToString() : "",
                //        //["ReportDetails"] = JsonInnerObjList
                //    };
                //}

                var FinalOutput = new JObject
                {
                    ["statusCode"] = Ok(result).StatusCode,
                    ["IsSuccess"] = "true",
                    ["Message"] = "Data fetched successfully",
                    //["data"] = OutputJsonResult
                    ["data"] = innerjsonResult
                };

                //string jsonDt = JsonConvert.SerializeObject(result);
                if (Ok(result).StatusCode != 200 || result.Tables.Count == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(FinalOutput.ToString(Formatting.None));

                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonDt });
                //return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = jsonResult.ToString(Formatting.None) });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        #endregion

        #region Investigation Requisition


        [HttpPost("EmpApp_GetExternalLabName")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetExternalLabName(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetExternalLabName(EmpId, loginId_EmpId.LoginId);
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

        [HttpPost("EmpApp_InvReq_GetServiceGrp")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_GetServiceGrp(LoginId_EmpId_Flag loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_GetServiceGrp(EmpId, loginId_EmpId.LoginId, loginId_EmpId.Flag);
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

        [HttpPost("EmpApp_InvReq_SearchService")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_SearchService(LoginId_EmpId_Flag loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_SearchService(EmpId, loginId_EmpId.LoginId, loginId_EmpId.Flag);
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

        [HttpPost("EmpApp_InvReq_SearchDrName")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_SearchDrName(LoginId_EmpId_SrchDr loginId_EmpId_SrchDr)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_SrchDr.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_SearchDrName(EmpId, loginId_EmpId_SrchDr.LoginId, loginId_EmpId_SrchDr.SearchText, loginId_EmpId_SrchDr.Srv);
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


        [HttpPost("EmpApp_InvReq_Get_Query")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_Get_Query(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, invReq_Get_Query.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_Get_Query(invReq_Get_Query);
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


        [HttpPost("SaveRequestSheetIPD")]
        public async Task<IActionResult> SaveRequestSheetIPD([FromBody] RequestSheetIPD request)
        {
            try
            {
                var savedMaster = await employeeRepository.SaveRequestSheetIPD_Dapper(request);

                if (savedMaster.LabDetail.Count > 0)
                {
                    foreach (var detail in savedMaster.LabDetail)
                    {
                        detail.MReqId = savedMaster.ReqId;
                        detail.Bill_Detail_Id = savedMaster.Bill_Detail_Id;
                        detail.Dr_Inst_Id = savedMaster.Dr_Inst_Id;
                    }

                    var detailsStatus = await employeeRepository.SaveRequestSheetDetailsIPD_Dapper(savedMaster.LabDetail);
                }
                else if (savedMaster.RadioDetail.Count > 0)
                {
                    foreach (var detail in savedMaster.RadioDetail)
                    {
                        detail.MReqId = savedMaster.ReqId;
                        detail.Bill_Detail_Id = savedMaster.Bill_Detail_Id;
                        detail.Dr_Inst_Id = savedMaster.Dr_Inst_Id;
                    }

                    var detailsStatus = await employeeRepository.SaveRequestSheetDetailsIPD_Dapper(savedMaster.RadioDetail);
                }
                else if (savedMaster.OtherDetail.Count > 0)
                {
                    foreach (var detail in savedMaster.OtherDetail)
                    {
                        detail.MReqId = savedMaster.ReqId;
                        detail.Bill_Detail_Id = savedMaster.Bill_Detail_Id;
                        detail.Dr_Inst_Id = savedMaster.Dr_Inst_Id;
                    }

                    var detailsStatus = await employeeRepository.SaveRequestSheetDetailsIPD_Dapper(savedMaster.OtherDetail);
                }
                else
                    return StatusCode(500, new { status = "error", message = savedMaster.ReqId });

                return Ok(new
                {
                    status = "success",
                    reqId = savedMaster.ReqId,
                    drInstId = savedMaster.Dr_Inst_Id,
                    billDetailId = savedMaster.Bill_Detail_Id
                });
            }
            catch (Exception ex)
            {
                string originalMessage = ex.Message;
                int index = originalMessage.ToLower().IndexOf("process id");
                string cleanMessage = index >= 0 ? originalMessage.Substring(0, index).Trim() : originalMessage;
                //return StatusCode(500, new { status = "error", message = cleanMessage });
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = 500,
                    isSuccess = "false",
                    message = "Something went wrong: \n" + cleanMessage,
                    data = new { }
                });
            }
        }

        [HttpPost("EmpApp_InvReq_Get_HistoryData")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_Get_HIstoryData(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, invReq_Get_Query.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_Get_HIstoryData(invReq_Get_Query);
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

        [HttpPost("EmpApp_InvReq_SelReq_HistoryDetail")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_InvReq_SelReq_HistoryDetail(InvReq_Get_Query invReq_Get_Query)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, invReq_Get_Query.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_InvReq_SelReq_HistoryDetail(invReq_Get_Query);
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

        [HttpPost("WebUserLoginCreds")]
        [Authorize]
        public async Task<ActionResult<dynamic>> WebUserLoginCreds([FromBody] WebEmpMobileCreds webEmpMobileCreds)
        {
            string IsValid = "", EmpId = "";
            var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string Token = WebUtility.UrlDecode(tokenNum);

            var isValidToken = await employeeRepository.IsTokenValid(tokenNum, webEmpMobileCreds.LoginId);
            if (isValidToken != null)
            {
                IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                if (IsValid != "Y")
                {
                    return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                }
            }

            if (webEmpMobileCreds.MobileNo == "" || webEmpMobileCreds.Password == "")
            {
                return Ok(new { statusCode = 401, isSuccess = "false", message = "Both MobileNo and Password are mandatory!", data = new { } });
            }

            //TokenData tokenData = new TokenData();

            string encodedPassword = "", Pass = "", hex1 = "", stringvalue = "", hexvalue = "";
            Pass = webEmpMobileCreds.Password.Trim();

            //mobileCreds.Password = EncodeDecode.DecodeFrom64(mobileCreds.Password);
            //hex1 = EncodeDecode.WebPass_Encode(webEmpMobileCreds.Password, System.Text.Encoding.Unicode);

            for (int i = 0; i < Pass.Length; i++)
            {
                int a = Convert.ToInt32(Pass[i]);
                stringvalue += a;
            }

            long intvalue = Convert.ToInt64(stringvalue);
            hexvalue = intvalue.ToString("X");
            webEmpMobileCreds.Password = hexvalue;

            //webEmpMobileCreds.Password = EncodeDecode.EncodePasswordToBase64(webEmpMobileCreds.Password);

            var result = await employeeRepository.Validate_Web_Creds(webEmpMobileCreds);

            if (result == null)
                return NotFound();

            if (Ok(result).StatusCode != 200 || result.Count() == 0)
                return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

            return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });
        }

        [HttpPost("EmpApp_Delete_InvReq_DetailSrv")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_Delete_InvReq_DetailSrv(InvReq_Del_ReqDtl invReq_Del_ReqDtl)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, invReq_Del_ReqDtl.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).FirstOrDefault()?.ToString();
                    EmpId = isValidToken.Select(x => x.UserId).FirstOrDefault()?.ToString();

                    if (IsValid != "Y")
                    {
                        return Unauthorized(new
                        {
                            statusCode = 401,
                            isSuccess = "false",
                            message = "Invalid Token!",
                            data = new { }
                        });
                    }
                }

                // Call repo method (which now does not return anything)
                await employeeRepository.EmpApp_Delete_InvReq_Detail(invReq_Del_ReqDtl);

                // Return success message
                return Ok(new
                {
                    statusCode = 200,
                    isSuccess = "true",
                    message = "Record deleted successfully",
                    data = new { }
                });
            }
            catch (Exception ex)
            {
                string originalMessage = ex.Message;
                int index = originalMessage.ToLower().IndexOf("process id");
                string cleanMessage = index >= 0 ? originalMessage.Substring(0, index).Trim() : originalMessage;
                // General error
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = 500,
                    isSuccess = "false",
                    message = "Something went wrong: \n" + cleanMessage,
                    data = new { }
                });
            }
        }


        #endregion

        #region Medication Sheet

        [HttpPost("EmpApp_GetSpecialOrderList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetSpecialOrderList(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "SPECIAL_ORDER_TYP");
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

        [HttpPost("EmpApp_GetTemplates")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetTemplates(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "MT_NAME");
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

        [HttpPost("EmpApp_GetMedicationType")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetMedicationType(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "MEDICATION TYPE");
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

        [HttpPost("EmpApp_GetInstructionType")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetInstructionType(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "INSTRUCTION TYPE");
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

        [HttpPost("EmpApp_GetDrTreatmentRoute")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetDrTreatmentRoute(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "Dr Treatment Route");
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

        [HttpPost("EmpApp_GetDrTreatmentFrequency")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetDrTreatmentFrequency(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAddMedicationDropdownData(EmpId, loginId_EmpId.LoginId, "Dr Treatment Frequency");
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

        [HttpPost("EmpApp_GetDrTreatmentMaster")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetDrTreatmentMaster(DrTreatmentMaster drTreatmentMaster)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, drTreatmentMaster.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                //var result = await employeeRepository.EmpApp_GetDrTreatmentMaster(EmpId, drTreatmentMaster.LoginId, drTreatmentMaster.IpdNo, drTreatmentMaster.TreatTyp, drTreatmentMaster.UserName);
                var result = await employeeRepository.EmpApp_GetDrTreatmentMaster("", drTreatmentMaster.LoginId, drTreatmentMaster.IpdNo, drTreatmentMaster.TreatTyp, drTreatmentMaster.UserName);
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

        [HttpPost("EmpApp_SaveDrTreatmentMaster")]
        //[Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_SaveDrTreatmentMaster([FromBody] Resp_DRTreatMaster drTreatmentMaster)
        {
            try
            {
                //string IsValid = "", EmpId = "";
                //var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                //string Token = WebUtility.UrlDecode(tokenNum);

                //var isValidToken = await employeeRepository.IsTokenValid(tokenNum, drTreatmentMaster.LoginId);
                //if (isValidToken != null)
                //{
                //    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                //    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                //    if (IsValid != "Y")
                //    {
                //        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                //    }
                //}

                var result = await employeeRepository.EmpApp_SaveDrTreatmentMaster(drTreatmentMaster, "");
                if (result == null)
                    return NotFound();

                return Ok(new
                {
                    statusCode = 200,
                    IsSuccess = "true",
                    Message = "Data saved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
        }

        [HttpPost("EmpApp_SaveAddMedicinesSheet")]
        //[Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_SaveAddMedicinesSheet([FromBody] Resp_DRTreatDetail drTreatDetail)
        {
            try
            {
                //string IsValid = "", EmpId = "";
                //var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                //string Token = WebUtility.UrlDecode(tokenNum);

                //var isValidToken = await employeeRepository.IsTokenValid(tokenNum, drTreatmentMaster.LoginId);
                //if (isValidToken != null)
                //{
                //    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                //    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                //    if (IsValid != "Y")
                //    {
                //        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                //    }
                //}

                var result = await employeeRepository.EmpApp_SaveAddMedicinesSheet(drTreatDetail, "");
                if (result == null)
                    return NotFound();

                return Ok(new
                {
                    statusCode = 200,
                    IsSuccess = "true",
                    Message = "Data saved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
        }

        [HttpPost("EmpApp_GetAdmissionIdFrmIPD")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetAdmissionIdFrmIPD(AdmissionId admissionId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, admissionId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetAdmissionIdFrmIPD(EmpId, admissionId.LoginId, admissionId.IpdNo);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200)
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

        [HttpPost("EmpApp_MedicationSheet_SearchMedicines")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_MedicationSheet_SearchMedicines(LoginId_EmpId_Flag loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_MedicationSheet_SearchMedicines(EmpId, loginId_EmpId.LoginId, loginId_EmpId.Flag);
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

        [HttpPost("EmpApp_MedicationSheet_DeleteMedicines")]
        public async Task<IActionResult> EmpApp_MedicationSheet_DeleteMedicines([FromBody] Resp_MedSheet_Del resp_MedSheet_Del)
        {
            try
            {
                var result = await employeeRepository.DeleteDoctorTreatmentDetailAsync(resp_MedSheet_Del.mstId, resp_MedSheet_Del.dtlId, resp_MedSheet_Del.UserName);

                if (result)
                {
                    return Ok(new
                    {
                        statusCode = 200,
                        IsSuccess = "true",
                        Message = "Data deleted successfully",
                        data = new { }
                    });
                }


                return NotFound(new { message = "Record not found or already deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting treatment detail.", details = ex.Message });
            }

        }

        #endregion

        #region Dietician Checklist

        [HttpPost("EMPApp_Getdata_DieticianChecklist")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EMPApp_Getdata_DieticianChecklist(DieticianChecklist_I dieticianChecklist_I)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, dieticianChecklist_I.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).FirstOrDefault()?.ToString();
                    EmpId = isValidToken.Select(x => x.UserId).FirstOrDefault()?.ToString();

                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, IsSuccess = "false", Message = "Invalid Token!", data = new { } });
                    }
                }

                //var result = await employeeRepository.EMPApp_Getdata_DieticianChecklist(EmpId, dieticianChecklist_I.LoginId);
                List<Resp_DieticianChecklist> result = new List<Resp_DieticianChecklist>();
                if (dieticianChecklist_I != null)
                    result = (List<Resp_DieticianChecklist>)await employeeRepository.EMPApp_Getdata_DieticianChecklist(EmpId, dieticianChecklist_I.LoginId, dieticianChecklist_I.PrefixText, dieticianChecklist_I.Wards, dieticianChecklist_I.Floors, dieticianChecklist_I.Beds);
                else
                    result = (List<Resp_DieticianChecklist>)await employeeRepository.EMPApp_Getdata_DieticianChecklist(EmpId, dieticianChecklist_I.LoginId, dieticianChecklist_I.PrefixText, new List<string>(), new List<string>(), new List<string>());


                if (result == null || !result.Any())
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = 200, IsSuccess = "true", Message = "Data fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("EMPApp_GetWardNm_Cnt_DieticianChecklist")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EMPApp_GetWardNm_Cnt_DieticianChecklist(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).FirstOrDefault()?.ToString();
                    EmpId = isValidToken.Select(x => x.UserId).FirstOrDefault()?.ToString();

                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, IsSuccess = "false", Message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EMPApp_GetWardNm_Cnt_DieticianChecklist(EmpId, loginId_EmpId.LoginId);

                if (result == null || !result.Any())
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = 200, IsSuccess = "true", Message = "Data fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("EmpApp_SaveDietChecklistMaster")]
        [Authorize]
        public async Task<IActionResult> EmpApp_SaveDietChecklistMaster([FromBody] DietChecklistMaster entity)
        {
            try
            {
                var savedEntity = await employeeRepository.EmpApp_SaveDietChecklistMaster(entity);
                if (savedEntity == null)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        IsSuccess = "true",
                        Message = "No Data found!",
                        data = new { }
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    IsSuccess = "true",
                    Message = "Data saved successfully",
                    data = savedEntity
                });

                //return Ok(new { statusCode = 200, IsSuccess = "true", Message = "Data saved successfully", data = savedEntity });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        #endregion


        #endregion

        #region Notification Provision

        [HttpPost("GetEMPNotificationsList")]
        [Authorize]
        public async Task<ActionResult<EMPNotificationList>> GetEMPNotificationsList([FromBody] LoginId__AdmPatients_Days_Tag loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEMPNotificationsList(loginIdNum.LoginId, EmpId, loginIdNum.Days, loginIdNum.Tag, loginIdNum.FromDate, loginIdNum.ToDate);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or no data found!", data = new { } });

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

        [HttpPost("UpdateNotification_Read")]
        [Authorize]
        public async Task<ActionResult<IsValidData>> UpdateNotification_Read([FromBody] LoginId_FileIndex loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.UpdateNotification_Read(loginIdNum.LoginId, loginIdNum.NotificationId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or no data found!", data = new { } });

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

        [HttpPost("GetNotificationFiles")]
        public async Task<IActionResult> GetNotificationFiles([FromBody] LoginId_FileIndex loginIdNum)
        {
            string IsValid = "", EmpId = "";
            var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string Token = WebUtility.UrlDecode(tokenNum);

            var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
            if (isValidToken != null)
            {
                IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                if (IsValid != "Y")
                {
                    return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                }
            }

            #region Working code to scan files

            ////string doctorFolder = Path.Combine(_basePath, "541".ToString());
            ////string doctorFolder = Path.Combine(_basePath, loginIdNum.NotificationId.ToString());
            //string doctorFolder = Path.Combine(_basePath, loginIdNum.NotificationId.ToString());


            //if (!Directory.Exists(doctorFolder))
            //{
            //    return Ok(new { statusCode = 404, isSuccess = "false", message = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString(),
            //        data = new { } });
            //}

            //// Asynchronously get files from the directory
            //var files = await Task.Run(() => Directory.GetFiles(doctorFolder));
            //if (files.Length == 0)
            //{
            //    return Ok(new { statusCode = 404, isSuccess = "false", message = "No photo found for this doctor.", data = new { } });
            //}

            //try
            //{
            //    // List to store multiple files
            //    var fileList = new List<object>();

            //    foreach (var filePath in files)
            //    {
            //        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //        var contentType = GetContentType(filePath);

            //        fileList.Add(new
            //        {
            //            fileName = Path.GetFileName(filePath),
            //            contentType = contentType,
            //            fileContent = Convert.ToBase64String(fileBytes) // Convert to Base64
            //        });
            //    }

            //    return Ok(new
            //    {
            //        statusCode = 200,
            //        isSuccess = "true",
            //        message = "Photos fetched successfully.",
            //        data = fileList
            //    });
            //}
            //catch (Exception ex)
            //{
            //    return Ok(new { statusCode = 500, isSuccess = "false", message = "An error occurred while processing the request.", data = new { error = ex.Message } });
            //}




            #endregion

            try
            {
                if (!Directory.Exists(_basePath))
                {
                    string printBasePath = _basePath;
                    return Ok(new
                    {
                        statusCode = 404,
                        isSuccess = "false",
                        message = "Base path does not exist. Identity: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + " \n basePath: " + printBasePath,
                        data = new { }
                    });
                }

                // Get all files in the base path directory
                var allFiles = await Task.Run(() => Directory.GetFiles(_basePath));

                // Filter files starting with NotificationId_
                string prefix = $"{loginIdNum.NotificationId}_";
                var matchingFiles = allFiles.Where(f => Path.GetFileName(f).StartsWith(prefix)).ToList();

                if (matchingFiles.Count == 0)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        isSuccess = "false",
                        message = $"No files found starting with '{prefix}'.",
                        data = new { }
                    });
                }

                var fileList = new List<object>();

                foreach (var filePath in matchingFiles)
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    var contentType = GetContentType(filePath);

                    fileList.Add(new
                    {
                        fileName = Path.GetFileName(filePath),
                        contentType = contentType,
                        fileContent = Convert.ToBase64String(fileBytes)
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    isSuccess = "true",
                    message = "Photos fetched successfully.",
                    data = fileList
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    statusCode = 500,
                    isSuccess = "false",
                    message = "An error occurred while processing the request.",
                    data = new { error = ex.Message }
                });
            }


            #region Old code may not work


            //// Return the files or a specific file logic
            //if (files.Length > 0)
            //{
            //    var filePath = files[0]; // Just returning the first file for simplicity
            //    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //    var contentType = GetContentType(filePath); // Get the content type based on the file extension

            //    return File(fileBytes, contentType); // Use dynamic content type
            //}

            //try
            //{
            //    // Read the first file (or apply your custom logic for selecting a file)
            //    var filePath = files[0];
            //    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //    var contentType = GetContentType(filePath);

            //    // Return the file as a base64-encoded string in the response
            //    return Ok(new
            //    {
            //        statusCode = 200,
            //        isSuccess = "true",
            //        message = "Photo fetched successfully.",
            //        data = new
            //        {
            //            fileName = Path.GetFileName(filePath),
            //            contentType = contentType,
            //            fileContent = Convert.ToBase64String(fileBytes)
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    // Handle unexpected errors gracefully
            //    return Ok(new { statusCode = 500, isSuccess = "false", message = "An error occurred while processing the request.", data = new { error = ex.Message } });
            //}

            #endregion
        }

        // Helper method to determine the content type based on the file extension
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" or ".tif" => "image/tiff",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                _ => "application/octet-stream" // Default type for unknown formats
            };
        }

        #endregion

    }
}
