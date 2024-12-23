using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public interface IEmpLoginRepository
    {
        Task<IEnumerable<IsValidData>> ValidateMobileNo(string MobileNo);
        Task<IEnumerable<OTP>> SendEmpMobileOTP(OTP respOTP);
        Task<IEnumerable<CommonProcOutputFields.IsValidData>> GenerateNewPassword(string MobileNo, string Password);
        Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> GetLoginUserNames(string LoginId);
    }
}
