using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;

public class FirebaseService
{
    //public FirebaseService()
    //{
    //    var pathToFirebaseCredentials = Path.Combine(Directory.GetCurrentDirectory(), "FirebaseCredentials", "google-services.json");
    //    if (FirebaseApp.DefaultInstance == null)
    //    {
    //        FirebaseApp.Create(new AppOptions()
    //        {
    //            Credential = GoogleCredential.FromFile(pathToFirebaseCredentials)
    //        });
    //    }
    //}

    public async Task<string> SendNotificationAsync(string title, string body, string token)
    {
        var message = new Message()
        {
            Notification = new Notification()
            {
                Title = title,
                Body = body
            },
            Token = token
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return response;
    }
}
