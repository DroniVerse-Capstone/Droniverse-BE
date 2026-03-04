using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;
        public OrderController(ILogger<OrderController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ApiResponse> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrders();
                return SuccessResponse<List<OrderResponseDto?>>
                    .Create(orders, "Lấy danh sách đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER2001");
            }
        }

        [HttpPost]
        public async Task<ApiResponse> AddOrder([FromBody]OrderCreateDto orderCreateDto)
        {
            try
            {
                var createdOrder = await _orderService.AddOrder(orderCreateDto);
                if (createdOrder == null)
                {
                    return ErrorResponse.Create("Dữ liệu đơn hàng không hợp lệ.", "ER2002");
                }

                return SuccessResponse<OrderResponseDto?>
                    .Create(createdOrder, "Tạo đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }
    }
}
