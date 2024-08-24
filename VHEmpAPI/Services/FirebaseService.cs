using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using VHEmpAPI;

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

    private readonly FirebaseMessaging _messaging;

    public FirebaseService()
    {
        // Initialize Firebase Admin SDK
        //var firebaseCredentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirebaseCredentials", "vhempapp-firebase-adminsdk-30xyo-820e48ac12.json");
        var firebaseCredentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirebaseCredentials", "vhempapp-3591e-firebase-adminsdk-v5daj-cb50988661.json");
        //var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirebaseCredentials", "firebase-adminsdk.json");
        var firebaseApp = FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseCredentialPath)
        });

        _messaging = FirebaseMessaging.GetMessaging(firebaseApp);

        //string scopes = "https://www.googleapis.com/auth/firebase.messaging";
        //var bearertoken = ""; // Bearer Token in this variable

        //using (var stream = new FileStream(firebaseCredentialPath, FileMode.Open, FileAccess.Read))
        //{
        //    bearertoken = GoogleCredential
        //      .FromStream(stream) // Loads key file
        //      .CreateScoped(scopes) // Gathers scopes requested
        //      .UnderlyingCredential // Gets the credentials
        //      .GetAccessTokenForRequestAsync().Result; // Gets the Access Token
        //}

    }

    public async Task<string> SendNotificationAsync(string title, string body, string token)
    {
        //var message = new Message()
        //{
        //    Notification = new Notification()
        //    {
        //        Title = title,
        //        Body = body
        //    },
        //    Token = token
        //};

        //string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        //return response;
        try
        {
            // Create a notification message
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Token = token // Device token retrieved from the client (Flutter app)
            };

            // Send the message
            //string response = await _messaging.SendAsync(message);
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }
        catch (Exception ex)
        {
            // Handle exception
            Console.WriteLine($"Error sending FCM notification: {ex.Message}");
            throw;
        }
    }
}
