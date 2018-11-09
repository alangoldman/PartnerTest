using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace PartnerTest
{
    public class Program
    {
        private const string BootstrapAppId = "625a0b73-0843-470f-a518-682e83c28b0e";
        private const string TenantDirectoryId = "c7b812ca-5b1f-4d55-b73d-937d3a4eab0e"; // alantest5.onmicrosoft.com
        private const string SubscriptionId = "80404868-E176-4696-9235-071A1E057B6F";

        private const string GraphResource = "https://graph.windows.net/";
        private const string ArmResource = "https://management.azure.com/";

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

            Console.WriteLine("Testing AzureAD api: list service principals, create one for the bootstrap app");
            try
            {
                var sps = await graphClient.ServicePrincipals.ExecuteAsync();
                if (!sps.CurrentPage.Any())
                {
                    Console.WriteLine("No service principals found");
                }
                foreach (var sp in sps.CurrentPage)
                {
                    Console.WriteLine(sp.AppId);
                }

                // Without this step the error is:
                // The received access token is not valid: at least one of the claims 'puid' or 'altsecid' or 'oid' should be present. If you are accessing as application please make sure service principal is properly created in the tenant.
                if (!sps.CurrentPage.Any(sp => sp.AppId == BootstrapAppId))
                {
                    Console.WriteLine("Creating service principal for bootstrap app");
                    await graphClient.ServicePrincipals.AddServicePrincipalAsync(new ServicePrincipal
                    {
                        AppId = BootstrapAppId
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
                throw;
            }

            var rmClient = new ResourceManagementClient(new Uri(ArmResource),
                new TokenCredentials((await GetAppAccessToken(BootstrapAppId,
                        ConfigurationManager.AppSettings["BootstrapAppSecret"], TenantDirectoryId, ArmResource))
                    .AccessToken))
            {
                SubscriptionId = SubscriptionId
            };

            Console.WriteLine("Testing AzureRM api: list resource groups");
            try
            {
                var rgs = await rmClient.ResourceGroups.ListAsync();
                if (rgs == null || !rgs.Any())
                {
                    Console.WriteLine("No resource groups found");
                }
                else
                {
                    foreach (var rg in rgs)
                    {
                        Console.WriteLine(rg.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
                throw;
            }

            Console.ReadKey();
        }
    }
}
