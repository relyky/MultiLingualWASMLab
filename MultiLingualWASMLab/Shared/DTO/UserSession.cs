using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.DTO;

public class UserSession
{
  public string UserName { get; set; }
  public string Token { get; set; }
  public string Role { get; set; } //--- 改成 Array<string>
  public DateTime ExpiresUtcTime { get; set; }
  public TimeSpan ExpiresIn => (ExpiresUtcTime - DateTime.UtcNow);
}
