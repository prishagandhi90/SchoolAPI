using Microsoft.AspNetCore.Mvc;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models.Repository;
using static VHEmpAPI.Shared.CommonProcOutputFields;
using VHMobileAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;

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

            //else if (mobileCreds.MobileNo != "" && mobileCreds.OTP == "" && mobileCreds.Password != "")
            //{
            //    TokenData tokenData = new TokenData();

            //    string encodedPassword = "";
            //    //mobileCreds.Password = EncodeDecode.DecodeFrom64(mobileCreds.Password);
            //    mobileCreds.Password = EncodeDecode.EncodePasswordToBase64(mobileCreds.Password);

            //    var IsValidMobile = await employeeRepository.ValidateMobile_Pass(mobileCreds);

            //    string IsValid = "", TokenYN = "N";
            //    if (IsValidMobile != null && IsValidMobile.Count() > 0)
            //    {
            //        IsValid = IsValidMobile.Select(x => x.IsValidCreds).ToList()[0].ToString();
            //        TokenYN = IsValidMobile.Select(x => x.TokenNo).ToList()[0].ToString();

            //        if (IsValid.ToUpper() != "TRUE")
            //        {
            //            tokenData.IsValidCreds = IsValid;
            //            return Ok(new { statusCode = Ok(IsValidMobile).StatusCode, isSuccess = "false", message = "Invalid MobileNo or Password", data = new { } });
            //        }

            //        else if (IsValid.ToUpper() == "TRUE")
            //        {
            //            SaveTokens_UserCreds saveTokens_UserCreds = new SaveTokens_UserCreds();
            //            saveTokens_UserCreds.MobileNo = mobileCreds.MobileNo;

            //            DashBoardList dashboardList = new DashBoardList();
            //            dashboardList = await Save_Get_Token(mobileCreds);
            //            if (dashboardList != null)
            //            {
            //                if (dashboardList.is_valid_token != "Y")
            //                {
            //                    return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
            //                }

            //                return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardList });
            //            }

            //            return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "false", message = "Bad Request", data = new { } });
            //        }
            //    }
            //}

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

    }
}
