using System;

namespace NHttp
{
    public class HttpContext
    {
        internal HttpContext(HttpClient client)
        {
            // Still set the obsolete entry.
#pragma warning disable CS0618 // Type or member is obsolete
            Server = client.Server.ServerUtility;
#pragma warning restore CS0618 // Type or member is obsolete

            Request = new HttpRequest(client);
            Response = new HttpResponse(this);
        }

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public HttpServerUtility Server { get; }

        public HttpRequest Request { get; }

        public HttpResponse Response { get; }
    }
}
