using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireMock;
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

        public WireMockServer GetServer()
        {
            return _server;
        }

        public void SetUpWorkingThenDownApi()
        {
            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .InScenario("WorkingThenDown")
                .WillSetStateTo("WorkingThenDown Down")
                .RespondWith(Response.Create()
                    .WithBody("{\"latitude\":52.52,\"longitude\":13.419998}")
                    .WithStatusCode(200));

            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .InScenario("WorkingThenDown")
                .WhenStateIs("WorkingThenDown Down")
                .WillSetStateTo("WorkingThenDown Down")
                .RespondWith(Response.Create()
                    .WithStatusCode(500));
        }


        public void SetUpTransientFaultApi()
        {
            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .InScenario("TransientFault")
                .WillSetStateTo("TransientFault Working")
                .RespondWith(Response.Create()
                    .WithDelay(1000)
                    .WithStatusCode(500));

            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .InScenario("TransientFault")
                .WhenStateIs("TransientFault Working")
                .WillSetStateTo("TransientFault Working")
                .RespondWith(Response.Create()
                    .WithBody("{\"latitude\":52.52,\"longitude\":13.419998}")
                    .WithStatusCode(200));
        }

        public void SetupWorkingApi()
        {
            _server.Given(Request.Create()
                .WithPath("/forecast")
                .UsingGet())
                .RespondWith(Response.Create()
                    .WithBody("{\"latitude\":52.52,\"longitude\":13.419998}")
                    .WithStatusCode(200));
        }

        public void SetupDownApi()
        {

            _server.Given(Request.Create()
                .WithPath("/forecast")
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
