namespace Application.DTOs;

public class WebhookPaymentIntentDto
{
    public string Id { get; set; }
    public PaymentErrorDto? LastPaymentError { get; set; }
}

public class PaymentErrorDto
{
    public string? Message { get; set; }
}
