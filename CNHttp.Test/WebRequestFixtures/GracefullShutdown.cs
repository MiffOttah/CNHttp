using NHttp.Test.Support;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace NHttp.Test.WebRequestFixtures
{
    [TestFixture]
    public class GracefullShutdown : FixtureBase
    {
        [Test]
        //[ExpectedException(typeof(WebException))]
        public void ForcedShutdown()
        {
            using (var server = new HttpServer())
            {
                server.ShutdownTimeout = TimeSpan.FromSeconds(1);

                server.RequestReceived += (s, e) =>
                {
                    // Start closing the server.

                    ThreadPool.QueueUserWorkItem(p => server.Stop());

                    // Wait some time to fulfill the request.

                    Thread.Sleep(TimeSpan.FromSeconds(30));

                    using (var writer = new StreamWriter(e.Response.OutputStream))
                    {
                        writer.WriteLine("Hello!");
                    }
                };

                server.Start();

                Assert.Throws<WebException>(() =>
                {
                    var request = (HttpWebRequest)WebRequest.Create(
                        $"http://{server.EndPoint}/"
                    );

                    GetResponseFromRequest(request);
                });
            }
        }
    }
}
