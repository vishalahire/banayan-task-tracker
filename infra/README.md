# TaskTracker Docker Setup

This directory contains Docker configuration for running the complete TaskTracker system locally.

## Services

- **db**: PostgreSQL 15 database
- **api**: TaskTracker.Api (.NET 8 Web API)
- **worker**: TaskTracker.Worker (.NET 8 Background Service)
- **web**: TaskTracker.Web (React + Vite + TypeScript + Tailwind)

## Quick Start

1. **Prerequisites**: Ensure Docker and Docker Compose are installed

2. **Build and run all services**:
   ```bash
   # From the repository root
   cd infra
   docker compose up --build
   ```

3. **Access the applications**:
   - Web UI: http://localhost:3000
   - API: http://localhost:5000
   - API Documentation: http://localhost:5000/swagger
   - PostgreSQL: localhost:5432

## Environment Configuration

### Database
- **Host**: db (internal) / localhost:5432 (external)
- **Database**: tasktracker
- **Username**: taskuser
- **Password**: taskpassword

### API Configuration
- Uses `TASKTRACKER_CONNECTION_STRING` environment variable
- Fallback to configuration if environment variable not set
- Runs on port 5000

### Worker Configuration  
- Uses same `TASKTRACKER_CONNECTION_STRING` as API
- Processes reminder notifications in background

### Web Configuration
- Uses `VITE_API_BASE_URL` environment variable
- Points to http://api:5000 within Docker network
- Served by nginx on port 3000

## Development Commands

```bash
# Build without cache
docker compose build --no-cache

# Run in background
docker compose up -d

# View logs
docker compose logs -f [service_name]

# Stop services
docker compose down

# Stop and remove volumes (deletes database data)
docker compose down -v

# Restart specific service
docker compose restart api
```

## Database Persistence

Database data is persisted in a named Docker volume `tasktracker_postgres_data`. To reset the database:

```bash
docker compose down -v
docker compose up --build
```

## Network Configuration

All services communicate through the `tasktracker-network` bridge network:
- Services can reference each other by service name (db, api, worker, web)
- External access is available through mapped ports

## Health Checks

The API includes health checks:
- Basic health: http://localhost:5000/health
- Ready check: http://localhost:5000/health/ready

## Security Notes

- Database credentials are set via environment variables in docker-compose.yml
- In production, use Docker secrets or external secret management
- JWT secret keys should be externalized for production use