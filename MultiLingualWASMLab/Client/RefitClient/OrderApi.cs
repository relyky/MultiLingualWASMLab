using Refit;
using MultiLingualWASMLab.DTO;

namespace MultiLingualWASMLab.Client.RefitClient;

public interface IOrderApi
{
  [Post("/api/Order")]
  Task<OrderModel> SaveOrder(OrderModel formData);
}
