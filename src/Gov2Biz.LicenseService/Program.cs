using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gov2Biz.LicenseService.Data;
using Gov2Biz.LicenseService.CQRS.Handlers;
using Gov2Biz.LicenseService.CQRS.Commands;
using Gov2Biz.LicenseService.CQRS.Queries;
using MediatR;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<LicenseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddTransient<IRequestHandler<CreateLicenseApplicationCommand, LicenseApplicationDto>, CreateLicenseApplicationHandler>();
builder.Services.AddTransient<IRequestHandler<GetLicenseApplicationQuery, LicenseApplicationDto>, GetLicenseApplicationHandler>();
builder.Services.AddTransient<IRequestHandler<GetLicenseApplicationsQuery, Gov2Biz.Shared.Responses.PagedResult<LicenseApplicationDto>>, GetLicenseApplicationsHandler>();
builder.Services.AddTransient<IRequestHandler<ApproveLicenseApplicationCommand, LicenseDto>, ApproveLicenseApplicationHandler>();
builder.Services.AddTransient<IRequestHandler<RejectLicenseApplicationCommand, LicenseApplicationDto>, RejectLicenseApplicationHandler>();
builder.Services.AddTransient<IRequestHandler<IssueLicenseCommand, LicenseDto>, IssueLicenseCommandHandler>();
builder.Services.AddTransient<IRequestHandler<RenewLicenseCommand, LicenseDto>, RenewLicenseCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetLicenseQuery, LicenseDto>, GetLicenseHandler>();
builder.Services.AddTransient<IRequestHandler<GetLicensesQuery, Gov2Biz.Shared.Responses.PagedResult<LicenseDto>>, GetLicensesHandler>();
builder.Services.AddTransient<IRequestHandler<GetUserLicensesQuery, List<LicenseDto>>, GetUserLicensesHandler>();
builder.Services.AddTransient<IRequestHandler<GetUserApplicationsQuery, List<LicenseApplicationDto>>, GetUserApplicationsHandler>();
builder.Services.AddTransient<IRequestHandler<GetDashboardStatsQuery, Gov2Biz.Shared.Responses.DashboardStatsDto>, GetDashboardStatsHandler>();

// Add Hangfire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

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
    c.SwaggerDoc("v1", new() { Title = "Gov2Biz License Service", Version = "v1" });
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

// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Run();

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
