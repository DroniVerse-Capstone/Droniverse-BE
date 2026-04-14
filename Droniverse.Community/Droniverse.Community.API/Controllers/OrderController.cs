using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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

        /// <summary>
        /// Lấy danh sách tất cả các đơn hàng
        /// </summary>
        /// <returns>
        /// 200 OK - Trả về danh sách đơn hàng
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<List<OrderResponseDto?>>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Tạo mới một đơn hàng. #1. Luồng thanh toán
        /// </summary>
        /// <param name="clubId">ID của club</param>
        /// <param name="orderCreateDto">Thông tin đơn hàng cần tạo</param>
        /// <returns>
        /// 200 OK - Tạo đơn hàng thành công
        /// 400 BadRequest - Dữ liệu không hợp lệ
        /// </returns>
        [HttpPost("clubs/{clubId:guid}")]
        [ProducesResponseType(typeof(SuccessResponse<OrderResponseDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ApiResponse> AddOrder(Guid clubId, [FromBody] OrderCreateDto orderCreateDto)
        {
            try
            {
                var createdOrder = await _orderService.AddOrder(clubId, orderCreateDto);
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

        /// <summary>
        /// Lấy thông tin đơn hàng chi tiết theo orderId (mã đơn hàng)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("orderId")]
        public async Task<ApiResponse> GetOrderByOrderId(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByOrderId(orderId);
                if (order == null)
                {
                    return ErrorResponse.Create("Không tìm thấy đơn hàng.", "ER2003");
                }
                return SuccessResponse<OrderResponseDto?>
                    .Create(order, "Lấy thông tin đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER103");
            }
        }

        /// <summary>
        /// Lấy thông tin đơn hàng chi tiết theo clubId (mã câu lạc bộ)
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet("clubs/{clubId:guid}")]
        public async Task<ApiResponse> GetOrdersByClubId(Guid clubId)
        {
            try
            {
                var orders = await _orderService.GetOrderByClubId(clubId);
                return SuccessResponse<IEnumerable<OrderResponseDto?>>
                    .Create(orders, "Lấy danh sách đơn hàng theo ClubID thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER104");
            }
        }
}
