using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiLingualWASMLab.DTO;
using System.Security.Claims;
using System.Text.Json;

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
      var authUser = await _sessionStorage.ReadEncryptedItemAsync<AuthUser>("UserSession");
      if(authUser is null)
        return new AuthenticationState(anonymous);

      // GO
      ClaimsIdentity identity = new ClaimsIdentity("JwtAuth");
      identity.AddClaim(new Claim(ClaimTypes.Name, authUser.UserId));
      identity.AddClaim(new Claim(ClaimTypes.GivenName, authUser.UserName));
      identity.AddClaims(authUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

      ///※ 加值 UserData 依完整的登入者資料(AuthUser) 
      identity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(authUser)));

      ClaimsPrincipal principal = new ClaimsPrincipal(identity);
      return new AuthenticationState(principal);
    }
    catch 
    {
      return new AuthenticationState(anonymous); 
    }
  }

  /// <summary>
  /// 更新登入狀態並通告
  /// </summary>
  internal async Task UpdateAuthenticationStateAsync(AuthUser? authUser)
  {
    if (authUser != null)
    {
      await _sessionStorage.SaveItemEncryptedAsync("UserSession", authUser);
      var authState = await this.GetAuthenticationStateAsync();
      NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
    else
    {
      await _sessionStorage.RemoveItemAsync("UserSession");
      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
  }

  internal async Task UpdateTokenAsync(AuthToken newToken)
  {
    try
    {
      var authUser = await _sessionStorage.ReadEncryptedItemAsync<AuthUser>("UserSession");
      if (authUser == null) return;

      authUser = authUser with { Token = newToken };
      await this.UpdateAuthenticationStateAsync(authUser); // 更新登入狀態並通告
    }
    catch { /* 防止當掉 */ }
  }

  internal async Task<string> GetTokenAsync()
  {
    var result = string.Empty;

    try
    {
      var authUser = await _sessionStorage.ReadEncryptedItemAsync<AuthUser>("UserSession");
      if (authUser != null && DateTime.UtcNow < authUser.Token.ExpiresUtcTime)
        result = authUser.Token.Token;
    }
    catch{ /* 防止當掉 */ }

    return result;
  }

  //internal async Task<AuthUser?> GetAuthDataAsync()
  //{
  //  try
  //  {
  //    var authUser = await _sessionStorage.ReadEncryptedItemAsync<AuthUser>("UserSession");
  //    return authUser;
  //  }
  //  catch
  //  {
  //    /* 防止當掉 */
  //    return null;
  //  }
  //}
}

internal static class AuthenticationStateClassExtensions
{
  /// <summary>
  /// 依據 AuthenticationState 取出完整的登入者資料。
  /// </summary>
  public static async Task<AuthUser?> UnpackAuthDataAsync(this AuthenticationState authState)
  {
    if (authState.User.Identity?.IsAuthenticated ?? false)
    {
      var claimsIdentity = (ClaimsIdentity)authState.User.Identity;
      string? authUserJson = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
      if (authUserJson == null) return null;
      using var authUserStream = GenerateStreamFromString(authUserJson);
      var result = await JsonSerializer.DeserializeAsync<AuthUser>(authUserStream);
      return result!;
    }

    return null;
  }

  private static MemoryStream GenerateStreamFromString(string str)
  {
    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);
    writer.Write(str);
    writer.Flush();
    stream.Position = 0;
    return stream;
  }
}
