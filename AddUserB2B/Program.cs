using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AddUserB2B
{
    class Program
    {
        
        static readonly string GraphResource = "https://graph.microsoft.com";

        static readonly string InviteEndPoint = "https://graph.microsoft.com/v1.0/invitations";

        static readonly string EstsLoginEndpoint = "https://login.microsoftonline.com";

        //Add tenant ID here ,you will get in help of axure at show diagonistics '?' sign button
        private static readonly string TenantID = "5d06e336-4f3c-4fbb-9bee-badcfc332596";

        

        //App ID registered in above tenant
        private static readonly string TestAppClientId = "9ca3ba1c-0907-4025-b174-ff0ccbf6ec64";

        //App Client secret
        private static readonly string TestAppClientSecret = @"m9b4mLPMOSH9/NhPO6doRGhjXCpueXPBuAFu9G3GNKo=";

        ////User Email Address
        //private static readonly string InvitedUserEmailAddress = @"";

        ////user display name
        //private static readonly string InvitedUserDisplayName = @"";

        static void Main(string[] args)
        {
            sendToAll();
            Console.WriteLine("Press 'Enter' to Exit ");
            Console.ReadLine();
        }

        private static void startMethod()
        {
            Console.WriteLine("This Application let you invite users to your Active Directory Application");
            Console.WriteLine("Press 'Y' to continue");
            var userResponse = Console.ReadLine();
            if(userResponse.ToLower().Trim() == "y")
            {
                Console.WriteLine("Enter your Tenant ID");
                var UserTenantId = Console.ReadLine();

                Console.WriteLine($"Enter your App Id registered in tenant :{UserTenantId}");
                var UserAppClientId = Console.ReadLine();

                Console.WriteLine($"Enter the Client Secret");
                var UserClientSecret = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Thank you for using the application");
            }         
        }

        


        private static void sendToAll()
        {
           
            //csv file name : TestADUsers
            using (var reader = new StreamReader(@"C:\ADUserList.csv"))
            {
                //List<string> listA = new List<string>();
                //List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //listA.Add(values[0]);
                    //listB.Add(values[1]);
                    Console.WriteLine(values[0] + " : " + values[1]);
                    var username = values[0];
                    var emailadd = values[1];
                    Invitation invitation = CreateInvitation(username, emailadd);
                    SendInvitation(invitation);
                }
                reader.Close();

            }
        }

        public class Invitation
        {
            /// <summary>
            /// Gets or sets display name.
            /// </summary>
            public string InvitedUserDisplayName { get; set; }

            /// <summary>
            /// Gets or sets display name.
            /// </summary>
            public string InvitedUserEmailAddress { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether Invitation Manager should send the email to InvitedUser.
            /// </summary>
            public bool SendInvitationMessage { get; set; }

            /// <summary>
            /// Gets or sets invitation redirect URL
            /// </summary>
            public string InviteRedirectUrl { get; set; }
        }

        /// <summary>
        /// Create the invitation object.
        /// </summary>
        /// <returns>Returns the invitation object.</returns>
        private static Invitation CreateInvitation(string InvitedUserDisplayName, string InvitedUserEmailAddress)
        {
            // Set the invitation object.
            Invitation invitation = new Invitation();

            invitation.InvitedUserDisplayName = InvitedUserDisplayName;
            invitation.InvitedUserEmailAddress = InvitedUserEmailAddress;
            invitation.InviteRedirectUrl = "https://www.tme4automation.com";
            invitation.SendInvitationMessage = true;
            return invitation;
        }

        /// <summary>
        /// Send the guest user invite request.
        /// </summary>
        /// <param name="invitation">Invitation object.</param>
        private static void SendInvitation(Invitation invitation)
        {
            string accessToken = GetAccessToken();

            HttpClient httpClient = GetHttpClient(accessToken);

            // Make the invite call. 
            HttpContent content = new StringContent(JsonConvert.SerializeObject(invitation));
            content.Headers.Add("ContentType", "application/json");
            //content.Headers.ContentLength = 551;
            var postResponse = httpClient.PostAsync(InviteEndPoint, content).Result;
            string serverResponse = postResponse.Content.ReadAsStringAsync().Result;
            Console.WriteLine(serverResponse);
        }

        /// <summary>
        /// Get the HTTP client.
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>Returns the Http Client.</returns>
        private static HttpClient GetHttpClient(string accessToken)
        {
            // setup http client.
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
            Console.WriteLine(
                "CorrelationID for the request: {0}",
                httpClient.DefaultRequestHeaders.GetValues("client-request-id").Single());
            return httpClient;
        }

        /// <summary>
        /// Get the access token for our application to talk to microsoft graph.
        /// </summary>
        /// <returns>Returns the access token for our application to talk to microsoft graph.</returns>
        private static string GetAccessToken()
        {
            string accessToken = null;

            // Get the access token for our application to talk to microsoft graph.
            try
            {
                AuthenticationContext testAuthContext =
                    new AuthenticationContext(string.Format("{0}/{1}", EstsLoginEndpoint, TenantID));
                AuthenticationResult testAuthResult = testAuthContext.AcquireTokenAsync(
                    GraphResource,
                    new ClientCredential(TestAppClientId, TestAppClientSecret)).Result;
                accessToken = testAuthResult.AccessToken;
            }
            catch (AdalException ex)
            {
                Console.WriteLine("An exception was thrown while fetching the token: {0}.", ex);
                throw;
            }

            return accessToken;
        }

    }
}
