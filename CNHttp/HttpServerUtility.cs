using System;
using System.Net;
using System.Text;

namespace NHttp
{
    [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
    public class HttpServerUtility
    {
        internal HttpServerUtility()
        {
        }

        public string MachineName
        {
            get { return Environment.MachineName; }
        }

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public string UriDecode(string value) => WebUtility.UrlDecode(value ?? throw new ArgumentNullException("value"));

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public string UriDecode(string value, Encoding encoding)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var encoded = encoding.GetBytes(value);
            var decoded = WebUtility.UrlDecodeToBytes(encoded, 0, encoded.Length);
            return encoding.GetString(decoded, 0, decoded.Length);
        }

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public string HtmlEncode(string value) => WebUtility.HtmlEncode(value ?? throw new ArgumentNullException("value"));

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public string HtmlDecode(string value) => WebUtility.HtmlDecode(value ?? throw new ArgumentNullException("value"));

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public string UrlEncode(string text)
        {
            return Uri.EscapeDataString(text);
        }
    }
}
