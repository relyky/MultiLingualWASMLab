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

  public AuthUser? GenerateJwtToken(AuthUser authUser) //--- 拆成二段：把認證與授權分開
  {
    ClaimsIdentity identity = new();
    identity.AddClaim(new Claim(ClaimTypes.Name, authUser.UserId));
    identity.AddClaim(new Claim(ClaimTypes.GivenName, authUser.UserName));
    identity.AddClaims(authUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    (string jwtToken, DateTime expiresUtcTime) = DoMakeToken(identity);

    // fill in the token information
    authUser = authUser with
    {
      Token = new AuthToken
      {
        Token = jwtToken,
        ExpiresUtcTime = expiresUtcTime
      }
    };

    return authUser;
  }

  public AuthToken? RefreshJwtToken(string token)
  {
    var claimsUser = GetPrincipalFromToken(token);
    if (claimsUser == null)
      return null;

    // 再檢查一次過期時限...未實作

    //## 產生 token
    ClaimsIdentity? identity = claimsUser.Identity as ClaimsIdentity;
    if (identity == null)
      return null;

    (string jwtToken, DateTime expiresUtcTime) = DoMakeToken(identity);

    var newToken = new AuthToken
    {
      Token = jwtToken,
      ExpiresUtcTime = expiresUtcTime
    };

    return newToken;
  }

  (string jwtToken, DateTime expiresUtcTime) DoMakeToken(ClaimsIdentity identity)
  {
    var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"]!);
    var expiresUtcTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_config.GetValue<double>("JwtSettings:TokenLifetimeMinutes")));

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
    {
      Subject = identity,
      Expires = expiresUtcTime,
      Issuer = _config["JwtSettings:Issuer"],
      Audience = _config["JwtSettings:Audience"],
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    });

    string jwtToken = tokenHandler.WriteToken(token);

    return (jwtToken, expiresUtcTime);
  }

  /// <summary>
  /// helper
  /// </summary>
  ClaimsPrincipal? GetPrincipalFromToken(string token)
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

  /// <summary>
  /// helper
  /// </summary>
  bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
  {
    return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
      jwtSecurityToken.Header.Alg.Equals(value: SecurityAlgorithms.HmacSha256,
        StringComparison.InvariantCultureIgnoreCase);
  }
}
