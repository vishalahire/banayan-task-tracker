# Developer Guide

This guide covers development workflows, setup, and best practices for the Banayan Task Tracker project.

## 🚀 Development Environment Setup

### Prerequisites
- **.NET 8 SDK** - Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - Download from [Node.js](https://nodejs.org/)
- **Docker Desktop** - Download from [Docker](https://www.docker.com/products/docker-desktop)
- **PostgreSQL** (optional for local dev) - Download from [PostgreSQL](https://www.postgresql.org/download/)
- **Git** - Download from [Git SCM](https://git-scm.com/)

### IDE Recommendations
- **Visual Studio Code** with extensions:
  - C# Dev Kit
  - ES7+ React/Redux/React-Native snippets
  - Tailwind CSS IntelliSense
  - Docker
  - Thunder Client (API testing)

## 🏗️ Project Structure

```
banayan-task-tracker/
├── src/                          # Application source code
│   ├── TaskTracker.Api/          # ASP.NET Core Web API
│   ├── TaskTracker.Application/  # Application layer (CQRS, Services)
│   ├── TaskTracker.Domain/       # Domain entities and interfaces
│   ├── TaskTracker.Infrastructure/ # Data access and external services
│   ├── TaskTracker.Worker/       # Background service for reminders
│   └── TaskTracker.Web/          # React frontend application
├── tests/                        # Test projects
│   ├── TaskTracker.UnitTests/    # Backend unit tests
│   └── TaskTracker.IntegrationTests/ # API integration tests
├── infra/                        # Infrastructure and deployment
│   ├── docker-compose.yml        # Container orchestration
│   ├── Dockerfile.api           # API container definition
│   ├── Dockerfile.worker        # Worker container definition
│   └── Dockerfile.web           # Frontend container definition
└── docs/                         # Documentation
    └── architecture-and-design/  # System architecture docs
```

## 🔧 Development Workflows

### 1. Full Docker Development (Recommended)
Best for: Complete system testing and production-like environment

```bash
# Start all services
cd infra
docker-compose up --build

# Access points
# Web: http://localhost:3000
# API: http://localhost:5000
# DB: localhost:5432
```

**Pros**: Production parity, all services running, no local dependencies
**Cons**: Slower rebuilds, less debugging flexibility

### 2. Hybrid Development
Best for: Active backend development with containerized database

```bash
# 1. Start only database
cd infra
docker-compose up -d db

# 2. Run API locally
cd ../src/TaskTracker.Api
export TASKTRACKER_CONNECTION_STRING="Host=localhost;Port=5432;Database=tasktracker;Username=taskuser;Password=taskpassword"
dotnet run

# 3. Run frontend locally
cd ../TaskTracker.Web
npm install
npm run dev

# 4. (Optional) Run worker locally
cd ../TaskTracker.Worker
dotnet run
```

**Pros**: Fast rebuilds, easy debugging, hot reload
**Cons**: Environment differences, more setup steps

### 3. Local Development (All Local)
Best for: Offline development or when Docker is not available

```bash
# 1. Setup local PostgreSQL
# Create database 'tasktracker' with user 'taskuser'

# 2. Update connection string in appsettings.json

# 3. Run migrations
cd src/TaskTracker.Api
dotnet ef database update

# 4. Start all projects
dotnet run --project src/TaskTracker.Api
dotnet run --project src/TaskTracker.Worker
cd src/TaskTracker.Web && npm run dev
```

## 🏛️ Architecture Patterns

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│           TaskTracker.Api           │ ← Presentation Layer
├─────────────────────────────────────┤
│       TaskTracker.Application       │ ← Application Layer  
├─────────────────────────────────────┤
│         TaskTracker.Domain          │ ← Domain Layer
├─────────────────────────────────────┤
│      TaskTracker.Infrastructure     │ ← Infrastructure Layer
└─────────────────────────────────────┘
```

**Dependencies flow inward**: API → Application → Domain ← Infrastructure

### Key Design Patterns
- **CQRS**: Commands and Queries separation in Application layer
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Service registration and IoC
- **Options Pattern**: Configuration management
- **JWT Authentication**: Stateless authentication

### Frontend Architecture
- **Component-Based**: Reusable React components
- **Custom Hooks**: Authentication, API calls, state management
- **TypeScript**: Strong typing throughout
- **Tailwind CSS**: Utility-first styling approach

## 🔄 Development Tasks

### Adding a New Feature

1. **Domain First**: Add entities and interfaces in `TaskTracker.Domain`
2. **Application Layer**: Create commands/queries in `TaskTracker.Application`
3. **Infrastructure**: Implement data access in `TaskTracker.Infrastructure`
4. **API**: Add controllers and DTOs in `TaskTracker.Api`
5. **Frontend**: Create components and API calls in `TaskTracker.Web`
6. **Tests**: Add unit and integration tests

### Database Changes

```bash
# Add migration
cd src/TaskTracker.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../TaskTracker.Api

# Update database
dotnet ef database update --startup-project ../TaskTracker.Api

# In Docker environment, restart API container to apply migrations
docker-compose restart api
```

### Frontend Development

```bash
# Development server with hot reload
cd src/TaskTracker.Web
npm run dev

# Type checking
npm run type-check

# Linting
npm run lint

# Build for production
npm run build
```

### API Development

```bash
# Run with hot reload
cd src/TaskTracker.Api
dotnet watch run

# Generate OpenAPI spec
dotnet run --urls="http://localhost:5000"
# Visit http://localhost:5000/swagger

# Debug with VS Code
# Use F5 with "Launch API" configuration
```

## 🧪 Testing Workflow

### Backend Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TaskTracker.UnitTests
dotnet test tests/TaskTracker.IntegrationTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Testing
```bash
cd src/TaskTracker.Web

# Run tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage
```

### Integration Testing
```bash
# Ensure Docker containers are running
cd infra && docker-compose up -d

# Run integration tests against live API
cd .. && dotnet test tests/TaskTracker.IntegrationTests
```

## 🐛 Debugging

### Backend Debugging
- Use **VS Code** with C# extension for breakpoint debugging
- Use **dotnet watch run** for hot reload during development
- Check logs in **Docker containers**: `docker-compose logs -f api`

### Frontend Debugging
- Use **Browser DevTools** for React component debugging
- Use **React Developer Tools** extension
- Check **Network tab** for API call debugging
- Use **console.log** and **debugger** statements

### Database Debugging
```bash
# Connect to PostgreSQL in Docker
docker exec -it banayan-task-tracker-db-1 psql -U taskuser -d tasktracker

# View connection strings
docker exec banayan-task-tracker-api-1 env | grep TASKTRACKER

# Check database migrations
docker exec banayan-task-tracker-api-1 dotnet ef database update --list
```

## 🚀 Deployment Preparation

### Environment Configuration
- **Development**: Use local settings and Docker Compose
- **Staging**: Update environment variables in docker-compose
- **Production**: Use external configuration management

### Security Checklist
- [ ] Update JWT secret keys
- [ ] Use environment variables for sensitive data
- [ ] Enable HTTPS in production
- [ ] Review CORS policies
- [ ] Update database credentials
- [ ] Enable logging and monitoring

### Performance Considerations
- Database indexes for query optimization
- Frontend bundle optimization
- API response caching
- Docker image size optimization

## 📦 Package Management

### Backend Dependencies
```bash
# Add package to specific project
dotnet add src/TaskTracker.Api package Microsoft.AspNetCore.Authentication.JwtBearer

# Update packages
dotnet restore
```

### Frontend Dependencies
```bash
cd src/TaskTracker.Web

# Add dependency
npm install axios
npm install -D @types/axios

# Update dependencies
npm update
```

## 🔍 Code Quality

### Backend Standards
- Follow **C# naming conventions**
- Use **async/await** for I/O operations
- Implement **proper error handling**
- Add **XML documentation** for public APIs
- Follow **Clean Architecture** principles

### Frontend Standards
- Use **TypeScript** for type safety
- Follow **React best practices**
- Use **functional components** with hooks
- Implement **error boundaries**
- Follow **Tailwind CSS** utility classes

### Git Workflow
```bash
# Feature branch workflow
git checkout -b feature/new-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/new-feature
# Create pull request
```

## 🆘 Troubleshooting

Common issues and solutions:

| Issue | Solution |
|-------|----------|
| **Port already in use** | Change ports in docker-compose.yml or stop conflicting services |
| **Database connection failed** | Check PostgreSQL container is running and credentials match |
| **npm install fails** | Clear cache: `npm cache clean --force` |
| **dotnet restore fails** | Check package versions and .NET SDK version |
| **Docker build fails** | Check Dockerfile paths and .dockerignore file |
| **Tests fail** | Ensure test database is running and connection strings are correct |

See [TROUBLESHOOTING.md](../infra/TROUBLESHOOTING.md) for detailed solutions.

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [React Documentation](https://reactjs.org/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Docker Documentation](https://docs.docker.com/)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)