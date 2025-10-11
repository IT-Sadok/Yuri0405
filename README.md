# Yuri0405

## PaymentService

A payment processing microservice built with ASP.NET Core 8.0 that provides RESTful API endpoints for handling payment transactions with multiple payment gateway providers.

### Features

- **Payment Processing**: Process payments through configurable payment gateway providers
- **Payment Retrieval**: Query payment status and details by payment ID
- **Idempotency Support**: Prevents duplicate payment processing using idempotency keys
- **Gateway Abstraction**: Factory pattern implementation for multiple payment gateway support
- **In-Memory Database**: Entity Framework Core with in-memory database for development
- **API Documentation**: Integrated Swagger/OpenAPI documentation

### Technology Stack

- **.NET 8.0**: Target framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core 8.0**: ORM for database operations
- **EF Core In-Memory**: In-memory database provider for testing/development
- **Swagger/OpenAPI**: API documentation and testing interface

### Project Structure

```
PaymentService/
├── Controllers/
│   └── PaymentController.cs          # API endpoints for payment operations
├── Data/
│   └── PaymentDbContext.cs           # EF Core database context
├── Gateways/
│   ├── IPaymentGateway.cs            # Payment gateway interface
│   ├── IPaymentGatewayFactory.cs     # Gateway factory interface
│   ├── PaymentGatewayFactory.cs      # Gateway factory implementation
│   └── MockGateway.cs                # Mock payment gateway for testing
├── Models/
│   ├── DTOs/
│   │   ├── PaymentRequest.cs         # Payment creation request DTO
│   │   ├── PaymentResponse.cs        # Payment response DTO
│   │   ├── GatewayChargeRequest.cs   # Gateway charge request DTO
│   │   └── GatewayResponse.cs        # Gateway response DTO
│   ├── Entities/
│   │   └── Payment.cs                # Payment entity model
│   └── Enums/
│       └── PaymentStatus.cs          # Payment status enumeration
├── Services/
│   ├── IPaymentService.cs            # Payment service interface
│   └── PaymentService.cs             # Payment service implementation
└── Program.cs                        # Application entry point and configuration
```

### API Endpoints

#### POST /api/payment
Process a new payment transaction.

**Headers:**
- `Idempotency-Key` (required): Unique key to prevent duplicate processing

**Request Body:**
```json
{
  "userId": "guid",
  "productId": "guid",
  "amount": 100.00,
  "currency": "USD",
  "providerId": 1
}
```

**Response:**
- `200 OK`: Payment processed successfully
- `400 Bad Request`: Invalid request or missing idempotency key
- `500 Internal Server Error`: Processing failure

#### GET /api/payment/{id}
Retrieve payment details by ID.

**Response:**
- `200 OK`: Payment details returned
- `404 Not Found`: Payment not found
- `500 Internal Server Error`: Retrieval failure

### Payment Statuses

- `Pending (0)`: Payment initiated but not yet processed
- `Processing (1)`: Payment is being processed by the gateway
- `Completed (2)`: Payment successfully completed
- `Failed (3)`: Payment processing failed

### Getting Started

#### Prerequisites
- .NET 8.0 SDK
- IDE (Visual Studio, VS Code, or Rider)

#### Running the Application

```bash
# Navigate to the project directory
cd PaymentService

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

The API will be available at `http://localhost:5238` (or configured port).

#### Access Swagger UI
When running in Development mode, access the Swagger UI at:
```
http://localhost:5238/swagger
```

### Configuration

The application uses dependency injection configured in `Program.cs`:
- **PaymentDbContext**: In-memory database for payment storage
- **IPaymentService**: Payment processing service
- **IPaymentGatewayFactory**: Factory for creating payment gateway instances
- **MockGateway**: Mock gateway implementation for testing

### Development Notes

- The project uses an in-memory database that resets on application restart
- Idempotency keys are enforced to prevent duplicate payment processing
- All payment operations are asynchronous for better scalability
- Comprehensive error handling and logging implemented throughout

### Future Enhancements

- Add support for real payment gateway providers (Stripe, PayPal, etc.)
- Implement persistent database storage (PostgreSQL, SQL Server)
- Add payment refund functionality
- Implement webhook handling for async payment notifications
- Add comprehensive unit and integration tests
- Implement payment retry mechanism for failed transactions