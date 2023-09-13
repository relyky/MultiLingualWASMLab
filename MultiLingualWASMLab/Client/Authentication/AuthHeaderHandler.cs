using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MultiLingualWASMLab.DTO;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MultiLingualWASMLab.Client.Authentication;

/// <summary>
/// Custom delegating handler for adding Auth headers to outbound requests
/// </summary>
class AuthHeaderHandler : DelegatingHandler
{
  readonly CustomAuthenticationStateProvider authProvider;
  readonly IWebAssemblyHostEnvironment env;

  public AuthHeaderHandler(AuthenticationStateProvider authBase, IWebAssemblyHostEnvironment _env)
  {
    authProvider = (CustomAuthenticationStateProvider)authBase;
    env = _env;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    //## 自 token 存放庫取得
    string token = await authProvider.GetTokenAsync();

    //※ potentially refresh token here if it has expired etc.
    if(true /* token 快過期了 */)
    {
      AuthToken? newToken = await RefreshTokenAsync(token);
      if (newToken != null)
      {
        //# 更新登入狀態
        await authProvider.UpdateTokenAsync(newToken);
        //# 更新 bearer token
        token = newToken.Token;
      }
    }

    //## GO
    if (!String.IsNullOrWhiteSpace(token))
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await base.SendAsync(request, cancellationToken);
    return resp;
  }

  protected async Task<AuthToken?> RefreshTokenAsync(string token)
  {
    try
    {
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Account/RefreshToken");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      HttpClient http = new HttpClient { BaseAddress = new Uri(env.BaseAddress) };
      var resp = await http.SendAsync(request);

      if (resp.IsSuccessStatusCode)
      {
        var newToken = await resp.Content.ReadFromJsonAsync<AuthToken>();
        return newToken;
      }

      //# for DEBUG
      //string? reason = resp.ReasonPhrase;
      //string? respContent = await resp.Content.ReadAsStringAsync();

      return null;
    }
    catch(Exception ex)
    {
      //# for DEBUG
      //string? reason = ex.Message;

      return null;
    }
  }
}