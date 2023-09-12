using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using MultiLingualWASMLab.DTO;
using MultiLingualWASMLab.Server.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace MultiLingualWASMLab.Server.Authentication;

class JwtAuthenticationManager
{
  //public const string JWT_SECURITY_KEY = "1234567890123456789012345678901234567890"; //--- 想辦法利用 HASH + today + salt 算的算出來。
  //private const int JWT_TOKEN_VALIDITY_MINUTES = 20; //--- 參數化

  UserAccountService _userAccountService;
  IConfiguration _config;
  TokenValidationParameters _tokenValidationParameters;

  public JwtAuthenticationManager(UserAccountService userAccountService, IConfiguration config, TokenValidationParameters tokenValidationParameters)
  {
    _userAccountService = userAccountService;
    _config = config;
    _tokenValidationParameters = tokenValidationParameters;
  }

  public UserSession? GenerateJwtToken(UserAccount account) //--- 拆成二段：把認證與授權分開
  {
    var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"]!);

    var claims = new List<Claim>
      {
        new Claim(ClaimTypes.Name, account.UserName),
        new Claim(ClaimTypes.Role, account.Role)
      };

    var tokenHandler = new JwtSecurityTokenHandler();
    var expiresUtcTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_config.GetValue<double>("JwtSettings:TokenLifetimeMinutes")));
    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = expiresUtcTime,
      Issuer = _config["JwtSettings:Issuer"],
      Audience = _config["JwtSettings:Audience"],
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    });

    string jwtToken = tokenHandler.WriteToken(token);

    /* Returning the User Session object */
    var userSession = new UserSession
    {
      UserName = account.UserName,
      Role = account.Role,
      Token = jwtToken,
      ExpiresUtcTime = expiresUtcTime
    };
    return userSession;
  }

  public UserSession? RefreshJwtToken(string token)
  {
    var claimsUser = GetPrincipalFromToken(token);
    if (claimsUser == null)
      return null;

    // 再檢查一次過期時限...未實作

    //## 產生 token
    ClaimsIdentity? claimsIdentity = claimsUser.Identity as ClaimsIdentity;
    if (claimsIdentity == null)
      return null;

    /* Re-generating JWT Token */
    var expiresUtcTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_config.GetValue<double>("JwtSettings:TokenLifetimeMinutes")));
    var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"]!);

    var tokenHandler = new JwtSecurityTokenHandler();
    var newToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
    {
      Subject = claimsIdentity,
      Expires = expiresUtcTime,
      Issuer = _config["JwtSettings:Issuer"],
      Audience = _config["JwtSettings:Audience"],
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    });

    string newJwtToken = tokenHandler.WriteToken(newToken);

    /* Returning the User Session object */
    var userSession = new UserSession
    {
      UserName = claimsIdentity.Name!,
      Role = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!,
      Token = newJwtToken,
      ExpiresUtcTime = expiresUtcTime
    };

    return userSession;
  }

  private ClaimsPrincipal? GetPrincipalFromToken(string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
      if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
        return null;

      return principal;
    }
    catch
    {
      return null;
    }
  }

  private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
  {
    return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
      jwtSecurityToken.Header.Alg.Equals(value: SecurityAlgorithms.HmacSha256,
        StringComparison.InvariantCultureIgnoreCase);
  }
}
