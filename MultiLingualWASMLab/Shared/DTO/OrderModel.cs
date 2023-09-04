using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.DTO;

public class OrderModel
{
  public string Name { get; set; } = "郝聰明";
  public string Email { get; set; } = "smart@mail.server";
  public string CCNumber { get; set; } = "4012 8888 8888 1881";
  public AddressModel Address { get; set; } = new AddressModel();
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
  public string Address { get; set; } = "中和區中正路７５５號";
  public string City { get; set; } = "新北市";
  public string Country { get; set; } = "中華民國";
}

public class OrderDetailsModel
{
  public string Description { get; set; }
  public decimal Offer { get; set; }
}

/// <summary>
/// A standard AbstractValidator which contains multiple rules and can be shared with the back end API
/// </summary>
/// <typeparam name="OrderModel"></typeparam>
public class OrderValidator : AbstractValidator<OrderModel>
{
  public OrderValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty()
        .Length(1, 100);

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
