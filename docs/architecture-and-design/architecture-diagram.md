# System Architecture Diagram

This document describes the high-level architecture of the Banayan Task Tracker system.

## 🏗️ System Overview

The Task Tracker follows a **Clean Architecture** pattern with clear separation of concerns across multiple layers and services.

## 🎯 Architecture Diagram

```mermaid
graph TB
    subgraph "Frontend Layer"
        Web[React Web App<br/>Port 3000]
        Web --> API_Gateway[API Gateway Layer]
    end
    
    subgraph "API Layer"
        API_Gateway --> Auth[Authentication<br/>JWT Middleware]
        Auth --> Controllers[API Controllers]
        Controllers --> App_Layer[Application Layer]
    end
    
    subgraph "Application Core"
        App_Layer --> Commands[Commands & Queries<br/>CQRS Pattern]
        Commands --> Services[Domain Services]
        Services --> Domain[Domain Entities]
        
        App_Layer --> Interfaces[Repository Interfaces]
    end
    
    subgraph "Infrastructure Layer"
        Interfaces --> Repositories[Entity Framework<br/>Repositories]
        Repositories --> DB[(PostgreSQL<br/>Database)]
        
        Worker[Background Worker<br/>Service] --> Repositories
        Worker --> Scheduler[Task Scheduler<br/>Reminders]
    end
    
    subgraph "Cross-Cutting Concerns"
        Logging[Structured Logging]
        Config[Configuration<br/>Environment Variables]
        Health[Health Checks]
    end
    
    API_Gateway -.-> Logging
    Controllers -.-> Logging
    Services -.-> Logging
    Worker -.-> Logging
    
    Controllers -.-> Config
    Worker -.-> Config
    Repositories -.-> Config
    
    API_Gateway --> Health
    
    style Web fill:#e1f5fe
    style Domain fill:#f3e5f5
    style DB fill:#e8f5e8
    style Worker fill:#fff3e0
```

## 🏛️ Architectural Layers

### 1. Presentation Layer
- **React Frontend**: Single Page Application with TypeScript
- **API Controllers**: ASP.NET Core Web API endpoints
- **Responsibilities**: User interface, HTTP request/response handling

### 2. Application Layer
- **Commands & Queries**: CQRS pattern for operation separation
- **Application Services**: Business workflow orchestration
- **DTOs**: Data transfer objects for API contracts
- **Responsibilities**: Use case implementation, business workflow coordination

### 3. Domain Layer
- **Entities**: Core business objects (TaskItem, User, Reminder)
- **Value Objects**: Immutable objects with validation
- **Domain Services**: Business logic that doesn't belong to entities
- **Interfaces**: Abstractions for external dependencies
- **Responsibilities**: Business rules, domain logic, entity behavior

### 4. Infrastructure Layer
- **Data Access**: Entity Framework Core repositories
- **External Services**: Email, logging, configuration
- **Background Services**: Worker service for reminders
- **Responsibilities**: Data persistence, external integrations

## 🔄 Data Flow

### 1. API Request Flow
```
User Input → React Component → API Call → JWT Middleware → Controller → 
Application Service → Domain Logic → Repository → Database
```

### 2. Background Processing Flow
```
Timer Trigger → Worker Service → Application Service → Domain Logic → 
Repository → Database → Notification Processing
```

## 🌐 Network Architecture

```mermaid
graph LR
    subgraph "Docker Network"
        Web_Container[Web Container<br/>nginx:3000]
        API_Container[API Container<br/>aspnet:5000]
        Worker_Container[Worker Container<br/>aspnet]
        DB_Container[PostgreSQL Container<br/>postgres:5432]
    end
    
    subgraph "External Access"
        Browser[Web Browser]
        API_Client[API Client/Swagger]
    end
    
    Browser -->|HTTP| Web_Container
    API_Client -->|HTTP| API_Container
    Web_Container -->|HTTP API Calls| API_Container
    API_Container -->|TCP/IP| DB_Container
    Worker_Container -->|TCP/IP| DB_Container
    
    style Web_Container fill:#e1f5fe
    style API_Container fill:#f3e5f5
    style Worker_Container fill:#fff3e0
    style DB_Container fill:#e8f5e8
```

## 🔒 Security Architecture

