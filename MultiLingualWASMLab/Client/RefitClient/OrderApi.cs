using Refit;
using MultiLingualWASMLab.DTO;

namespace MultiLingualWASMLab.Client.RefitClient;

public interface IOrderApi
{
  [Post("/api/Order/SaveOrder")]
  Task<OrderModel> SaveOrderAsync(OrderModel formData);

  [Post("/api/Order/GetOrder/{id}")]
  Task<OrderModel> GetOrderAsync(string id);
}
