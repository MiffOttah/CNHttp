using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using NHttp.Test.Support;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

namespace NHttp.Test.WebRequestFixtures
{
    [TestFixture]
    public class Cookies : FixtureBase
    {
        private const string DateFormat = "ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'";
        private static readonly CultureInfo DateCulture = new CultureInfo("en-US");

        [Test]
        public async Task SetCookie()
        {
            using var server = new HttpServer();
            server.RequestReceived += (sender, e) =>
            {
                e.Response.Cookies["a"].Value = "b";
            };
            server.Start();

            var requestUri = new Uri($"http://{server.EndPoint}/");

            using var handler = new HttpClientHandler();
            using var client = new HttpClient(handler);
            using var response = await client.GetAsync(requestUri);

            string setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            Assert.AreEqual("a=b; path=/", setCookieHeader);
            var cookies = handler.CookieContainer.GetCookies(requestUri);
            Assert.AreEqual(1, cookies.Count);


            var cookie = cookies[0];
            Assert.AreEqual("a", cookie.Name);
            Assert.AreEqual("b", cookie.Value);
            Assert.AreEqual("/", cookie.Path);
            Assert.AreEqual(server.EndPoint.Address.ToString(), cookie.Domain);
            Assert.AreEqual(DateTime.MinValue, cookie.Expires);
            Assert.AreEqual(false, cookie.HttpOnly);
            Assert.AreEqual(false, cookie.Secure);
        }

        [Test]
        public async Task SetCookieWithDetails()
        {
            var expires = DateTime.Now + TimeSpan.FromDays(1);
            expires -= TimeSpan.FromMilliseconds(expires.Millisecond);

            using var server = new HttpServer();
            server.RequestReceived += (sender, e) =>
            {
                var cookie = new HttpCookie("a", "b")
                {
                    Path = "/path",
                    HttpOnly = true,

                    // if this flag is set, the cookie will be hoarded by the HttpClientHandler
                    // becuase this isn't a HTTPS connection
                    Secure = false,

                    Expires = expires
                };

                e.Response.Cookies.Add(cookie);
            };
            server.Start();

            var requestUri = new Uri($"http://{server.EndPoint}/path");

            using var handler = new HttpClientHandler();
            using var client = new HttpClient(handler);
            using var response = await client.GetAsync(requestUri);

            string setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            Assert.AreEqual("a=b; expires=" + expires.ToString(DateFormat, DateCulture) + "; path=/path; HttpOnly", setCookieHeader);
            var cookies = handler.CookieContainer.GetCookies(requestUri);
            Assert.AreEqual(1, cookies.Count);


            var cookie = cookies[0];
            Assert.AreEqual("a", cookie.Name);
            Assert.AreEqual("b", cookie.Value);
            Assert.AreEqual("/path", cookie.Path);
            Assert.AreEqual(expires.ToString(CultureInfo.InvariantCulture), cookie.Expires.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(true, cookie.HttpOnly);
            Assert.AreEqual(false, cookie.Secure);
        }

        [Test]
        public async Task SetCookieWithDomain()
        {
            var expires = DateTime.Now + TimeSpan.FromDays(1);
            expires -= TimeSpan.FromMilliseconds(expires.Millisecond);

            using var server = new HttpServer();
            server.RequestReceived += (sender, e) =>
            {
                var cookie = new HttpCookie("a", "b")
                {
                    Domain = "example.com"
                };

                e.Response.Cookies.Add(cookie);
            };
            server.Start();

            var requestUri = new Uri($"http://{server.EndPoint}/path");

            using var handler = new HttpClientHandler();
            using var client = new HttpClient(handler);
            using var response = await client.GetAsync(requestUri);

            string setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            Assert.AreEqual("a=b; domain=example.com; path=/", setCookieHeader);
        }

        [Test]
        public void SetMultipleCookies()
        {
            using (var server = new HttpServer())
            {
                server.RequestReceived += (s, e) =>
                {
                    e.Response.Cookies["a"].Value = "b";
                    e.Response.Cookies["c"].Value = "d";
                };

                server.Start();

                var request = (HttpWebRequest)WebRequest.Create(
                    String.Format("http://{0}/", server.EndPoint)
                );

                request.CookieContainer = new CookieContainer();

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Assert.AreEqual("a=b; path=/, c=d; path=/", response.Headers["Set-Cookie"]);

                    Assert.AreEqual(2, response.Cookies.Count);

                    Assert.AreEqual("a", response.Cookies[0].Name);
                    Assert.AreEqual("b", response.Cookies[0].Value);
                    Assert.AreEqual("c", response.Cookies[1].Name);
                    Assert.AreEqual("d", response.Cookies[1].Value);
                }
            }
        }

