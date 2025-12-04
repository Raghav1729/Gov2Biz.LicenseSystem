using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Gov2Biz.Web.Services;
using Gov2Biz.Shared.Configuration;
using Gov2Biz.LicenseService.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<LicenseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    // Allow anonymous access to Auth controller
    options.Filters.Add(new AnonymousAuthorizationFilter());
});

// Register authentication service
builder.Services.AddScoped<IAuthService>(provider => {
    var context = provider.GetRequiredService<LicenseDbContext>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<AuthService>>();
    return new AuthService(context, configuration, logger);
});

// Add JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AgencyStaff", policy => policy.RequireRole("Administrator", "AgencyStaff"));
    options.AddPolicy("Applicant", policy => policy.RequireRole("Administrator", "AgencyStaff", "Applicant"));
});

// Register HTTP clients for microservices
builder.Services.AddHttpClient<ILicenseServiceClient, LicenseServiceClient>();
builder.Services.AddHttpClient<IDocumentServiceClient, DocumentServiceClient>();
builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>();
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>();

// Add scoped services
builder.Services.AddScoped<ILicenseServiceClient, LicenseServiceClient>();
builder.Services.AddScoped<IDocumentServiceClient, DocumentServiceClient>();
builder.Services.AddScoped<INotificationServiceClient, NotificationServiceClient>();
builder.Services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

// Custom filter to allow anonymous access to specific controllers
public class AnonymousAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Allow anonymous access to Auth controller
        if (context.ActionDescriptor.RouteValues["controller"] == "Auth")
        {
            return;
        }
        
        // For all other controllers, require authentication
        if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.Result = new ChallengeResult();
        }
    }
}
