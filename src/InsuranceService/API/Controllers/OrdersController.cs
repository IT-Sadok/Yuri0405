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
    private readonly IOrderCommandService _commandService;
    private readonly IOrderQueryService _queryService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderCommandService commandService,
        IOrderQueryService queryService,
        ILogger<OrdersController> logger)
    {
        _commandService = commandService;
        _queryService = queryService;
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
            var response = await _commandService.CreateOrderAsync(request, customerId);
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Order.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", customerId);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == Guid.Empty)
        {
            return Unauthorized("Customer ID not found in token");
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var orders = await _queryService.GetOrdersByCustomerIdAsync(customerId, page, pageSize);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
    {
        var order = await _queryService.GetOrderByIdAsync(id);

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
