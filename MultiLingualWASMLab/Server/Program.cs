using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using MultiLingualWASMLab.Server.Authentication;
using MultiLingualWASMLab.Models;
using Serilog;
using Serilog.Events;
using System.Text;

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
  builder.Services.AddControllersWithViews(opt => {
    //## 禁止自動把 HTTP Response 為 null 時轉換成 204 NoContent。因為 System.Text.Json 無法對 204 NoContent 解序列化！
    // 此設定應該只適用於 Web API。
    // remove formatter that turns nulls into 204 - No Content responses
    opt.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
  });

  builder.Services.AddRazorPages();

  //## for Authentication & Authorization
  var tokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SigningKey"])),
    TokenDecryptionKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"])), // JWE
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
    ValidAudience = builder.Configuration["JwtSettings:Audience"],
  };

  builder.Services.AddAuthentication(option =>
  {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
  }).AddJwtBearer(option =>
  {
#if DEBUG
    option.RequireHttpsMetadata = false;
#endif
    option.SaveToken = true;
    option.TokenValidationParameters = tokenValidationParameters;
  });
  builder.Services.AddAuthorization(option =>
  {
    option.AddPolicy(IdentityAttr.AdminPolicyName, p =>
      p.RequireClaim(IdentityAttr.AdminClaimName, "true"));
  });
  builder.Services.AddSingleton<UserAccountService>();
  builder.Services.AddSingleton(tokenValidationParameters);

  //## for 健康狀態檢查
  builder.Services.AddHealthChecks()
         .AddSqlServer(new HealthChecks.SqlServer.SqlServerHealthCheckOptions
         {
            ConnectionString = "Unknow",
            CommandText = "SELECT 1;"
         })
         .AddCheck<SimpleHealthCheck>(nameof(SimpleHealthCheck));

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

  /// Serilog - 格式化紀錄內容
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

  //## for Authentication & Authorization
  app.UseAuthentication();
  app.UseAuthorization();

  //## for 健康狀態檢查
  app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
  {
    ResponseWriter = SimpleHealthCheck.WriteHealthCheckUIResponse
  });

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
