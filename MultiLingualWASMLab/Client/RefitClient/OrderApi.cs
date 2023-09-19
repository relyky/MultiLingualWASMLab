using Refit;
using MultiLingualWASMLab.DTO;

namespace MultiLingualWASMLab.Client.RefitClient;

public interface IOrderApi
{
  [Post("/api/Order/SaveOrder")]
  Task<OrderModel> SaveOrder(OrderModel formData);
}
