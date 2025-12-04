using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Gov2Biz.Web.Services;
using Gov2Biz.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Gov2Biz.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add HTTP client for API Gateway communication
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add service clients
builder.Services.AddHttpClient<ILicenseServiceClient, LicenseServiceClient>();
builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>();
builder.Services.AddHttpClient<IDocumentServiceClient, DocumentServiceClient>();
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>();

// Add DbContext for direct database access
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add cookie authentication for MVC web application
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    // Add global authorization filter
    options.Filters.Add(new AuthorizeFilter());
    // Allow anonymous access to Auth controller
    options.Filters.Add(new AnonymousAuthorizationFilter());
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AgencyStaff", policy => policy.RequireRole("Administrator", "AgencyStaff"));
    options.AddPolicy("Applicant", policy => policy.RequireRole("Administrator", "AgencyStaff", "Applicant"));
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add global 401 error handler
app.Use(async (context, next) =>
{
    await next();
    
    if (context.Response.StatusCode == 401)
    {
        var returnUrl = context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"/Auth/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

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
            // Redirect to login page with return URL
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Auth", new { ReturnUrl = returnUrl });
        }
    }
}
