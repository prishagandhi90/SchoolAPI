using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using StudentAPI.Shared;
using static StudentAPI.Shared.StudentModel;

namespace StudentAPI.Models.Repository
{
    public interface IStudentRepository
    {
        //Task<IDbContextTransaction> BeginTransaction();
        //Task CommitTransaction(IDbContextTransaction transaction);
        //Task RollbackTransaction(IDbContextTransaction transaction);
        //Task DisposeTransaction(IDbContextTransaction transaction);
        Task<IEnumerable<IsValidToken>> IsTokenValid(string TokenNo, string LoginId);
        Task<RegistrationModel> InsertOrUpdateAsync(RegistrationModel model);
        Task<IEnumerable<RegistrationModel>> FetchRegistrationData();

    }
}
