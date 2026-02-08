namespace Application.DTOs;

public class WebhookPaymentIntentModel
{
    public string Id { get; set; }
    public PaymentErrorModel? LastPaymentError { get; set; }
}

public class PaymentErrorModel
{
    public string? Message { get; set; }
}
