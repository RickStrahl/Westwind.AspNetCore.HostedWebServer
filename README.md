# Westwind.AspNetCore.HostedWebServer

### An example of hosting a ASP.NET in a desktop or other non-Web application

This is a sample that demonstrates hosting ASP.NET in a non-primary Web application using the simplest thing possible. 

## Integration
Currently there's no NuGet package, so you have to include a project reference to the `Westwind.AspNetCore.HostedWebServer` project into your own solution, or directly use the source files. The library is self-contained in a single `HosteAspnetWebServer.cs` file so it should be easy to integrate into your own projects.

#### Important: You need a Framework Reference to ASP.NET
If you decide to include the library in your own code, you'll need to add a framework reference for the ASP.NET Runtime into your project:

```xml
<ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

If you decide to use the project as reference it'll include that. Regardless of which way you are accessing this functioality:

> [!NOTE]
> **ASP.NET Runtime Files are required!**   In order to use the ASP.NET Hosting components you need to have the ASP.NET Runtime installed or alternately build a self-contained application that will include all the required runtime files. 
>
> Potentially this is an **additional install** beyond say a Desktop Runtime installation that adds additional distribution size to your application.

## Example Usage
The following is a simple example of setting up hostable ASP.NET Server instance in a WPF form and displaying requests in a text block.

```cs
public MainWindow()
{
    InitializeComponent();

    InitializeWebServer();
    
    ... 
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

        app.MapGet("/api/test", async ctx =>
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { Message = "What's up doc?", Time = DateTime.UtcNow });
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
```
  
#### Starting and Stopping the Server
The above configures the server but doesn't start it. You can start it by using:

```cs
private async void Button_Start_Click(object sender, RoutedEventArgs e)
{
    Statusbar.ShowStatusSuccess("Server started.");
    Server.LaunchAsync(
         "http://localhost:5003", 
         System.IO.Path.GetFullPath("./wwwroot")
    ).FireAndForget();  // async - don't wait
    
    Model.RequestText = "*** Web Server started.";
    Model.ServerStatus = "server is running";
}
```

You pass in two things:

* **One or more launch Urls**  
Like an ASP.NET Core application you can specify the host and port on which the server should be started. You can provide multple semi-colon seperated urls for `http://` and `https://` urls. 

* **Static File Resource Path (optional)**  
If you want to serve static files from a local folder you can specify a folder from which files are served. If you don't provide a path, no local resources will be served otherwise the specified folder is mapped for static file handling.

### Https usage
Note if you want to use `https://` urls you need to ensure that certificates are installed and configured for the specified ports or - if you can use the .NET SDK you can use `dotnet dev-cert https -t` to configure and trust the local dotnet certificate. 

There's currently no support to explicitly specify certificates, so you have to use the host OS to provide the certificate and port mappings.

