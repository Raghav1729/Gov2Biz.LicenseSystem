using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gov2Biz.PaymentService.Data;
using Gov2Biz.PaymentService.CQRS.Handlers;
using Gov2Biz.PaymentService.CQRS.Commands;
using Gov2Biz.PaymentService.CQRS.Queries;
using Gov2Biz.PaymentService.Services;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddTransient<IRequestHandler<CreatePaymentCommand, PaymentDto>, CreatePaymentHandler>();
builder.Services.AddTransient<IRequestHandler<GetPaymentQuery, PaymentDto>, GetPaymentHandler>();
builder.Services.AddTransient<IRequestHandler<GetPaymentsQuery, Gov2Biz.Shared.Responses.PagedResult<PaymentDto>>, GetPaymentsHandler>();
builder.Services.AddTransient<IRequestHandler<RefundPaymentCommand, PaymentDto>, RefundPaymentHandler>();
builder.Services.AddTransient<IRequestHandler<GetUserPaymentsQuery, List<PaymentDto>>, GetUserPaymentsHandler>();
builder.Services.AddTransient<IRequestHandler<GetPaymentStatsQuery, PaymentStatsDto>, GetPaymentStatsHandler>();
builder.Services.AddTransient<IRequestHandler<GetPaymentByTransactionIdQuery, PaymentDto>, GetPaymentByTransactionIdHandler>();

// Register payment gateways with keyed services
builder.Services.AddKeyedSingleton<IPaymentGateway, StripePaymentGateway>("stripe");
builder.Services.AddKeyedSingleton<IPaymentGateway, PayPalPaymentGateway>("paypal");

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Gov2Biz Payment Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
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
