using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly FirebaseService _firebaseService;

    public NotificationsController(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    [HttpPost("newPatient")]
    public async Task<IActionResult> NewPatient([FromBody] PatientModel patient)
    {
        var doctorDeviceToken = GetDoctorDeviceToken(patient.DoctorId);

        await _firebaseService.SendNotificationAsync("New Patient Admitted", "A new patient has been admitted.", doctorDeviceToken);

        return Ok("Notification sent.");
    }

    private string GetDoctorDeviceToken(int doctorId)
    {
        // Your logic to retrieve the doctor's device token from the database
        return "doctor_device_token";
    }
}

public class PatientModel
{
    public int DoctorId { get; set; }
    public string PatientName { get; set; }
}
