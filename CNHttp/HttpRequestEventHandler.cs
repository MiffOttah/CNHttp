﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NHttp
{
    public class HttpRequestEventArgs : EventArgs
    {
        public HttpRequestEventArgs(HttpContext context)
        {
            Context = context ?? throw new ArgumentNullException("context");
        }

        public HttpContext Context { get; }

        [Obsolete]
        public HttpServerUtility Server => Context.Server;

        public HttpRequest Request => Context.Request;

        public HttpResponse Response => Context.Response;
    }

    public delegate void HttpRequestEventHandler(object sender, HttpRequestEventArgs e);
}
