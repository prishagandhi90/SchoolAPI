using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using FirebaseAdmin.Messaging;

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
        _connectionString = configuration.GetConnectionString("VHMobileDBConnection");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //await CheckForNewPatientsAsync();
            await CheckForNotificationsAsync();
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

                //await _firebaseService.SendNotificationAsync("New Patient Admitted", $"Patient: {patient.PatientName}", doctorDeviceToken);
                await _firebaseService.SendNotificationAsync("New Patient Admitted", $"Patient: {patient.PatientName}", patient.DeviceToken);

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

    private async Task CheckForNotificationsAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            //var newPatients = await connection.QueryAsync<EmpNotification>("select bd.Id, b.MessageTitle, b.Message, l.FirebaseId as DeviceToken, bd.SentDate, b.InactiveDate " +
            //                                                              "from NotificationDetail bd, NotificationMaster b, LoginMobile l " +
            //                                                              "where l.LoginId = bd.LoginId and AppName = 'Emp' " +
            //                                                              "and bd.MessageId = b.Id and bd.Status = 'Pending'");
            var queryString = await connection.ExecuteScalarAsync<string>("SELECT dbo.GetUnreadNotifications_Str('GetData_Str', 0)");
            var newPatients = await connection.QueryAsync<EmpNotification>(queryString);

            foreach (var patient in newPatients)
            {
                try
                {
                    //var doctorDeviceToken = GetDoctorDeviceToken(patient.DoctorId);

                    //await _firebaseService.SendNotificationAsync("New Patient Admitted", $"Patient: {patient.PatientName}", doctorDeviceToken);
                    var response = await _firebaseService.SendNotificationAsync(patient.MessageTitle, patient.Message, patient.DeviceToken);

                    if (!string.IsNullOrEmpty(response))
                    {
                        // Update the notification status in the database
                        //await connection.ExecuteAsync("UPDATE NotificationDetail SET SentDate = Getdate(), status = 'Sent' WHERE Id = @Id", new { patient.Id });

                        //await connection.ExecuteAsync("UPDATE bd SET SentDate = Getdate(), status = 'Sent' " +
                        //                              "from NotificationDetail bd, NotificationMaster b " +
                        //                              "WHERE bd.MessageId = b.Id and b.AppName = 'Emp' and bd.Id = @Id", new { patient.Id });

                        var updStr = await connection.ExecuteScalarAsync<string>($"SELECT dbo.GetUnreadNotifications_Str('UpdData_Str', {patient.Id})");
                        await connection.ExecuteAsync(updStr);
                    }
                    else
                    {
                        //_logger.LogWarning($"CheckForNotificationsAsync: Notification not sent for Patient ID: {patient.Id}. Response was null.");
                    }
                    //patient.SentDate = true;
                    //await connection.ExecuteAsync("UPDATE NotificationDetail SET SentDate = Getdate() WHERE Id = @Id", new { patient.Id });
                    //await connection.ExecuteAsync("UPDATE NotificationDetail SET SentDate = Getdate() WHERE Id = @Id", new { patient.Id });
                }
                catch (Exception ex)
                {
                    // Log the exception and continue to the next patient
                    //_logger.LogError($"NotificationService.cs: CheckForNotificationsAsync: catch: Error sending notification for : {patient.Id}. Error: {ex.Message}");
                }
            }
        }
    }

}

public class PatientNotification
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string PatientName { get; set; }
    public string DeviceToken { get; set; }
    public bool Notified { get; set; }
}

public class EmpNotification
{
    public long Id { get; set; }
    //public int DoctorId { get; set; }
    public string MessageTitle { get; set; }
    public string Message { get; set; }
    public string DeviceToken { get; set; }
    public DateTime SentDate { get; set; }
    public DateTime InactiveDate { get; set; }
}
