using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Westwind.AspNetCore.HostedWebServer
{

    /// <summary>
    /// The simplest implementation of a hosted ASP.NET Server that 
    /// can be embedded into a host application without any other
    /// configuration.
    /// 
    /// Includes hooks for:
    /// * Mapping Requests (routes or generically)
    /// * StartRequest/CompletedRequest
    /// </summary>

    public class HostedAspNetWebServer
    {
        public bool IsRunning { get; set; }

        public WebApplication WebApp { get; set; }

        public string ErrorMessage { get; set; }

        public Exception LastException { get; set; }

        public Action<WebApplication> OnMapRequests { get; set;  }

        public Action<HttpContext> OnRequestStarting { get; set; }

        public Action<HttpContext, TimeSpan> OnRequestCompleted { get; set;  }

        /// <summary>
        /// Launches the Web Server synchronously and
        /// waits for it to shut down
        /// </summary>
        /// <param name="urls">One or more startup urls separated by semicolons (optional default: http://localhost:5003)</param>
        /// <param name="webRootPath">An optional path for static file locations. If not specified uses entry assembly location</param>
        /// <returns></returns>
        public bool Launch(
            string urls = "http://localhost:5003",
            string webRootPath = null)
        {
            ErrorMessage = null;
            LastException = null;
                           
            try
            {
                var options = new WebApplicationOptions()
                {
                    WebRootPath =  webRootPath,
                    ContentRootPath = webRootPath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)

                };
                var webBuilder = WebApplication.CreateBuilder(options);                    
                webBuilder.WebHost.UseUrls(urls);                
                WebApp = webBuilder.Build();

                // Allow Host to Intercept start and end requests to display UI/Logging etc.
                WebApp.Use(RequestInterceptorMiddlewareHandler);

                // Optionally support static files
                if (!string.IsNullOrEmpty(webRootPath))
                {
                    var staticFileOptions = new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(webRootPath),
                        RequestPath = new PathString(""),
                        DefaultContentType = "application/octet-stream"
                    };
                    WebApp.UseStaticFiles(staticFileOptions);
                }

                // Map your Requests
                OnMapRequests?.Invoke(WebApp);

                IsRunning = true;

                WebApp.Run();
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                ErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// Launches the Web Server in the background so you can continue
        /// processing
        /// so you 
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="webRootPath"></param>
        public async Task<bool> LaunchAsync(
            string urls = "http://localhost:5003",
            string webRootPath = null)
        {
            bool result = false;

            return await Task.Run(() => Launch(urls, webRootPath));
        }


        public async Task Stop()
        {
            await WebApp.StopAsync();
            IsRunning = false;
        }



        /// <summary>
        /// This middle ware handler intercepts every request captures the time
        /// and then logs out to the screen (when that feature is enabled) the active
        /// request path, the status, processing time.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        async Task RequestInterceptorMiddlewareHandler(HttpContext context, Func<Task> next)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            if (OnRequestStarting != null)
            {
                OnRequestStarting.Invoke(context);
            }

            await next();

            if (OnRequestCompleted != null)
            {
                OnRequestCompleted.Invoke(context, sw.Elapsed);
            }
        }
    }
         
}
