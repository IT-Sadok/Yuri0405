using API.Extensions;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddPaymentHttpClient(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddInsuranceDatabase(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.MigrateAndSeedDatabase();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
