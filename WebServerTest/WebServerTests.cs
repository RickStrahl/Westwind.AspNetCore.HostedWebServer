using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Westwind.AspNetCore.HostedWebServer;

namespace WebServerTest
{
    public class WebServerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RunWebServerTest()
        {
            // Path.GetDirectoryName(typeof(WebServerTests).Assembly.Location)
            var server = new HostedAspNetWebServer();
            server.OnMapRequests = (app) =>
            {

                app.MapGet("/test", async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync($"<html><body><h1>Hello Test Request! {DateTime.Now.ToString()}</h1></body></html>");
                    await ctx.Response.CompleteAsync();
                });

                app.MapFallback(async ctx =>
                {
                    // pick up path and manually parse/serve
                    string path = ctx.Request.Path;
                    string verb = ctx.Request.Method;

                    ctx.Response.StatusCode = 404;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync($"<html><body><h1>Hello Fallback! {DateTime.Now.ToString()}</h1></body></html>");
                    await ctx.Response.CompleteAsync();
                });

            };

            // this really won't fire until the server is shut down
            Assert.IsTrue(server.Launch("http://localhost:5003", Path.GetDirectoryName(typeof(WebServerTests).Assembly.Location)), server.ErrorMessage);
        }

        [Test]
        public async Task RunWebServerAsyncTest()
        {
            // Path.GetDirectoryName(typeof(WebServerTests).Assembly.Location)
            var server = new HostedAspNetWebServer();
            server.OnMapRequests = (app) =>
            {

                app.MapGet("/", async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync($"<html><body><h1>Hello Test Request! {DateTime.Now.ToString()}</h1></body></html>");
                    await ctx.Response.CompleteAsync();
                });

                app.MapFallback(async ctx =>
                {
                    // pick up path and manually parse/serve
                    string path = ctx.Request.Path;
                    string verb = ctx.Request.Method;

                    ctx.Response.StatusCode = 404;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync($"<html><body><h1>Fallback World! {DateTime.Now.ToString()}</h1></body></html>");
                    await ctx.Response.CompleteAsync();
                });

            };
            var t =  server.LaunchAsync("http://localhost:5003", Path.GetDirectoryName(typeof(WebServerTests).Assembly.Location));


            // run for 10secs
            await Task.Delay(10000);

            await server.Stop();

            await Task.Delay(2000);

            bool result = await t;

            Console.WriteLine("Server Stopped");

            // this really won't fire until the server is shut down
            Assert.IsTrue(result, server.ErrorMessage);
        }
    }
}
