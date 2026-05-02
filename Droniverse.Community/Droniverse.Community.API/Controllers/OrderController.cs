using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Lấy ra số lượng các order theo status
        /// </summary>
        [ProducesResponseType(typeof(SuccessResponse<OrderOverviewDto>), StatusCodes.Status200OK)]
        [HttpGet("overview")]
        public async Task<ApiResponse> GetOrdersOverview()
        {
            try
            {
                OrderOverviewDto overview = await _orderService.GetOrdersOverview();
                return SuccessResponse<OrderOverviewDto>.Create(overview, "Lấy thông tin tổng quan đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER101");
            }
        }


        /// <summary>
        /// Lấy danh sách tất cả các đơn hàng cùng với thông tin tổng quan
        /// </summary>
        /// <returns>
        /// 200 OK - Trả về danh sách đơn hàng kèm thông tin tổng quan
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<AllOrdersWithOverviewDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetAllOrders([FromQuery] OrderSearchRequest searchRequest)
        {
            try
            {
                var result = await _orderService.GetAllOrdersWithOverview(searchRequest);
                return SuccessResponse<AllOrdersWithOverviewDto>
                    .Create(result, "Lấy danh sách đơn hàng cùng thông tin tổng quan thành công!");
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
        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(SuccessResponse<OrderResponseDto?>), StatusCodes.Status200OK)]
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
        /// Lấy danh sách (phân trang) các đơn hàng theo clubId (mã câu lạc bộ) - Dành cho Admin và System Manager
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="currentPage">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên một trang</param>
        /// <returns></returns>
        [HttpGet("clubs/{clubId:guid}")]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<OrderResponseDto?>>>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetOrdersByClubId(Guid clubId, [FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (currentPage < 1) currentPage = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var orders = await _orderService.GetOrdersByClubIdWithPagination(clubId, currentPage, pageSize);
                return SuccessResponse<PaginationResult<IEnumerable<OrderResponseDto?>>>
                    .Create(orders, "Lấy danh sách đơn hàng theo ClubID thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER104");
            }
        }

        ///// <summary>
        ///// Lấy danh sách (phân trang) các đơn hàng của câu lạc bộ hiện tại - Dành cho Club Manager
        ///// </summary>
        ///// <param name="currentPage">Trang hiện tại (bắt đầu từ 1)</param>
        ///// <param name="pageSize">Số bản ghi trên một trang</param>
        ///// <returns></returns>
        //[HttpGet("my-club")]
        //[Authorize(Roles = Roles.ClubManager)]
        //[ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<OrderResponseDto?>>>), StatusCodes.Status200OK)]
        //public async Task<ApiResponse> GetOrdersByCurrentClub([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        if (currentPage < 1) currentPage = 1;
        //        if (pageSize < 1) pageSize = 10;
        //        if (pageSize > 100) pageSize = 100;

        //        var orders = await _orderService.GetOrdersByCurrentClubWithPagination(currentPage, pageSize);
        //        return SuccessResponse<PaginationResult<IEnumerable<OrderResponseDto?>>>
        //            .Create(orders, "Lấy danh sách đơn hàng của người dùng hiện tại thành công!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResponse.Create(ex.Message, "ER105");
        //    }
        //}

        /// <summary>
        /// Lấy danh sách (phân trang) các đơn hàng theo người dùng hiện tại - Dành cho Club Manager và Club Member
        /// </summary>
        /// <param name="currentPage">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên một trang</param>
        /// <returns></returns>
        [HttpGet("me")]
        [Authorize(Roles = Roles.ClubRoles)]
        [ProducesResponseType(typeof(SuccessResponse<OrderResponseDto?>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetOrdersByCurrentUser([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (currentPage < 1) currentPage = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var orders = await _orderService.GetOrdersByCurrentUserWithPagination(currentPage, pageSize);
                return SuccessResponse<PaginationResult<IEnumerable<OrderResponseDto?>>>
                    .Create(orders, "Lấy danh sách đơn hàng của người dùng hiện tại thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER106");
            }
        }


        /// <summary>
        /// Người mua chủ động hủy đơn hàng
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPut("{orderId:guid}/cancel")]
        public async Task<ApiResponse> CancelOrder(Guid orderId)
        {
            try
            {
                var result = await _orderService.CancelOrder(orderId);
                if (!result)
                {
                    return ErrorResponse.Create("Hủy đơn hàng thất bại.", "ER107");
                }
                return SuccessResponse<bool>.Create(true, "Hủy đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER108");
            }
        }

        ///// <summary>
        ///// Người mua sau khi nhận được hàng thì gọi api này
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns></returns>

        //[HttpPut("{orderId:guid}/received")]
        //public async Task<ApiResponse> ReceiveOrder(Guid orderId)
        //{
        //    try
        //    {
        //        var result = await _orderService.ReceiveOrder(orderId);
        //        if (!result)
        //        {
        //            return ErrorResponse.Create("Xác nhận nhận hàng thất bại.", "ER109");
        //        }
        //        return SuccessResponse<bool>.Create(true, "Xác nhận nhận hàng thành công!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResponse.Create(ex.Message, "ER110");
        //    }
        //}
    }
}