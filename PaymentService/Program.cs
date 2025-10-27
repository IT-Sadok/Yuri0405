using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Gateways;
using PaymentService.Helpers;
using PaymentService.Services;

namespace PaymentService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddExceptionHandler<PaymentExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddScoped<MockGateway>();
        builder.Services.AddScoped<StripeGateway>();
        builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        builder.Services.AddDbContext<PaymentDbContext>(options =>
            options.UseInMemoryDatabase("PaymentsDb"));
        builder.Services.AddScoped<IPaymentService, Services.PaymentService>();
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseExceptionHandler();
        app.MapControllers();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}