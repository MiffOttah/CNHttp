using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace NHttp
{
    internal static class HttpUtil
    {
        internal static void UrlDecodeTo(ReadOnlySpan<char> text, NameValueCollection queryString)
        {
            int indexOfAmpersand;
            do
            {
                indexOfAmpersand = text.IndexOf('&');
                var portion = indexOfAmpersand == -1 ? text : text.Slice(0, indexOfAmpersand);
                text = text.Slice(indexOfAmpersand + 1); // this would be a nice case for tail-call recursion of c# had that

                int indexOfEquals = portion.IndexOf('=');
                string key, value;

                if (indexOfEquals != -1)
                {
                    key = WebUtility.UrlDecode(new string(portion.Slice(0, indexOfEquals)));
                    value = WebUtility.UrlDecode(new string(portion.Slice(indexOfEquals + 1)));
                }
                else
                {
                    key = WebUtility.UrlDecode(new string(portion));
                    value = "";
                }

                queryString.Add(key, value);
            } while (indexOfAmpersand != -1);
        }

        public static void TrimAll(string[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }
        }

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public static string UriDecode(string value) => WebUtility.UrlDecode(value ?? throw new ArgumentNullException("value"));

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public static string UriDecode(string value, Encoding encoding)
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
        public static string HtmlEncode(string value) => WebUtility.HtmlEncode(value ?? throw new ArgumentNullException("value"));

        [Obsolete("Use the .NET builtin methods in System.Net.WebUtility intead.")]
        public static string HtmlDecode(string value) => WebUtility.HtmlDecode(value ?? throw new ArgumentNullException("value"));
    }
}
