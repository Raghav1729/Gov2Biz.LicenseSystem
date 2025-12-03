# Gov2Biz License Management System - Quick Start Guide

## Prerequisites

### For Docker Setup
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)

### For Local Development
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (LocalDB, Express, or Developer Edition)
- **Visual Studio 2022** or **VS Code** (optional)

---

## Option 1: Run with Docker (Recommended)

### 1. Start All Services

Open a terminal in the project root and run:

```bash
docker compose up
```

This single command will:
- Build all microservices (License, Payment, Document, Notification)
- Build the API Gateway and Web frontend
- Start SQL Server database
- Configure networking between services
- Run everything in containers

### 2. Access the Application

| Service | URL |
|---------|-----|
| **Web Application** | http://localhost:5000 |
| **Gateway API** | http://localhost:5001/swagger |
| **License Service** | http://localhost:5002/swagger |
| **Document Service** | http://localhost:5003/swagger |
| **Notification Service** | http://localhost:5004/swagger |
| **Payment Service** | http://localhost:5005/swagger |

### 3. Demo Login Credentials

The web application uses simple email-based role detection:

- **Admin**: admin@gov.com (any password)
- **Agency Staff**: staff@agency.com (any password)
- **Applicant**: user@example.com (any password)

### 4. Stop the Application

```bash
docker compose down
```

To remove volumes and clean up completely:

```bash
docker compose down -v
```

---

## Option 2: Run Locally (Development)

### 1. Start SQL Server

Ensure SQL Server is running locally. Update connection strings in `appsettings.Development.json` files if needed.

Default connection string format:
```
Server=localhost;Database=LicenseDB;Trusted_Connection=True;TrustServerCertificate=True
```

### 2. Run Services Individually

Open separate terminal windows for each service:

**License Service:**
```bash
cd src/Gov2Biz.LicenseService
dotnet run
```
Runs on: https://localhost:7001

**Document Service:**
```bash
cd src/Gov2Biz.DocumentService
dotnet run
```
Runs on: https://localhost:7002

**Notification Service:**
```bash
cd src/Gov2Biz.NotificationService
dotnet run
```
Runs on: https://localhost:7003

**Payment Service:**
```bash
cd src/Gov2Biz.PaymentService
dotnet run
```
Runs on: https://localhost:7004

**API Gateway:**
```bash
cd src/Gov2Biz.Gateway
dotnet run
```
Runs on: https://localhost:7000

**Web Frontend:**
```bash
cd src/Gov2Biz.Web
dotnet run
```
Runs on: https://localhost:7005

### 3. Access the Application

Navigate to: https://localhost:7005

### 4. Run All Services at Once (Alternative)

From the project root:

```bash
dotnet build
```

Then use Visual Studio to run multiple startup projects, or use a process manager like `dotnet watch` in each directory.

---

## Option 3: Run with Visual Studio

### 1. Open Solution

Open `Gov2Biz.LicenseSystem.sln` in Visual Studio 2022

### 2. Configure Multiple Startup Projects

1. Right-click the solution → **Properties**
2. Select **Multiple startup projects**
3. Set these projects to **Start**:
   - Gov2Biz.Web
   - Gov2Biz.Gateway
   - Gov2Biz.LicenseService
   - Gov2Biz.DocumentService
   - Gov2Biz.NotificationService
   - Gov2Biz.PaymentService

### 3. Press F5 to Run

All services will start simultaneously.

---

## Troubleshooting

### Docker Issues

**Port conflicts:**
```bash
# Check what's using the ports
netstat -ano | findstr :5000
# Or on Mac/Linux
lsof -i :5000
```

**Rebuild containers:**
```bash
docker compose build --no-cache
docker compose up
```

### Local Development Issues

**Database connection errors:**
- Verify SQL Server is running
- Check connection strings in `appsettings.Development.json`
- Ensure databases are created (EF migrations will auto-create)

**Port already in use:**
- Change ports in `Properties/launchSettings.json` for each service
- Update `ocelot.json` in Gateway to match new ports

---

## Next Steps

- Explore the Swagger documentation for each microservice
- Check the `/docs` folder for architecture details
- Review role-based dashboards in the web application
- Test the CQRS pattern in License Service