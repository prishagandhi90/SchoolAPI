using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly string _connectionString;

    public PatientsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public async Task<IActionResult> AddPatient([FromBody] PatientModel patient)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("EXEC AddNewPatient @DoctorId, @PatientName", new { patient.DoctorId, patient.PatientName });
        }

        return Ok();
    }
}

public class PatientModel
{
    public int DoctorId { get; set; }
    public string PatientName { get; set; }
}
