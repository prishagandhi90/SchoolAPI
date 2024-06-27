using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly FirebaseNotificationService _notificationService;

    public NotificationController(FirebaseNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification(string token, string title, string body)
    {
        await _notificationService.SendNotificationAsync(token, title, body);
        return Ok("Notification sent successfully.");
    }
}
