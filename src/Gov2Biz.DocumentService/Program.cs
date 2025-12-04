using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gov2Biz.DocumentService.Data;
using Gov2Biz.DocumentService.CQRS.Handlers;
using Gov2Biz.DocumentService.CQRS.Commands;
using Gov2Biz.DocumentService.CQRS.Queries;
using Gov2Biz.DocumentService.Services;
using Gov2Biz.Shared.DTOs;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient<IRequestHandler<UploadDocumentCommand, DocumentDto>, UploadDocumentHandler>();
builder.Services.AddTransient<IRequestHandler<GetDocumentQuery, DocumentDto>, GetDocumentHandler>();
builder.Services.AddTransient<IRequestHandler<GetDocumentsQuery, PagedResult<DocumentDto>>, GetDocumentsHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteDocumentCommand, bool>, DeleteDocumentHandler>();
builder.Services.AddTransient<IRequestHandler<DownloadDocumentQuery, byte[]>, DownloadDocumentHandler>();
builder.Services.AddTransient<IRequestHandler<GetEntityDocumentsQuery, List<DocumentDto>>, GetEntityDocumentsHandler>();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
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
    c.SwaggerDoc("v1", new() { Title = "Gov2Biz Document Service", Version = "v1" });
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
