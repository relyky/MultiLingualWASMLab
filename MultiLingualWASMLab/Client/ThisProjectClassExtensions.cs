using Blazored.LocalStorage;
using Blazored.LocalStorage.StorageOptions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MultiLingualWASMLab.Client;

public static class ThisProjectClassExtensions
{
  /// <summary>
  /// 設定現在語系：依 localStorage 的 culture 屬性值設定現在語系。
  /// </summary>
  public async static Task SetDefaultCultureAsync(this WebAssemblyHost host)
  {
    var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
    var cultureString = await localStorage.GetItemAsync<string>("culture");

    CultureInfo cultureInfo = !String.IsNullOrWhiteSpace(cultureString)
        ? new CultureInfo(cultureString)
        : new CultureInfo(LocalizerSettings.NeutralCulture.Culture);

    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
  }

}

/// <summary>
/// global type handling helper
/// </summary>
public static class GT
{
  /// <summary>
  /// 自 Property 的 DisplayAttribute 屬性取 Title。
  /// get DisplayAttribute Name of a property of a model. 
  /// </summary>
  public static string Display<TProperty>(Expression<Func<TProperty>> f)
  {
    MemberExpression? exp = f.Body switch
    {
      MemberExpression member => member,
      UnaryExpression unary => unary.Operand as MemberExpression,
      _ => null
    };

    return exp is null 
      ? "Unknow" 
      : ResolveDisplayName(exp.Member) ?? "Unknow";
  }

  /// <summary>
  /// 於 FluentValidation 註冊 DisplayNameResolver 解析程序。
  /// </summary>
  /// <remarks>
  /// 參考[inferring property names from [Display] attribute]
  ///     (https://docs.fluentvalidation.net/en/latest/upgrading-to-9.html?highlight=display%20name#removed-inferring-property-names-from-display-attribute)
  /// </remarks>
  public static string? ResolveDisplayName(MemberInfo member)
  {
    //## 支援多國語系
    //# 依 [PropertyName("欄位名稱","zh-TW")] 取 PropertyName，限定語系為 "zh-TW"
    //# 依 [PropertyName("欄位名稱","en-US")] 取 PropertyName，限定語系為 "en-US"
    //# 依 [PropertyName("欄位名稱")] 取 PropertyName 不指定語系
    string culture = CultureInfo.CurrentUICulture.Name;
    var withPropertyName = member?.GetCustomAttributes(typeof(MultiLingualWASMLab.Shared.PropertyNameAttribute), false)
                                  .Select(property => (MultiLingualWASMLab.Shared.PropertyNameAttribute)property)
                                  .Where(c => c.Culture == culture || c.Culture == string.Empty)
                                  .OrderByDescending(c => c.Culture)
                                  .FirstOrDefault();
    if (withPropertyName != null) return withPropertyName.Name;

    //## 依 [Display(Name = "欄位名稱")] 取 PropertyName
    var withDisplay = member?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
    if (withDisplay != null) return withDisplay.GetName();

    //## 依 [Label("欄位名稱")] 取 PropertyName
    var withLabel = member?.GetCustomAttributes(typeof(MudBlazor.LabelAttribute), false).FirstOrDefault() as MudBlazor.LabelAttribute;
    if (withLabel != null) return withLabel.Name;

    return null;
  }
}
