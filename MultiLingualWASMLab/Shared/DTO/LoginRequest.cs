using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.DTO;

public class LoginRequest
{
  public string UserId { get; set; } = string.Empty;
  public string Mima { get; set; } = string.Empty;
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
  public LoginRequestValidator()
  {
    //var culture = CultureInfo.CurrentUICulture;
    //bool zhTW = culture.Name == "zh-TW";
    //bool enUS = culture.Name == "en-US";

    RuleFor(m => m.UserId)
      .NotEmpty();

    RuleFor(m => m.Mima)
      .NotEmpty();
  }

  public Func<object, string, Task<IEnumerable<string>>> Validation => async (model, propertyName) =>
  {
    var result = await ValidateAsync(ValidationContext<LoginRequest>.CreateWithOptions((LoginRequest)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
      return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
  };
}