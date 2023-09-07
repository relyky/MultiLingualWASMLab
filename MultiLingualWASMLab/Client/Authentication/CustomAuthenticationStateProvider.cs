using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiLingualWASMLab.DTO;
using System.Security.Claims;

namespace MultiLingualWASMLab.Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
  readonly ISessionStorageService _sessionStorage;
  ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

  public CustomAuthenticationStateProvider(ISessionStorageService sessionStorage)
  {
    _sessionStorage = sessionStorage;
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    try
    {
      var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");
      if(userSession is null)
        return new AuthenticationState(anonymous);

      var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
      {
        new Claim(ClaimTypes.Name, userSession.UserName),
        new Claim(ClaimTypes.Role, userSession.Role)
      },"JwtAuth"));

      return new AuthenticationState(claimsPrincipal);
    }
    catch 
    {
      return new AuthenticationState(anonymous); 
    }
  }

  public async Task UpdateAuthenticationStateAsync(UserSession? userSession)
  {
    if(userSession != null)
    {
      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
      {
        new Claim(ClaimTypes.Name, userSession.UserName),
        new Claim(ClaimTypes.Role, userSession.Role)
      })); //--- 不用加 "JwtAuth"？

      userSession.ExpiryTimeStamp = DateTime.Now.AddSeconds(userSession.ExpiresIn); //--- 應該在後端就算好到期時間
      await _sessionStorage.SaveItemEncryptedAsync("UserSession", userSession);
      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
    else
    {
      await _sessionStorage.RemoveItemAsync("UserSession");
      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
  }

  public async Task<string> GetTokenAsync()
  {
    var result = string.Empty;

    try
    {
      var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");
      if (userSession != null && DateTime.Now < userSession.ExpiryTimeStamp)
        result = userSession.Token;
    }
    catch{ /* 防止當掉 */ }

    return result;
  }
}
