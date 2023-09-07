using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiLingualWASMLab.DTO;
using MultiLingualWASMLab.Server.Authentication;

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
}
