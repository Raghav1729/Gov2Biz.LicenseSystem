# Gov2Biz License System - Quick Start Guide

Get the entire Gov2Biz License System running in under 2 minutes with a single command.

## Prerequisites

**Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)

That's it! Docker will handle everything else.

---

## Run the Application

### 1. Start All Services

Open a terminal and run:

```bash
docker compose up -d
```

This single command will:
- Build all 6 microservices
- Start SQL Server database
- Configure networking between services
- Run everything in the background

### 2. Access the Application

Once started (takes ~2-3 minutes first time), access:

| Service | URL |
|---------|-----|
| **Web Application** | http://localhost:5000 |
| **Gateway API** | http://localhost:5001/swagger |
| **License Service** | http://localhost:5002/swagger |
| **Document Service** | http://localhost:5003/swagger |
| **Notification Service** | http://localhost:5004/swagger |
| **Payment Service** | http://localhost:5005/swagger |

### 3. Stop the Application

```bash
docker compose down
```

To also remove database data:
```bash
docker compose down -v
```

---

## That's It!

Your entire microservices application is now running. Open http://localhost:5000 in your browser to get started.

---

## Useful Commands

**View logs:**
```bash
docker compose logs -f
```

**View logs for specific service:**
```bash
docker compose logs -f gateway
```

**Restart services:**
```bash
docker compose restart
```

**Rebuild after code changes:**
```bash
docker compose up -d --build
```

**Check service status:**
```bash
docker compose ps
```