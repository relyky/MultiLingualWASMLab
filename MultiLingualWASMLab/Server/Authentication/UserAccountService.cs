namespace MultiLingualWASMLab.Server.Authentication;

public class UserAccountService
{
  readonly List<UserAccount> _userAccountList = default!; // 模擬使用者登入資料庫

  public UserAccountService()
  {
    _userAccountList = new List<UserAccount>()
    {
      new() { UserId = "admin", UserName = "系統管理員", Mima = "admin", Role = "Admin"},
      new() { UserId = "user" , UserName = "使用者"    , Mima = "user" , Role = "User"},
    };
  }

  internal UserAccount? GetUserAccount(string userId)
  {
    System.Threading.SpinWait.SpinUntil(() => false, 1000); // 模擬 DB 取存花了１秒。
    return _userAccountList.FirstOrDefault(c => c.UserId.Equals(userId));
  }
}
