using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace hihapi.test.IntegrationTests
{
    public class IdentityServerSetup
    {
        private IWebHost _webHost;
        private static IdentityServerSetup instance = null;
        private static readonly object padlock = new object();

        IdentityServerSetup() { }

        public static IdentityServerSetup Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new IdentityServerSetup();
                        instance.InitializeIdentityServer();
                    }
                    return instance;
                }
            }
        }

        public async Task<string> GetAccessTokenForUser(string userName, string password, 
            string clientId = DataSetupUtility.IntegrationTestClient, 
            string clientSecret = "secret")
        {
            var client = new TokenClient(DataSetupUtility.IntegrationTestIdentityServerUrl + "/connect/token", clientId, clientSecret);

            var response = await client.RequestResourceOwnerPasswordAsync(userName, password, DataSetupUtility.IntegrationTestAPIScope);

            return response.AccessToken;
        }

        private void InitializeIdentityServer()
        {
            _webHost = WebHost.CreateDefaultBuilder()
                .UseUrls(DataSetupUtility.IntegrationTestIdentityServerUrl)
                .ConfigureServices(services =>
                {
                    services.AddIdentityServer()
                        .AddDeveloperSigningCredential()
                        .AddInMemoryApiResources(GetApiResources())
                        .AddInMemoryClients(GetClients())
                        .AddTestUsers(GetUsers());
                })
                .Configure(app => app.UseIdentityServer())
                .Build();
            Task.Factory.StartNew(() => _webHost.Run());
            //_webHost.Run();
        }

        public static List<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(DataSetupUtility.IntegrationTestAPIScope, "HIH API") { ApiSecrets = {new Secret("secret".Sha256())} }
            };
        }

        // Default client
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = DataSetupUtility.IntegrationTestClient,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        DataSetupUtility.IntegrationTestAPIScope
                    },
                    AllowOfflineAccess = true
                }
            };
        }

        // Default user
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = DataSetupUtility.UserA,
                    Username = DataSetupUtility.UserA,
                    Password = DataSetupUtility.IntegrationTestPassword,

                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserA),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserA),
                    }
                },
                new TestUser
                {
                    SubjectId = DataSetupUtility.UserB,
                    Username = DataSetupUtility.UserB,
                    Password = DataSetupUtility.IntegrationTestPassword,

                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserB),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserB),
                    }
                },
                new TestUser
                {
                    SubjectId = DataSetupUtility.UserC,
                    Username = DataSetupUtility.UserC,
                    Password = DataSetupUtility.IntegrationTestPassword,

                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserC),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserC),
                    }
                },
                new TestUser
                {
                    SubjectId = DataSetupUtility.UserD,
                    Username = DataSetupUtility.UserD,
                    Password = DataSetupUtility.IntegrationTestPassword,

                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserD),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserD),
                    }
                }
            };
        }
    }
}
