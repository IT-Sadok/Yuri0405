using Application.DTOs;

namespace Application.Interfaces;

public interface IPolicyService
{
    Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request, Guid customerId);
    Task<PolicyResponse?> GetPolicyByIdAsync(Guid id);
    Task<IEnumerable<PolicyResponse>> GetPoliciesByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<PolicyResponse>> GetAllPoliciesAsync();
}
