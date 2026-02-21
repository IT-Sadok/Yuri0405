using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Application.Queries;

namespace Application.Handlers.Queries;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, PagedResponse<OrderResponse>>
{
    public async Task<PagedResponse<OrderResponse>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (orders, totalCount) = await orderRepository.GetAllWithPolicyAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                PolicyId = o.PolicyId,
                PolicyName = o.Policy.Name,
                ProductType = o.Policy.ProductType,
                CustomerId = o.CustomerId,
                CustomerName = o.CustomerName,
                PremiumAmount = o.PremiumAmount,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                Status = o.Status,
                PaymentReferenceId = o.PaymentReferenceId,
                CreatedAt = o.CreatedAt
            }),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
