using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MultiLingualWASMLab.DTO;
using MultiLingualWASMLab.Server.Authentication;
using System.Net.Http.Headers;
using System.Text;

namespace MultiLingualWASMLab.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
  readonly UserAccountService _userAccountService;
  readonly IConfiguration _config;
  readonly TokenValidationParameters _tokenValidationParameters;

  public AccountController(UserAccountService userAccountService, IConfiguration config, TokenValidationParameters tokenValidationParameters)
  {
    _userAccountService = userAccountService;
    _config = config;
    _tokenValidationParameters = tokenValidationParameters;
  }

  [HttpPost("[action]")]
  [AllowAnonymous]
  public ActionResult<UserSession> Login([FromBody] LoginRequest request)
  {
    //## Validating the User Credentials
    if (String.IsNullOrWhiteSpace(request.UserName) || String.IsNullOrWhiteSpace(request.Mima))
      return Unauthorized();

    // ---- 認證登入者
    UserAccount? userAccount = _userAccountService.GetUserAccount(request.UserName);
    if (userAccount == null || userAccount.Mima != request.Mima)
      return Unauthorized();

    //※ 已通過帳密檢查

    var jwtAuthenticationManager = new JwtAuthenticationManager(_userAccountService, _config, _tokenValidationParameters);
    var userSession = jwtAuthenticationManager.GenerateJwtToken(userAccount);
    if (userSession is null)
      return Unauthorized();

    return userSession;
  }

  [HttpPost("[action]")]
  [Authorize]
  public ActionResult<UserSession> RefreshToken([FromHeader] string authorization)
  {
    string? token = null;
    if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
    {
      // we have a valid AuthenticationHeaderValue that has the following details:
      // scheme will be "Bearer"
      var scheme = headerValue.Scheme;
      // parmameter will be the token itself.
      token = headerValue.Parameter;
    }

    if (token is null)
      return null!;

    var jwtAuthenticationManager = new JwtAuthenticationManager(_userAccountService, _config, _tokenValidationParameters);
    var userSession = jwtAuthenticationManager.RefreshJwtToken(token);
    if (userSession is null)
      return Unauthorized();

    return userSession;
  }
}

public class TokenGenerationRequest
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = string.Empty;
  public Dictionary<string, string>? CustomClaims { get; set; }
}
