namespace MultiLingualWASMLab.Server.Authentication;

public class UserAccountService
{
  readonly List<UserAccount> _userAccountList = default!; // 模擬使用者登入資料庫

  public UserAccountService()
  {
    _userAccountList = new List<UserAccount>()
    {
      new() { UserName = "admin", Mima = "admin", Role = "Admin"},
      new() { UserName = "user", Mima = "user", Role = "User"},
    };
  }

  internal UserAccount? GetUserAccount(string userName)
  {
    System.Threading.SpinWait.SpinUntil(() => false, 1000); // 模擬 DB 取存花了１秒。
    return _userAccountList.FirstOrDefault(c => c.UserName.Equals(userName));
  }
}
