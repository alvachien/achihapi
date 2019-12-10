using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

namespace hihapi.test.IntegrationTests
{
    public abstract class BasicIntegrationTest: IClassFixture<CustomWebApplicationFactory<hihapi.Startup>>, IDisposable
    {
        protected readonly CustomWebApplicationFactory<hihapi.Startup> _factory;
        protected readonly HttpClient _client;

        public BasicIntegrationTest(CustomWebApplicationFactory<hihapi.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
            // .WithWebHostBuilder(builder => builder.UseSolutionRelativeContentRoot("relative/path/of/project/under/test"));

            //_client = factory.CreateClient(new WebApplicationFactoryClientOptions
            //{
            //    AllowAutoRedirect = false
            //});
            //_client.DefaultRequestHeaders.Accept.Clear();
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            if (_factory.DBConnection != null)
            {
                _factory.DBConnection.Close();
            }
        }
    }
}
