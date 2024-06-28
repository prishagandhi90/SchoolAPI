using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

public class NotificationService : BackgroundService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly FirebaseService _firebaseService;
    private readonly string _connectionString;

    public NotificationService(ILogger<NotificationService> logger, IServiceProvider serviceProvider, FirebaseService firebaseService, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _firebaseService = firebaseService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckForNewPatientsAsync();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task CheckForNewPatientsAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var newPatients = await connection.QueryAsync<PatientNotification>("SELECT * FROM PatientNotifications WHERE Notified = 0");

            foreach (var patient in newPatients)
            {
                var doctorDeviceToken = GetDoctorDeviceToken(patient.DoctorId);

                await _firebaseService.SendNotificationAsync("New Patient Admitted", $"Patient: {patient.PatientName}", doctorDeviceToken);

                patient.Notified = true;
                await connection.ExecuteAsync("UPDATE PatientNotifications SET Notified = 1 WHERE Id = @Id", new { patient.Id });
            }
        }
    }

    private string GetDoctorDeviceToken(int doctorId)
    {
        // Your logic to retrieve the doctor's device token from the database
        return "doctor_device_token";
    }
}

public class PatientNotification
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string PatientName { get; set; }
    public bool Notified { get; set; }
}
