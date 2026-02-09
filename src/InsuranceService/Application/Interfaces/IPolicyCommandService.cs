using Application.DTOs;

namespace Application.Interfaces;

public interface IPolicyCommandService
{
    Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request);
}
