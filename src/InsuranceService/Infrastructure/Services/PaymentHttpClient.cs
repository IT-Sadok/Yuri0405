using Application.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services;

public class PaymentHttpClient : IPaymentHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/Payment");
            httpRequest.Content = content;
            httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResponse { Success = false };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseJson, _jsonOptions);

            if (paymentResponse != null)
            {
                paymentResponse.Success = true;
                return paymentResponse;
            }

            return new PaymentResponse { Success = false };
        }
        catch
        {
            return new PaymentResponse { Success = false };
        }
    }
}
