using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.DTO;

public record AuthUser
{
  public string UserId { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string[] Roles { get; set; } = new string[0];
  public AuthToken Token { get; set; } = new AuthToken();
}

public record AuthToken
{
  public string Token { get; set; } = string.Empty;
  public DateTime? ExpiresUtcTime { get; set; }
}
