using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireMock.Org.Abstractions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SpaceWeatherStation.Tests.Integration
{
    public class OpenMeteoFakeAPIServer : IDisposable
    {
        private WireMockServer _server;

        public string Url => _server.Url!;

        public void Start()
        {
            _server = WireMockServer.Start();
        }

        public void SetupWorkingApi()
        {
            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .RespondWith(Response.Create()
                    .WithBody("{\"latitude\":52.52,\"longitude\":13.419998}")
                    .WithStatusCode(200));

            _server.Given(Request.Create()
                .WithPath("/forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m,relativehumidity_2m,windspeed_10m")
                .UsingGet())
                .RespondWith(Response.Create()
                    .WithBody("{\"latitude\":52.52,\"longitude\":13.419998}")
                    .WithStatusCode(200));
        }

        public void SetupDownApi()
        {

            _server.Given(Request.Create()
                .WithPath("/forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m,relativehumidity_2m,windspeed_10m")
                .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500));
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
