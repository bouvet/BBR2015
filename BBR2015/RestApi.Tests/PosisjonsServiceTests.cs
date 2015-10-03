using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Database;
using NUnit.Framework;
using RestApi.Models;

namespace RestApi.Tests
{
    [TestFixture]
    public class PosisjonsServiceTests
    {
        [Test]
        public async void RegistrerPosisjon()
        {
            var container = RestApiApplication.CreateContainer();
            var config = container.Resolve<CascadingAppSettings>();
            var client = new HttpClient();
            client.BaseAddress = new Uri(config.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string route = "api/posisjoner";
            var uriRoute = Path.Combine(client.BaseAddress.AbsoluteUri, route);
            var remoteUri = new Uri(uriRoute);

            var koordinat = new Koordinat(10.2, 24.123);

            var response = await client.PostAsJsonAsync(remoteUri, koordinat);

            response.EnsureSuccessStatusCode();
        }
    }
}
