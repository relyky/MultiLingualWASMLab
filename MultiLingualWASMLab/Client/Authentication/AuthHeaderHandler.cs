using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiLingualWASMLab.DTO;
using System.Net.Http.Headers;

namespace MultiLingualWASMLab.Client.Authentication;

/// <summary>
/// Custom delegating handler for adding Auth headers to outbound requests
/// </summary>
class AuthHeaderHandler : DelegatingHandler
{
  readonly CustomAuthenticationStateProvider authProvider;

  public AuthHeaderHandler(AuthenticationStateProvider authBase)
  {
    authProvider = (CustomAuthenticationStateProvider)authBase;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    //## 自 token 存放庫取得
    string token = await authProvider.GetTokenAsync();

    //※ potentially refresh token here if it has expired etc.
    //--- 在此實作 refresh token 

    if (!String.IsNullOrWhiteSpace(token))
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await base.SendAsync(request, cancellationToken);
    return resp;
  }
}