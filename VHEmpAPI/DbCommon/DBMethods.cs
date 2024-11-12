using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using VHEmpAPI.Interfaces;

namespace VHEmpAPI.DbCommon
{
    public class DBMethods : IDBMethods
    {
        private readonly IConfiguration _configuration;

        public DBMethods(IConfiguration configuration)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public DataTable GetDataTable(string sqlStr)
        {
            DataTable objresutl = new DataTable();

            try
            {
                SqlDataReader myReader;
                using (SqlConnection myCon = new SqlConnection(this._configuration.GetConnectionString("VHMobileDBConnection")))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(sqlStr, myCon))
                    {
                        myReader = myCommand.ExecuteReader();
                        objresutl.Load(myReader);

                        myReader.Close();
                        myCon.Close();
                    }
                }

                return objresutl;
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
            return new DataTable();
        }

        public DataSet GetDataSet(string sqlStr)
        {
            DataTable objresutl = new DataTable();
            DataSet ds = new DataSet();

            try
            {
                SqlDataReader myReader;
                using (SqlConnection myCon = new SqlConnection(this._configuration.GetConnectionString("VHMobileDBConnection")))
                {
                    myCon.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(sqlStr, myCon))
                    {
                        da.Fill(ds);
                        myCon.Close();
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
            return new DataSet();
        }
    }
}
