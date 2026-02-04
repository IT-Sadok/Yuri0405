using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == Guid.Empty)
        {
            return Unauthorized("Customer ID not found in token");
        }

        try
        {
            var response = await _orderService.CreateOrderAsync(request, customerId);
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Order.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", customerId);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders()
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == Guid.Empty)
        {
            return Unauthorized("Customer ID not found in token");
        }

        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    private Guid GetCustomerIdFromClaims()
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Guid.Empty;
        }

        return customerId;
    }
}