        [Test]
        public void CookieRoundTrip()
        {
            TestCookieRoundtrip(
                p =>
                {
                    p.Response.Cookies["a"].Value = "b";
                },
                p =>
                {
                    Assert.That(p.Request.Cookies.AllKeys, Is.EquivalentTo(new[] { "a" }));
                    Assert.AreEqual("b", p.Request.Cookies["a"].Value);
                }
            );
        }

        [Test]
        public void DuplicateCookiesRoundTrip()
        {
            TestCookieRoundtrip(
                p =>
                {
                    p.Response.Cookies.Add(new HttpCookie("a", "b"));
                    p.Response.Cookies.Add(new HttpCookie("a", "c"));
                },
                p =>
                {
                    Assert.That(p.Request.Cookies.AllKeys, Is.EquivalentTo(new[] { "a" }));
                    Assert.AreEqual("c", p.Request.Cookies["a"].Value);
                }
            );
        }

        [Test]
        public void ValueToCollectionRoundTrip()
        {
            TestCookieRoundtrip(
                p =>
                {
                    p.Response.Cookies["a"].Value = "a=b&&c";
                },
                p =>
                {
                    Assert.That(p.Request.Cookies.AllKeys, Is.EquivalentTo(new[] { "a" }));
                    Assert.That(p.Request.Cookies["a"].Values.AllKeys, Is.EquivalentTo(new[] { "a", null, "c" }));
                    Assert.AreEqual("b", p.Request.Cookies["a"].Values["a"]);
                    Assert.AreEqual(null, p.Request.Cookies["a"].Values["c"]);
                }
            );
        }

        [Test]
        public void CollectionToCollectionRoundTrip()
        {
            TestCookieRoundtrip(
                p =>
                {
                    p.Response.Cookies["a"].Values["a"] = "b";
                    p.Response.Cookies["a"].Values["c"] = "d";
                },
                p =>
                {
                    Assert.That(p.Request.Cookies.AllKeys, Is.EquivalentTo(new[] { "a" }));
                    Assert.That(p.Request.Cookies["a"].Values.AllKeys, Is.EquivalentTo(new[] { "a", "c" }));
                    Assert.AreEqual("b", p.Request.Cookies["a"].Values["a"]);
                    Assert.AreEqual("d", p.Request.Cookies["a"].Values["c"]);
                }
            );
        }

        [Test]
        public void InvalidlyParsedCollectionRoundTrip()
        {
            TestCookieRoundtrip(
                p =>
                {
                    p.Response.Cookies["a"].Values["a"] = "b&c=d";
                },
                p =>
                {
                    Assert.That(p.Request.Cookies.AllKeys, Is.EquivalentTo(new[] { "a" }));
                    Assert.That(p.Request.Cookies["a"].Values.AllKeys, Is.EquivalentTo(new[] { "a", "c" }));
                    Assert.AreEqual("b", p.Request.Cookies["a"].Values["a"]);
                    Assert.AreEqual("d", p.Request.Cookies["a"].Values["c"]);
                }
            );
        }

        private void TestCookieRoundtrip(Action<HttpContext> fromCallback, Action<HttpContext> toCallback)
        {
            using (var server = new HttpServer())
            {
                server.RequestReceived += (s, e) =>
                {
                    switch (e.Request.Path)
                    {
                        case "/from":
                            fromCallback(e.Context);

                            e.Response.RedirectPermanent("/to");
                            break;

                        case "/to":
                            toCallback(e.Context);
                            break;
                    }
                };

                server.Start();

                var request = (HttpWebRequest)WebRequest.Create(
                    String.Format("http://{0}/from", server.EndPoint)
                );

                request.CookieContainer = new CookieContainer();

                GetResponseFromRequest(request);
            }
        }
    }
}