### Authentication Flow
```mermaid
sequenceDiagram
    participant U as User
    participant W as Web App
    participant A as API
    participant DB as Database
    
    U->>W: Login Request
    W->>A: POST /auth/login
    A->>DB: Validate Credentials
    DB-->>A: User Data
    A-->>W: JWT Token + User Info
    W->>W: Store Token (Memory)
    
    Note over W,A: Subsequent Requests
    W->>A: API Request + Bearer Token
    A->>A: Validate JWT
    A-->>W: Protected Resource
```

### Security Measures
- **JWT Tokens**: Stateless authentication with expiration
- **Password Hashing**: bcrypt for secure password storage
- **HTTPS**: SSL/TLS encryption in production
- **CORS**: Cross-origin request protection
- **Input Validation**: Data validation at API boundaries

## 📊 Data Architecture

### Entity Relationship Diagram
```mermaid
erDiagram
    Users {
        int Id PK
        string Email UK
        string Username
        string PasswordHash
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    TaskItems {
        int Id PK
        string Title
        string Description
        int Status
        int Priority
        datetime DueDate
        datetime CreatedAt
        datetime UpdatedAt
        int UserId FK
    }
    
    Reminders {
        int Id PK
        int TaskId FK
        datetime ScheduledFor
        bool IsProcessed
        datetime CreatedAt
        datetime ProcessedAt
    }
    
    Users ||--o{ TaskItems : "owns"
    TaskItems ||--o{ Reminders : "has"
```

## 🚀 Deployment Architecture

### Container Orchestration
```mermaid
graph TB
    subgraph "Docker Compose Stack"
        LoadBalancer[nginx<br/>Load Balancer]
        
        subgraph "Application Services"
            WebApp[React Web App<br/>nginx]
            API[ASP.NET Core API<br/>Kestrel]
            Worker[Background Worker<br/>.NET Service]
        end
        
        subgraph "Data Layer"
            PostgreSQL[(PostgreSQL<br/>Database)]
            Volume[Docker Volume<br/>Data Persistence]
        end
        
        subgraph "Monitoring"
            HealthChecks[Health Checks]
            Logging[Centralized Logging]
        end
    end
    
    LoadBalancer --> WebApp
    LoadBalancer --> API
    WebApp --> API
    API --> PostgreSQL
    Worker --> PostgreSQL
    PostgreSQL --> Volume
    
    API -.-> HealthChecks
    Worker -.-> HealthChecks
    API -.-> Logging
    Worker -.-> Logging
```

## 📈 Scalability Considerations

### Horizontal Scaling Opportunities
- **Web Tier**: Multiple React app instances behind load balancer
- **API Tier**: Multiple API instances with shared database
- **Background Processing**: Multiple worker instances for different tasks

### Vertical Scaling Points
- **Database**: Increased CPU/Memory for PostgreSQL
- **API Server**: More cores for request processing
- **Cache Layer**: Redis for session/data caching (future enhancement)

## 🔧 Technology Decisions

| Component | Technology | Rationale |
|-----------|------------|-----------|
| **Frontend** | React + TypeScript | Type safety, component reusability, modern ecosystem |
| **Backend API** | ASP.NET Core 8 | High performance, cross-platform, rich ecosystem |
| **Database** | PostgreSQL | ACID compliance, JSON support, mature and reliable |
| **Authentication** | JWT | Stateless, scalable, standard |
| **Background Processing** | .NET Worker Service | Integrated with .NET ecosystem, reliable |
| **Containerization** | Docker + Compose | Environment consistency, easy deployment |
| **Architecture Pattern** | Clean Architecture | Separation of concerns, testability, maintainability |

## 🔍 Quality Attributes

### Performance
- **API Response Time**: < 200ms for standard operations
- **Database Queries**: Indexed for common lookup patterns
- **Frontend Bundle**: Code splitting for optimal load times

### Reliability
- **Health Checks**: API and database connectivity monitoring
- **Error Handling**: Graceful degradation and user feedback
- **Logging**: Structured logging for debugging and monitoring

### Maintainability
- **Clean Architecture**: Clear separation of concerns
- **Dependency Injection**: Loose coupling between components
- **Testing Strategy**: Unit and integration tests for confidence

### Security
- **Authentication**: JWT-based with proper validation
- **Authorization**: User-specific data access controls
- **Input Validation**: Comprehensive data validation at boundaries