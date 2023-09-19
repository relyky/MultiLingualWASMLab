using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiLingualWASMLab.DTO;
using System;

namespace MultiLingualWASMLab.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
  [HttpPost("[action]")]
  public async Task<ActionResult<OrderModel>> SaveOrder(OrderModel formData)
  {
    var validator = new OrderValidator();
    ValidationResult result = await validator.ValidateAsync(formData);
    if(!result.IsValid)
    {
      string errMsg = result.ToString("~");
      return BadRequest(errMsg);
    }

    formData.Address.Address = "曾經來過";

    return Ok(formData);
  }

}
