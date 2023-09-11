using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

  public AccountController(UserAccountService userAccountService)
  {
    _userAccountService = userAccountService;
  }

  [HttpPost]
  [Route("Login")]
  [AllowAnonymous]
  public ActionResult<UserSession> Login([FromBody] LoginRequest request)
  {
    var jwtAuthenticationManager = new JwtAuthenticationManager(_userAccountService);
    var userSession = jwtAuthenticationManager.GenerateJwtToken(request.UserName, request.Mima);
    if (userSession is null)
      return Unauthorized();

    return userSession;
  }

  [HttpPost]
  [Route("RefreshToken")]
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

    var jwtAuthenticationManager = new JwtAuthenticationManager(_userAccountService);
    var userSession = jwtAuthenticationManager.RefreshJwtToken(token);
    if (userSession is null)
      return Unauthorized();

    return userSession;
  }
}
