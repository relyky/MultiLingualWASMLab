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
  public string Role { get; set; }
  public int ExpiresIn { get; set; }
  public DateTime ExpiryTimeStamp { get; set; } // --- 應該改成 DateTimeOffset
}
