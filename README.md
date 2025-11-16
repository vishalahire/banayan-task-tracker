# Banayan Task Tracker

A full-stack task management application built with .NET 8, React, and PostgreSQL. Created as a take-home assignment for Banyan Software.

## 🚀 Quick Start

```bash
# Clone and start with Docker
git clone <repository-url>
cd banayan-task-tracker/infra
docker-compose up --build
```

**Access Points:**
- 🌐 **Web App**: http://localhost:3000
- 🔧 **API**: http://localhost:5000
- 📚 **API Docs**: http://localhost:5000/swagger
- 🗄️ **Database**: localhost:5432

## 📋 Features

### Core Functionality
- ✅ **Task Management**: Create, read, update, delete tasks
- 👤 **User Authentication**: JWT-based registration and login
- 📱 **Responsive UI**: Modern React interface with Tailwind CSS
- 🔄 **Real-time Updates**: Background worker for notifications
- 🔔 **On-Demand Reminders**: HTTP API for triggering reminders

### Technical Features
- 🐳 **Containerized**: Docker Compose for full-stack deployment
- 🧪 **Tested**: Unit and integration tests for backend and frontend
- 📊 **Documented**: Comprehensive API documentation
- 🔒 **Secure**: JWT authentication with proper validation

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Web     │────│  .NET 8 API     │────│   PostgreSQL    │
│   (Port 3000)   │    │   (Port 5000)   │    │   (Port 5432)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                        ┌─────────────────┐
                        │ Background      │
                        │ Worker Service  │
                        └─────────────────┘
```

**Technology Stack:**
- **Backend**: ASP.NET Core 8, Entity Framework Core, PostgreSQL
- **Frontend**: React 18, TypeScript, Vite, Tailwind CSS
- **Infrastructure**: Docker, Docker Compose
- **Testing**: xUnit, Vitest, React Testing Library
- **Authentication**: JWT with ASP.NET Core Identity

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [📋 TESTING.md](./TESTING.md) | Complete testing guide and commands |
| [👨‍💻 DEVELOPER.md](./DEVELOPER.md) | Development setup and workflows |
| [🐳 Docker Setup](./infra/README.md) | Container deployment guide |
| [🔧 Troubleshooting](./infra/TROUBLESHOOTING.md) | Common issues and solutions |

### Architecture & Design Documentation
| Document | Description |
|----------|-------------|
| [🏗️ System Architecture](./docs/architecture-and-design/architecture-diagram.md) | Complete system architecture with diagrams |
| [🎯 Design Decisions](./docs/architecture-and-design/design-decisions.md) | Major technical decisions and trade-offs |
| [📋 Development Approach](./docs/architecture-and-design/approach.md) | Step-by-step development process and AI prompts |

## 🛠️ Development

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- PostgreSQL (for local development)

### Local Development Setup
```bash
# 1. Start infrastructure
cd infra && docker-compose up -d db

# 2. Run API locally
cd ../src/TaskTracker.Api
dotnet run

# 3. Run frontend locally
cd ../TaskTracker.Web
npm install && npm run dev
```

See [DEVELOPER.md](./DEVELOPER.md) for detailed development workflows.

## 🧪 Testing

```bash
# Backend tests
dotnet test                              # All backend tests
dotnet test tests/TaskTracker.UnitTests  # Unit tests only

# Frontend tests  
cd src/TaskTracker.Web && npm test      # React component tests

# Integration tests (requires Docker containers)
docker-compose up -d
dotnet test tests/TaskTracker.IntegrationTests
```

See [TESTING.md](./TESTING.md) for comprehensive testing documentation.

## 🤖 AI-Assisted Development

**Where AI Helped:**
- 🏗️ **Project Scaffolding**: Initial project structure and Clean Architecture setup
- 🔧 **Infrastructure**: Docker configuration and containerization approach
- 🧪 **Testing Strategy**: Test project setup and minimal coverage approach
- 🔄 **Reminder System**: On-demand reminder feature implementation
- 📝 **Documentation**: Comprehensive docs and troubleshooting guides

**Manual Decisions:**
- 🎯 **Business Logic**: Task entity design and validation rules
- 🔒 **Security Model**: JWT implementation and user authentication flow
- 🎨 **UI/UX Design**: React component structure and user interface
- 📊 **Database Schema**: Entity relationships and data model

**Time Constraint Trade-offs:**
- ⚡ **Simplified UI**: Basic styling over polished design
- 🧪 **Minimal Testing**: Focused coverage over exhaustive tests
- 🔄 **Basic Reminders**: Simple implementation over complex notification system
- 📝 **Essential Features**: Core CRUD over advanced task management features

## 🚢 Deployment

**Production Ready Features:**
- Docker containerization
- Environment-based configuration
- Health check endpoints
- Structured logging
- Database migrations

**For Production Deployment:**
```bash
# Update environment variables in docker-compose.yml
# Set secure JWT secrets and database credentials
docker-compose -f docker-compose.prod.yml up -d
```

## 📄 License

Created for Banyan Software take-home assignment.
