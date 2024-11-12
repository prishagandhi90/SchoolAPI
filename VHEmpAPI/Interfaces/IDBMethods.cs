using System.Data;
using VHEmpAPI.Shared;

namespace VHEmpAPI.Interfaces
{
    public interface IDBMethods
    {
        DataTable GetDataTable(string sqlStr);
        DataSet GetDataSet(string sqlStr);
    }
}
