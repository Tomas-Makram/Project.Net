using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace CinemaProject.Services
{
    public class FirebaseService
    {
        private static bool isFirebaseInitialized = false;

        public FirebaseService()
        {
            if (!isFirebaseInitialized)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("D:\\Universty\\s2\\.NET MVC ASP\\CenimaProject\\CenimaProject\\wwwroot\\Firebase.json")
                });
                isFirebaseInitialized = true;
            }
        }

        public async Task<bool> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decodedToken != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
