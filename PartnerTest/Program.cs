using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.Authorization.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace PartnerTest
{
    public class Program
    {
        private const string BootstrapAppId = "625a0b73-0843-470f-a518-682e83c28b0e";
        private const string TenantDirectoryId = "c7b812ca-5b1f-4d55-b73d-937d3a4eab0e"; // alantest5.onmicrosoft.com
        private const string SubscriptionId = "80404868-E176-4696-9235-071A1E057B6F";

        private const string GraphResource = "https://graph.windows.net/";
        private const string ArmResource = "https://management.azure.com/";

        internal static async Task<string> GetAppAccessToken(string resource)
        {
            var appCredentials = new ClientCredential(BootstrapAppId, ConfigurationManager.AppSettings["BootstrapAppSecret"]);
            var context = new AuthenticationContext($"https://login.windows.net/{TenantDirectoryId}");
            var result = await context.AcquireTokenAsync(resource, appCredentials);
            return result.AccessToken;
        }

        internal static async Task<string> GetUserAccessTokenFromRefreshToken(string resource)
        {
            using (var client = new HttpClient())
            {
                var body = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", ConfigurationManager.AppSettings["RefreshToken"] },
                    { "client_id", BootstrapAppId },
                    { "client_secret", ConfigurationManager.AppSettings["BootstrapAppSecret"]},
                    { "resource", resource }
                };

                var response = await client.PostAsync($"https://login.microsoftonline.com/{TenantDirectoryId}/oauth2/token",
                    new FormUrlEncodedContent(body));
                response.EnsureSuccessStatusCode();
                var o = await response.Content.ReadAsAsync<JObject>();
                return o["access_token"].ToString();
            }
        }

        public static async Task Main(string[] args)
        {
            var graphClient = new ActiveDirectoryClient(new Uri(new Uri(GraphResource), TenantDirectoryId),
                async () => await GetAppAccessToken(GraphResource));

            Console.WriteLine("Testing AzureAD api: list service principals, create one for the bootstrap app");
            string appPrincipalId = null;
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

                var appSp = await graphClient.ServicePrincipals.Where(sp => sp.AppId == BootstrapAppId)
                    .ExecuteSingleAsync();
                appPrincipalId = appSp?.ObjectId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
                throw;
            }

            Console.WriteLine("Testing AzureRM api: list resource groups with App+User auth");
            var userRmClient = new ResourceManagementClient(new Uri(ArmResource),
                new TokenCredentials(await GetUserAccessTokenFromRefreshToken(ArmResource)))
            {
                SubscriptionId = SubscriptionId
            };

            try
            {
                var rgs = await userRmClient.ResourceGroups.ListAsync();
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

            Console.WriteLine("Testing AzureRM api: assign bootstrap app IAM permissions to subscription");
            var userAuthClient = new AuthorizationManagementClient(new Uri(ArmResource),
                new TokenCredentials(await GetUserAccessTokenFromRefreshToken(ArmResource)))
            {
                SubscriptionId = SubscriptionId
            };
            var roleAssignments = await userAuthClient.RoleAssignments.ListAsync();
            if (!roleAssignments.Any(assignment => assignment.PrincipalId == appPrincipalId))
            {
                var scope = $"/subscriptions/{SubscriptionId}";
                var ownerDefinition =
                    await userAuthClient.RoleDefinitions.GetAsync(scope, "8e3af657-a8ff-443c-a75c-2fe8c4bcb635");  // owner
                await userAuthClient.RoleAssignments.CreateAsync(scope,
                    Guid.NewGuid().ToString(), new RoleAssignmentCreateParameters
                    {
                        PrincipalId = appPrincipalId,
                        RoleDefinitionId = ownerDefinition.Id
                    });
            }

            Console.WriteLine("Testing AzureRM api: list resource groups with App Only auth");
            var appRmClient = new ResourceManagementClient(new Uri(ArmResource),
                new TokenCredentials(await GetAppAccessToken(ArmResource)))
            {
                SubscriptionId = SubscriptionId
            };
            try
            {
                var rgs = await appRmClient.ResourceGroups.ListAsync();
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

            Console.WriteLine("Success!");
            Console.ReadKey();
        }
    }
}
