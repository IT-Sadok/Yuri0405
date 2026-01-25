using Application.DTOs;

namespace Application.Interfaces;

public enum PolicyActivationResult
{
    Success,
    PolicyNotFound,
    AlreadyProcessed,
    InvalidStatus
}

public interface IPolicyService
{
    Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request, Guid customerId);
    Task<PolicyResponse?> GetPolicyByIdAsync(Guid id);
    Task<IEnumerable<PolicyResponse>> GetPoliciesByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<PolicyResponse>> GetAllPoliciesAsync();
    Task<PolicyActivationResult> ActivatePolicyAsync(Guid policyId, string paymentReferenceId);
}
