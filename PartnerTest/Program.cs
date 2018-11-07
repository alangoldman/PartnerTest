
using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PartnerTest
{
    public class Program
    {
        private const string BootstrapAppId = "9f11594c-d487-4572-a986-5dd44f8a6aa2";
        private const string TenantDirectoryId = "d945addd-74f3-491f-8cc5-5ac214b5a105"; // alantest9.onmicrosoft.com

        private const string GraphResource = "https://graph.windows.net/";

        internal static async Task<AuthenticationResult> GetAppAccessToken(string appId, string appSecret, string tenantId, string resource)
        {
            var appCredentials = new ClientCredential(appId, appSecret);
            var context = new AuthenticationContext($"https://login.windows.net/{tenantId}");
            var result = await context.AcquireTokenAsync(resource, appCredentials);
            return result;
        }

        public static async Task Main(string[] args)
        {
            var graphClient = new ActiveDirectoryClient(new Uri(new Uri(GraphResource), TenantDirectoryId),
                async () => (await GetAppAccessToken(BootstrapAppId, ConfigurationManager.AppSettings["BootstrapAppSecret"], TenantDirectoryId, GraphResource)).AccessToken);

            try
            {
                var apps = await graphClient.Applications.ExecuteAsync();
                foreach (var app in apps.CurrentPage)
                {
                    Console.WriteLine(app.AppId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
                throw;
            }
        }
    }
}
