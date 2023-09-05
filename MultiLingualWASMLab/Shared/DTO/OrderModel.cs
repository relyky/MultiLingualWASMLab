using FluentValidation;
using MultiLingualWASMLab.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.DTO;

public class OrderModel
{
  [PropertyName("名稱", "zh-TW")]
  [PropertyName("Name", "en-US")]
  public string Name { get; set; } = "郝聰明";

  [PropertyName("電子郵件", "zh-TW")]
  [PropertyName("Email", "en-US")]
  public string Email { get; set; } = "smart@mail.server";

  [PropertyName("信用卡號", "zh-TW")]
  [PropertyName("Credit card No", "en-US")]
  public string CCNumber { get; set; } = "4012 8888 8888 1881";

  [PropertyName("地址", "zh-TW")]
  [PropertyName("Address", "en-US")]
  public AddressModel Address { get; set; } = new AddressModel();

  [PropertyName("訂單明細", "zh-TW")]
  [PropertyName("Order details", "en-US")]
  public List<OrderDetailsModel> OrderDetails = new List<OrderDetailsModel>()
    {
      new OrderDetailsModel()
      {
        Description = "Perform Work order 1",
        Offer = 100
      },
      new OrderDetailsModel()
      {
          Description = "炸雞腿",
          Offer = 99
      }
    };
}

public class AddressModel
{
  [PropertyName("地址", "zh-TW")]
  [PropertyName("Address", "en-US")]
  public string Address { get; set; } = "中和區中正路７５５號";

  [PropertyName("城市", "zh-TW")]
  [PropertyName("City", "en-US")]
  public string City { get; set; } = "新北市";

  [PropertyName("國家", "zh-TW")]
  [PropertyName("Country", "en-US")]
  public string Country { get; set; } = "中華民國";
}

public class OrderDetailsModel
{

  [PropertyName("下訂說明", "zh-TW")]
  [PropertyName("Description", "en-US")]
  public string Description { get; set; } = default!;

  [PropertyName("下訂數量", "zh-TW")]
  [PropertyName("Offer", "en-US")]
  public decimal Offer { get; set; }
}

/// <summary>
/// A standard AbstractValidator which contains multiple rules and can be shared with the back end API
/// </summary>
/// <remarks>
/// 參考：[FluentValidation - Overriding the Message](https://docs.fluentvalidation.net/en/latest/configuring.html#overriding-the-message)
/// </remarks>
public class OrderValidator : AbstractValidator<OrderModel>
{
  public OrderValidator()
  {
    var culture = CultureInfo.CurrentUICulture;
    bool zhTW = culture.Name == "zh-TW";
    bool enUS = culture.Name == "en-US";

    RuleFor(x => x.Name)
        .NotEmpty().WithMessage(zhTW ? "{PropertyName} 不可空白哦～" : "{PropertyName} no empty ～。")
        .Length(1, 3).WithMessage(zhTW ? "{PropertyName} 長度不合３～" : "{PropertyName} length invalid ～。")
        .WithName(x => zhTW ? "客製化名稱" : "Customized Name");

    //RuleFor(x => x.Name)
    //    .NotEmpty()
    //    .Length(1, 20);

    RuleFor(x => x.Email)
        .Cascade(CascadeMode.Stop)
        .NotEmpty()
        .EmailAddress()
        .MustAsync(async (value, cancellationToken) => await IsUniqueAsync(value));

    RuleFor(x => x.CCNumber)
        .NotEmpty()
        .Length(1, 100)
        .CreditCard();

    RuleFor(x => x.Address.Address)
        .NotEmpty()
        .Length(1, 100);

    RuleFor(x => x.Address.City)
        .NotEmpty()
        .Length(1, 100);

    RuleFor(x => x.Address.Country)
        .NotEmpty()
        .Length(1, 100);

    RuleForEach(x => x.OrderDetails)
      .SetValidator(new OrderDetailsValidator());
  }

  private async Task<bool> IsUniqueAsync(string email)
  {
    // Simulates a long running http call
    await Task.Delay(2000);
    return email.ToLower() != "test@test.com";
  }

  public Func<object, string, Task<IEnumerable<string>>> Validation => async (model, propertyName) =>
  {
    var result = await ValidateAsync(ValidationContext<OrderModel>.CreateWithOptions((OrderModel)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
      return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
  };
}

/// <summary>
/// A standard AbstractValidator for the Collection Object
/// </summary>
/// <typeparam name="OrderDetailsModel"></typeparam>
public class OrderDetailsValidator : AbstractValidator<OrderDetailsModel>
{
  public OrderDetailsValidator()
  {
    RuleFor(x => x.Description)
        .NotEmpty()
        .Length(1, 100);

    RuleFor(x => x.Offer)
      .GreaterThan(0)
      .LessThan(999);
  }

  public Func<object, string, Task<IEnumerable<string>>> Validation => async (model, propertyName) =>
  {
    var result = await ValidateAsync(ValidationContext<OrderDetailsModel>.CreateWithOptions((OrderDetailsModel)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
      return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
  };
}
