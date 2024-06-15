using Microsoft.AspNetCore.Mvc;
using System.Net;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models.Repository;
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

                var response = Ok(result);
                if (response.StatusCode == 200)
                {
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

    }
}
