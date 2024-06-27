using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class FirebaseNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly string _firebaseServerKey;
    private readonly HttpClient _httpClient;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _firebaseServerKey = configuration["Firebase:ServerKey"];
        _httpClient = new HttpClient();
    }

    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var requestUri = "https://fcm.googleapis.com/fcm/send";
        var notification = new
        {
            to = token,
            notification = new
            {
                title,
                body
            }
        };

        var json = JsonConvert.SerializeObject(notification);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("Authorization", $"key={_firebaseServerKey}");

        var response = await _httpClient.PostAsync(requestUri, content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Successfully sent message: {response.Content.ReadAsStringAsync().Result}");
        }
        else
        {
            _logger.LogError($"Error sending message: {response.ReasonPhrase}");
        }
    }
}
