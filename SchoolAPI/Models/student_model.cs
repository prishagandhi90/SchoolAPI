using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentAPI.Shared
{
    public class StudentModel
    {
        [Keyless]
        public class RegistrationModel
        {
            public long? Id { get; set; }
            public string? Name { get; set; }
            public string? FatherName { get; set; }
            public string? Surname { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public string? Pincode { get; set; }
            public string? City { get; set; }
            public string? Mobile1 { get; set; }
            public string? Mobile2 { get; set; }
        }

        [Keyless]
        public class MobileNum
        {
            public string? MobileNo { get; set; }
        }

        [Keyless]
        public class LoginIdNum
        {
            public string? LoginId { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
        }

        [Keyless]
        public class IsValidData
        {
            public string? IsValid { get; set; }
            public string? Message { get; set; }
        }

        [Keyless]
        public class IsValidToken
        {
            public string? IsValid { get; set; }
            public string? UserId { get; set; }
        }

        [Keyless]
        public class OTP
        {
            public string? MobileNo { get; set; }
            public string? OTPNo { get; set; }
            public string? Message { get; set; }
        }

        [Keyless]
        public class MobileCreds
        {
            public string? MobileNo { get; set; }
            public string? Password { get; set; }
            public string? OTP { get; set; }
            public string? DeviceType { get; set; }
            public string? DeviceName { get; set; }
            public string? OSType { get; set; }
            public string? DeviceToken { get; set; }
            public string? FirebaseId { get; set; }
        }

        [Keyless]
        public class LoginId_TokenData
        {
            public string? LoginId { get; set; }
            public string? TokenNo { get; set; }
            public string? IsValidCreds { get; set; }
        }

        [Keyless]
        public class TokenData
        {
            public string? TokenNo { get; set; }
            public string? IsValidCreds { get; set; }
        }

        [Keyless]
        public class DashBoardList
        {
            public string? is_valid_token { get; set; }
            public Int64? login_id { get; set; }
            public string? token { get; set; }
            public int? EmployeeId { get; set; }
            public string? EmployeeName { get; set; }
            public string? MobileNumber { get; set; }
            public string? EmailAddress { get; set; }
            public string? DOB { get; set; }
            public string? Address { get; set; }
            public string? EmpCode { get; set; }
            public string? Department { get; set; }
            public string? Designation { get; set; }
            public string? Emp_Type { get; set; }
            public string? IsSuperAdmin { get; set; }
            public string? IsPharmacyUser { get; set; }
            public int? NotificationCount { get; set; }
            public string? IsPasswordSet { get; set; }
        }

        [Keyless]
        public class Ddl_Value_Nm
        {
            //public string? IsValidToken { get; set; }
            public string? Value { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class LoginOTP
        {
            public string? statusCode { get; set; }
            public string? Message { get; set; }
            public OTP? Data { get; set; }
        }

        [Keyless]
        public class OutSingleString
        {
            public string? OutValue { get; set; }
        }

        [Keyless]
        public class Resp_value_name
        {
            public string? Value { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_id_name
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_id_int_name
        {
            public int? Id { get; set; }
            public string? Name { get; set; }
        }

        [Keyless]
        public class Resp_name
        {
            public string? Name { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_Flag
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? Flag { get; set; }
        }

        [Keyless]
        public class SavedYesNo
        {
            public string? SavedYN { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_DtRange
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? DtRange { get; set; }
        }

        [Keyless]
        public class LoginId_EmpId_PresMed
        {
            public string? LoginId { get; set; }
            public string? EmpId { get; set; }
            public string? MstId { get; set; }
        }


    }
}
