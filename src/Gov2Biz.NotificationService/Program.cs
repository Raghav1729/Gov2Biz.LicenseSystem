using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gov2Biz.NotificationService.Data;
using Gov2Biz.NotificationService.CQRS.Handlers;
using Gov2Biz.NotificationService.CQRS.Commands;
using Gov2Biz.NotificationService.CQRS.Queries;
using Gov2Biz.NotificationService.Services;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddTransient<IRequestHandler<CreateNotificationCommand, NotificationDto>, CreateNotificationHandler>();
builder.Services.AddTransient<IRequestHandler<GetNotificationQuery, NotificationDto>, GetNotificationHandler>();
builder.Services.AddTransient<IRequestHandler<GetNotificationsQuery, Gov2Biz.Shared.Responses.PagedResult<NotificationDto>>, GetNotificationsHandler>();
builder.Services.AddTransient<IRequestHandler<MarkAsReadCommand, bool>, MarkAsReadHandler>();
builder.Services.AddTransient<IRequestHandler<MarkAllAsReadCommand, bool>, MarkAllAsReadHandler>();
builder.Services.AddTransient<IRequestHandler<GetUnreadCountQuery, int>, GetUnreadCountHandler>();
builder.Services.AddTransient<IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>, GetUserNotificationsHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteNotificationCommand, bool>, DeleteNotificationHandler>();

// Register notification senders with keyed services
builder.Services.AddKeyedSingleton<INotificationSender, EmailNotificationSender>("email");
builder.Services.AddKeyedSingleton<INotificationSender, SmsNotificationSender>("sms");
builder.Services.AddKeyedSingleton<INotificationSender, PushNotificationSender>("push");

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
    c.SwaggerDoc("v1", new() { Title = "Gov2Biz Notification Service", Version = "v1" });
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
