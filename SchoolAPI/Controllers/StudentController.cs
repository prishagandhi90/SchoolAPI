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
using StudentAPI.Models.Repository;
using static StudentAPI.Shared.StudentModel;

namespace SchoolAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository? studentRepository;
        private readonly IJwtAuth jwtAuth;
        private readonly IConfiguration _configuration;
        public string Message = "";
        //private readonly string _basePath = @"C:\inetpub\wwwroot\VHMobileAPI\Uploads";
        //private readonly string _basePath = @"C:\inetpub\wwwroot\VHTestEmpAPI\Uploads";
        private readonly IWebHostEnvironment _env;

        public StudentController(IStudentRepository studentRepository, IJwtAuth jwtAuth, IConfiguration configuration, IWebHostEnvironment env)
        {
            this.studentRepository = studentRepository;
            this.jwtAuth = jwtAuth;
            _configuration = configuration;
            _env = env;
        }

        #region Registration Module

        [HttpPost("FetchRegistrationData")]
        //[Authorize]
        public async Task<IActionResult> FetchRegistrationData()
        {
            try
            {
                var result = await studentRepository.FetchRegistrationData();

                if (result == null || result.Count() == 0)
                    return Ok(new { statusCode = 404, isSuccess = "false", message = "No data found", data = new List<RegistrationModel>() });

                return Ok(new
                {
                    statusCode = 200,
                    isSuccess = "true",
                    message = "Data fetched successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = 500,
                    isSuccess = "false",
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }


        [HttpPost("InsUpdRegistration")]
        //[Authorize]
        public async Task<ActionResult> InsUpdRegistration([FromBody] RegistrationModel model)
        {
            try
            {
                string isValid = "", empId = "";

                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await studentRepository.IsTokenValid(token, model.Id.ToString()); // model.Id ya koi LoginId pass karo
                if (isValidToken != null)
                {
                    isValid = isValidToken.Select(x => x.IsValid).FirstOrDefault()?.ToString();
                    empId = isValidToken.Select(x => x.UserId).FirstOrDefault()?.ToString();

                    if (isValid != "Y")
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                }

                if (model == null)
                    return Ok(new { statusCode = 400, isSuccess = "false", message = "Model cannot be null", data = new { } });

                var savedModel = await studentRepository.InsertOrUpdateAsync(model);

                if (savedModel == null || savedModel.Id == 0)
                    return Ok(new { statusCode = 400, isSuccess = "false", message = "Insert/Update failed", data = new { } });

                return Ok(new
                {
                    statusCode = 200,
                    isSuccess = "true",
                    message = model.Id == 0 ? "Inserted Successfully" : "Updated Successfully",
                    data = savedModel
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = 500,
                    isSuccess = "false",
                    message = "Internal Server Error",
                    error = ex.Message
                });
            }
        }


        #endregion


    }
}
