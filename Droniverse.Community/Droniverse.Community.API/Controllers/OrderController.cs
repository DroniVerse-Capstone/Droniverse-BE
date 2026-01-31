using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("api/orders")]
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
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetOrders();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody]OrderCreateDto orderCreateDto)
        {
            var createdOrder = await _orderService.AddOrder(orderCreateDto);
            if (createdOrder == null)
            {
                return BadRequest("Invalid order data.");
            }
            return CreatedAtAction(nameof(GetOrders), new { id = createdOrder.OrderID }, createdOrder);
        }
    }
}
