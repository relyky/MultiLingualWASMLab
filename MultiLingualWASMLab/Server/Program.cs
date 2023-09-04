using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Events;

/// 參考：[.NET 6.0 如何使用 Serilog 對應用程式事件進行結構化紀錄](https://blog.miniasp.com/post/2021/11/29/How-to-use-Serilog-with-NET-6)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
  Log.Information("Starting web application");

  var builder = WebApplication.CreateBuilder(args);
  builder.Host.UseSerilog();

  #region //§§ Add services to the container. -------------------------------------------
  builder.Services.AddControllersWithViews();
  builder.Services.AddRazorPages();
  #endregion

  var app = builder.Build();

  #region //§§ Configure the HTTP request pipeline. -------------------------------------
  if (app.Environment.IsDevelopment())
  {
    app.UseWebAssemblyDebugging();
  }
  else
  {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
  }

  app.UseHttpsRedirection();

  app.UseBlazorFrameworkFiles();
  app.UseStaticFiles();

  /// 格式化紀錄內容
  app.UseSerilogRequestLogging(options =>
  {
    // Customize the message template
    options.MessageTemplate = "Handled {UserID} => {RequestScheme} {RequestHost} {RequestPath} {RequestContentType} => {ResponseStatus} {ResponseContentType} ";

    //// Emit debug-level events instead of the defaults
    //options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
      diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
      diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
      diagnosticContext.Set("UserID", httpContext.User.Identity?.Name ?? "guest");
      diagnosticContext.Set("RequestContentType", httpContext.Request.ContentType);
      diagnosticContext.Set("ResponseStatus", httpContext.Response.StatusCode);
      diagnosticContext.Set("ResponseContentType", httpContext.Response.ContentType);
    };
  });

  app.UseRouting();

  #endregion

  app.MapRazorPages();
  app.MapControllers();
  app.MapFallbackToFile("index.html");

  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
