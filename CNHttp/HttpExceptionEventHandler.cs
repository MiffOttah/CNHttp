using System;
using System.Collections.Generic;
using System.Text;

namespace NHttp
{
    public class HttpExceptionEventArgs : HttpRequestEventArgs
    {
        public Exception Exception { get; private set; }

        public bool Handled { get; set; }

        public HttpExceptionEventArgs(HttpContext context, Exception exception)
            : base(context)
        {
            Exception = exception ?? throw new ArgumentNullException("exception");
        }
    }

    public delegate void HttpExceptionEventHandler(object sender, HttpExceptionEventArgs e);
}
