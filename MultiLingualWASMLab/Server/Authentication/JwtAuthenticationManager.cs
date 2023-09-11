using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using MultiLingualWASMLab.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiLingualWASMLab.Server.Authentication;

public class JwtAuthenticationManager
{
  public const string JWT_SECURITY_KEY = "1234567890123456789012345678901234567890"; //--- 想辦法利用 HASH + today + salt 算的算出來。
  private const int JWT_TOKEN_VALIDITY_MINUTES = 20; //--- 參數化

  UserAccountService _userAccountService;
  
  public JwtAuthenticationManager(UserAccountService userAccountService)
  {
    _userAccountService = userAccountService;
  }

  public UserSession? GenerateJwtToken(string userName, string mima) //--- 拆成二段：把認證與授權分開
  {
    if(String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(mima))
      return null;

    /* Validating the User Credentials */
    // ---- 認證登入者
    var userAccount = _userAccountService.GetUserAccount(userName);
    if (userAccount == null || userAccount.Mima != mima)
      return null;

    // --- 取得授權，並轉換成 Role。

    /* Generating JWT Token */
    var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINUTES);
    var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
    var claimsIdentity = new ClaimsIdentity(new List<Claim>
    {
      new Claim(ClaimTypes.Name, userAccount.UserName),
      new Claim(ClaimTypes.Role, userAccount.Role)
    });

    var signingCredentials = new SigningCredentials(
      new SymmetricSecurityKey(tokenKey),
      SecurityAlgorithms.HmacSha256Signature);

    var securityTokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = claimsIdentity,
      Expires = tokenExpiryTimeStamp,
      SigningCredentials = signingCredentials
    };

    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
    var token = jwtSecurityTokenHandler.WriteToken(securityToken);

    /* Returning the User Session object */
    var userSession = new UserSession
    {
      UserName = userAccount.UserName,
      Role = userAccount.Role,
      Token = token,
      ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds,
      //ExpiryTimeStamp = tokenExpiryTimeStamp 
    };

    return userSession;
  }

  public UserSession? RefreshJwtToken(string token)
  {
    var validatedPrincipal = GetPrincipalFromToken(token);
    if(validatedPrincipal == null) 
      return null;

    // 再檢查一次過期時限...

    // 
    ClaimsIdentity? claimsIdentity = validatedPrincipal.Identity as ClaimsIdentity;
    if (claimsIdentity == null)
      return null;

    /* Re-generating JWT Token */
    var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINUTES);
    var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);

    var signingCredentials = new SigningCredentials(
      new SymmetricSecurityKey(tokenKey),
      SecurityAlgorithms.HmacSha256Signature);

    var securityTokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = claimsIdentity,
      Expires = tokenExpiryTimeStamp,
      SigningCredentials = signingCredentials
    };

    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
    var newToken = jwtSecurityTokenHandler.WriteToken(securityToken);

    /* Returning the User Session object */
    var userSession = new UserSession
    {
      UserName = claimsIdentity.Name!,
      Role = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!,
      Token = newToken,
      ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds,
      //ExpiryTimeStamp = tokenExpiryTimeStamp 
    };

    return userSession;
  }

  private ClaimsPrincipal? GetPrincipalFromToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();

    //※ copy from Program.cs --- 可以做成 singleton.... ref:https://youtu.be/AU0TIOZhGqs?si=IJIKn__GeC_FC38n&t=416
    var _tokenValidationParameter = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtAuthenticationManager.JWT_SECURITY_KEY)),
      ValidateIssuer = false,
      ValidateAudience = false
    };

    try
    {
      var principal = tokenHandler.ValidateToken(token, _tokenValidationParameter, out var validatedToken);
      if(!IsJwtWithValidSecurityAlgorithm(validatedToken)) 
      {
        return null;
      }

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
