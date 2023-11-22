using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Windows;
using Westwind.AspNetCore.HostedWebServer;
using Westwind.Utilities;
using Westwind.Wpf.Statusbar;

namespace WebServerWpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowModel Model { get; }

        public HostedAspNetWebServer Server { get; set; }
        public StatusbarHelper Statusbar { get; }

        public MainWindow()
        {
            InitializeComponent();
            Model = new MainWindowModel();
            DataContext = Model;
            
            InitializeWebServer();

            // Map helper to Status bar text and icon
            Statusbar = new StatusbarHelper(StatusText, StatusIcon);             
        }

        private void InitializeWebServer()
        {         
            Server = new HostedAspNetWebServer();

            // set up routes/mappings or generic handling (fallback)
            Server.OnMapRequests = (app) =>
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
                    // You can also use this fallback to generically handle requests
                    // based on the incoming ctx.Request.Path
                    
                    // In this case I just return a 404 error

                    // pick up path and manually parse/serve
                    string path = ctx.Request.Path;
                    string verb = ctx.Request.Method;

                    ctx.Response.StatusCode = 404;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync($"<html><body><h1>Invalid Resource - Try again, Punk!</h1></body></html>");
                    await ctx.Response.CompleteAsync();
                });
            };

            // Optionally Intercept Request Start/Completed for logging or UI
            Server.OnRequestCompleted = (ctx, ts) =>
            {
                // Request comes in on non-ui thread!
                Dispatcher.Invoke(() =>
                {
                    var method = ctx.Request.Method.PadRight(8);
                    var path = ctx.Request.Path.ToString();
                    var query = ctx.Request.QueryString.ToString();
                    if (!string.IsNullOrEmpty(query))
                        path += query;
                    var status = ctx.Response.StatusCode;


                    var text = method + path.PadRight(94) +
                               " (" + status + ") " +
                               ts.TotalMilliseconds.ToString("n3") + "ms";

                    Model.AddRequestLine(text);
                    Model.RequestCount++;
                });
            };
        }



        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Statusbar.ShowStatusSuccess("Server started.");
            Server.LaunchAsync(
                "http://localhost:5003", 
                System.IO.Path.GetFullPath("./wwwroot")
                ).FireAndForget();

            Model.RequestText = "*** Web Server started.";
            Model.ServerStatus = "server is running";
        }

        private async void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            await Server.Stop();
            Statusbar.ShowStatusSuccess("Server stopped.");
            Model.ServerStatus = "server is stopped";
        }
    }
}