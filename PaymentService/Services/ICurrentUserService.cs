namespace PaymentService.Services;

public interface ICurrentUserService
{
    Guid? GetUserId();
}
