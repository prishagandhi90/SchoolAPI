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

namespace VHEmpAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository? employeeRepository;
        private readonly IJwtAuth jwtAuth;
        public string Message = "";

        public EmployeeController(IEmployeeRepository employeeRepository, IJwtAuth jwtAuth)
        {
            this.employeeRepository = employeeRepository;
            this.jwtAuth = jwtAuth;
        }

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
            var dashboardData = await DisplayDashboardList(Token, LoginId);
            if (dashboardData != null)
            {
                return dashboardData;
            }

            return dashboardData;
        }

        [HttpPost("GetDashboardList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetDashboardList([FromBody] LoginIdNum loginIdNum)
        {
            var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string Token = WebUtility.UrlDecode(tokenNum);
            if (tokenNum != "")
            {
                var dashboardData = await DisplayDashboardList(tokenNum, loginIdNum.LoginId);
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
        public async Task<DashBoardList> DisplayDashboardList(string Token, string LoginId)
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
    }
}
