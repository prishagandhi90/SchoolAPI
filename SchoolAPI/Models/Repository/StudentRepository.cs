using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestSharp;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using VHEmpAPI.Interfaces;
using static Dapper.SqlMapper;
using static StudentAPI.Shared.StudentModel;

namespace StudentAPI.Models.Repository
{
    public class StudentRepository : IStudentRepository
    {
        public StudentDbContext StudDbContext { get; }
        private readonly IDBMethods _dbMethods;
        private IDbContextTransaction _transaction;
        private readonly IConfiguration _configuration;

        public StudentRepository(StudentDbContext studDbContext, IDBMethods dbm, IConfiguration configuration)
        {
            StudDbContext = studDbContext;
            _dbMethods = dbm ?? throw new ArgumentNullException(nameof(dbm));
            _configuration = configuration;
        }

        public async Task<IEnumerable<IsValidToken>> IsTokenValid(string TokenNo, string LoginId)
        {
            try
            {
                var tokenParam = new SqlParameter("@TokenNo", TokenNo ?? (object)DBNull.Value);
                var loginParam = new SqlParameter("@LoginId", LoginId ?? (object)DBNull.Value);

                var GetToken = await StudDbContext.IsValidToken
                    .FromSqlRaw("SELECT * FROM dbo.StudentApp_IsTokenValid(@TokenNo, @LoginId)", tokenParam, loginParam)
                    .ToListAsync();

                return GetToken;
            }
            catch (TaskCanceledException ex)
            {

            }
            finally
            {
                StudDbContext.Database.CloseConnection();
            }

            return (IEnumerable<IsValidToken>)Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<RegistrationModel>> FetchRegistrationData()
        {
            try
            {
                //using var conn = new SqlConnection(_configuration.GetConnectionString("VHMobileDBConnection"));
                //var result = await conn.ExecuteAsync("Ins_Upd_Registration", commandType: CommandType.StoredProcedure);

                var mobileNo = new SqlParameter("@p_Mobile", "" ?? (object)DBNull.Value);

                var result = await StudDbContext.RegistrationData
                                            .FromSqlRaw("EXEC dbo.StudApp_GetRegData @p_Mobile", mobileNo)
                                            .ToListAsync();

                return result.ToList();
            }
            catch (Exception ex)
            {
                // Optional: log exception here
                throw;
            }
        }


        public async Task<RegistrationModel> InsertOrUpdateAsync(RegistrationModel model)
        {
            try
            {
                var dp = new DynamicParameters();
                dp.Add("@Id", model.Id, DbType.Int64, ParameterDirection.InputOutput);
                dp.Add("@Name", model.Name, DbType.String);
                dp.Add("@FatherName", model.FatherName, DbType.String);
                dp.Add("@Surname", model.Surname, DbType.String);
                dp.Add("@DateOfBirth", model.DateOfBirth, DbType.Date);
                dp.Add("@Gender", model.Gender, DbType.String);
                dp.Add("@Address", model.Address, DbType.String);
                dp.Add("@Pincode", model.Pincode, DbType.String);
                dp.Add("@City", model.City, DbType.String);
                dp.Add("@Mobile1", model.Mobile1, DbType.String);
                dp.Add("@Mobile2", model.Mobile2, DbType.String);

                using var conn = new SqlConnection(_configuration.GetConnectionString("VHMobileDBConnection"));
                await conn.ExecuteAsync("Ins_Upd_Registration", dp, commandType: CommandType.StoredProcedure);

                model.Id = dp.Get<long>("@Id");
                return model;
            }
            catch (Exception ex)
            {
                // Optional: log exception here
                throw;
            }
        }

    }
}
