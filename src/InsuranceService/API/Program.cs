using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.HttpHandlers;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add HttpContextAccessor for forwarding auth headers
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

// Add HttpClient for Payment service with auth header forwarding
builder.Services.AddHttpClient<IPaymentHttpClient, PaymentHttpClient>(client =>
{
    var paymentServiceUrl = builder.Configuration["PaymentServiceUrl"];
    client.BaseAddress = new Uri(paymentServiceUrl!);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

// Add Kafka consumer
builder.Services.AddKafkaConsumer(builder.Configuration);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Insurance Service API",
        Version = "v1",
        Description = "Insurance microservice with JWT authentication"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\n\nExample: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply pending migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    db.Database.Migrate();

    if (!db.Policies.Any())
    {
        db.Policies.AddRange(
            new Policy
            {
                Id = Guid.NewGuid(),
                Name = "Basic Health Plan",
                Description = "Essential health coverage for individuals including doctor visits and emergency care",
                ProductType = ProductType.Health,
                CoverageAmount = 50_000m,
                PremiumAmount = 199.99m,
                DurationMonths = 12,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Policy
            {
                Id = Guid.NewGuid(),
                Name = "Full Auto Coverage",
                Description = "Comprehensive auto insurance covering collision, liability, and uninsured motorist",
                ProductType = ProductType.Auto,
                CoverageAmount = 100_000m,
                PremiumAmount = 89.50m,
                DurationMonths = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Policy
            {
                Id = Guid.NewGuid(),
                Name = "Home Protection Plus",
                Description = "Full home insurance covering structure, personal property, and natural disasters",
                ProductType = ProductType.Home,
                CoverageAmount = 250_000m,
                PremiumAmount = 149.00m,
                DurationMonths = 12,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
