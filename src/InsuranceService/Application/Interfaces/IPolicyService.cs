using Application.DTOs;

namespace Application.Interfaces;

public interface IPolicyService
{
    Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request);
    Task<PolicyResponse?> GetPolicyByIdAsync(Guid id);
    Task<PagedResponse<PolicyResponse>> GetAllPoliciesAsync(int page = 1, int pageSize = 10);
}
