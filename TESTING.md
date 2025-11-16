# Running Tests

This project includes a minimal but comprehensive test suite covering both backend and frontend components.

## Prerequisites

- .NET 8 SDK installed
- Node.js installed
- For integration tests: Docker containers must be running (API at http://localhost:5000 and PostgreSQL database)

## Backend Tests

### Unit Tests
Tests core business logic without external dependencies.

```bash
# Run unit tests only
dotnet test tests/TaskTracker.UnitTests/

# Run with coverage
dotnet test tests/TaskTracker.UnitTests/ --collect:"XPlat Code Coverage"
```

**Coverage:**
- Domain layer: TaskItem entity validation and behavior
- Application layer: Command validation and service logic

### Integration Tests
Tests the full API against running Docker containers.

```bash
# Ensure containers are running first
cd infra
docker-compose up -d

# Run integration tests
cd ..
dotnet test tests/TaskTracker.IntegrationTests/
```

**Coverage:**
- Health check endpoint
- User registration and authentication flow
- Task CRUD operations (Create, Read, Delete)
- Reminders API endpoints

**Note:** Integration tests require:
- PostgreSQL running on localhost:5432
- API running on localhost:5000
- Tests will create and clean up their own test data

### Run All Backend Tests

```bash
# Run both unit and integration tests
dotnet test
```

## Frontend Tests

Tests React components using Vitest and React Testing Library.

```bash
# Navigate to frontend project
cd src/TaskTracker.Web

# Run tests once
npm run test:run

# Run tests in watch mode (for development)
npm test
```

**Coverage:**
- LoginPage: Form rendering and validation
- TaskListPage: Task display and filtering interface
- Component rendering and basic user interactions

## Test Structure

```
tests/
├── TaskTracker.UnitTests/          # Backend unit tests
│   ├── Domain/                     # Entity tests
│   └── Application/                # Service tests
└── TaskTracker.IntegrationTests/   # API integration tests
    ├── ApiTestFixture.cs          # Test setup and authentication
    └── TasksApiTests.cs           # API endpoint tests

src/TaskTracker.Web/src/test/       # Frontend tests
├── setup.ts                       # Test configuration
├── LoginPage.test.tsx             # Login component tests
└── TaskListPage.test.tsx          # Task list component tests
```

## Running Tests in CI/CD

For automated environments, ensure the following order:

1. **Unit Tests** (no dependencies): `dotnet test tests/TaskTracker.UnitTests/`
2. **Start Services**: `docker-compose up -d`
3. **Integration Tests**: `dotnet test tests/TaskTracker.IntegrationTests/`
4. **Frontend Tests**: `cd src/TaskTracker.Web && npm run test:run`

## Test Configuration

### Backend Tests
- **Framework**: xUnit with .NET 8
- **Packages**: xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, coverlet.collector
- **Integration**: Direct HTTP calls to running API (not in-process)

### Frontend Tests  
- **Framework**: Vitest with React Testing Library
- **Environment**: jsdom for browser simulation
- **Configuration**: `vite.config.ts` includes test settings
- **Mocking**: API calls and authentication hooks are mocked

## Troubleshooting

### Integration Test Failures
- Ensure Docker containers are running: `docker-compose ps`
- Check API is accessible: `curl http://localhost:5000/health`
- Verify database connection: Check Docker logs

### Frontend Test Failures
- Clear node modules and reinstall: `rm -rf node_modules && npm install`
- Check Vitest configuration in `vite.config.ts`

### Database Issues
- Integration tests use the same database as development
- Tests create unique users to avoid conflicts
- Failed tests may leave test data - this is acceptable for development

## Coverage Goals

This test suite aims for **meaningful coverage** rather than exhaustive coverage:

✅ **Core business logic** (Domain entities)  
✅ **Critical user flows** (Auth, CRUD operations)  
✅ **API contract validation** (Request/response formats)  
✅ **UI component rendering** (Key user interface elements)  

The test suite provides confidence in core functionality while remaining maintainable and fast to execute.