# Project Architecture

## System Overview

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Browser/Mobile App]
    end

    subgraph "API Gateway / Load Balancer"
        Gateway[API Gateway]
    end

    subgraph "Microservices"
        Identity[IdentityAPI<br/>Port: 8088/8089]
        Catalog[CatalogAPI<br/>Port: 8080/8081]
        Ordering[OrderingAPI<br/>Port: 8084/8085]
    end

    subgraph "Infrastructure"
        Redis[(Redis<br/>Token Blacklist<br/>Cache)]
        MongoDB[(MongoDB<br/>Identity Data)]
        SQLServer[(SQL Server<br/>Catalog & Orders)]
        RabbitMQ[RabbitMQ<br/>Message Bus]
    end

    Browser -->|HTTP/REST| Gateway
    Gateway --> Identity
    Gateway --> Catalog
    Gateway --> Ordering

    Identity -->|JWT Auth| Redis
    Identity -->|User Data| MongoDB
    
    Catalog -->|Product Data| SQLServer
    Catalog -->|Events| RabbitMQ
    Catalog -->|Validate Token| Redis
    
    Ordering -->|Order Data| SQLServer
    Ordering -->|Subscribe Events| RabbitMQ
    Ordering -->|gRPC| Catalog
    Ordering -->|Validate Token| Redis

    style Identity fill:#e1f5ff
    style Catalog fill:#fff4e1
    style Ordering fill:#f0e1ff
```

## Microservices Architecture

```mermaid
graph LR
    subgraph "IdentityAPI"
        IA_Controller[Controllers]
        IA_Handler[CQRS Handlers]
        IA_Service[Services<br/>JWT, Blacklist]
        IA_Identity[ASP.NET Identity]
        
        IA_Controller --> IA_Handler
        IA_Handler --> IA_Service
        IA_Handler --> IA_Identity
    end

    subgraph "CatalogAPI"
        CA_Controller[Controllers]
        CA_Handler[CQRS Handlers]
        CA_Domain[Domain Models]
        CA_EF[Entity Framework]
        CA_gRPC[gRPC Services]
        
        CA_Controller --> CA_Handler
        CA_Handler --> CA_Domain
        CA_Domain --> CA_EF
        CA_gRPC --> CA_Domain
    end

    subgraph "OrderingAPI"
        OA_Controller[Controllers]
        OA_Handler[CQRS Handlers]
        OA_Domain[Domain Models]
        OA_EF[Entity Framework]
        OA_gRPC_Client[gRPC Client]
        
        OA_Controller --> OA_Handler
        OA_Handler --> OA_Domain
        OA_Domain --> OA_EF
        OA_Handler --> OA_gRPC_Client
    end

    OA_gRPC_Client -.->|Product Info| CA_gRPC
```

## Clean Architecture Layers

```mermaid
graph TB
    subgraph "Presentation Layer"
        Controllers[Controllers<br/>REST API Endpoints]
        gRPC[gRPC Services]
        GraphQL[GraphQL<br/>CatalogAPI only]
    end

    subgraph "Application Layer"
        CQRS[CQRS<br/>Commands & Queries]
        Handlers[Mediator Handlers]
        Validators[FluentValidation]
    end

    subgraph "Domain Layer"
        Entities[Domain Entities]
        ValueObjects[Value Objects]
        DomainEvents[Domain Events]
        Aggregates[Aggregates]
    end

    subgraph "Infrastructure Layer"
        EF[Entity Framework]
        MongoDB[MongoDB Driver]
        Identity[ASP.NET Identity]
        Redis[Redis Cache]
        RabbitMQ[Wolverine + RabbitMQ]
    end

    Controllers --> CQRS
    gRPC --> CQRS
    GraphQL --> CQRS
    CQRS --> Handlers
    Handlers --> Validators
    Handlers --> Entities
    Entities --> ValueObjects
    Entities --> DomainEvents
    DomainEvents --> RabbitMQ
    Handlers --> EF
    Handlers --> MongoDB
    Handlers --> Identity
    Handlers --> Redis
```

## Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant Identity
    participant Redis
    participant Catalog
    participant Ordering

    Client->>Identity: POST /api/auth/login
    Identity->>Identity: Validate credentials
    Identity->>Redis: Check if token blacklisted
    Identity-->>Client: Return JWT Token

    Client->>Catalog: GET /api/products (with JWT)
    Catalog->>Catalog: Validate JWT signature
    Catalog->>Redis: Check token blacklist
    Redis-->>Catalog: Token valid
    Catalog-->>Client: Return products

    Client->>Ordering: POST /api/orders (with JWT)
    Ordering->>Ordering: Validate JWT signature
    Ordering->>Redis: Check token blacklist
    Redis-->>Ordering: Token valid
    Ordering->>Catalog: gRPC GetProduct
    Catalog-->>Ordering: Product details
    Ordering-->>Client: Order created

    Client->>Identity: POST /api/auth/logout (with JWT)
    Identity->>Redis: Add token to blacklist
    Identity-->>Client: Logged out

    Client->>Catalog: GET /api/products (with revoked JWT)
    Catalog->>Redis: Check token blacklist
    Redis-->>Catalog: Token revoked
    Catalog-->>Client: 401 Unauthorized
```

## Event-Driven Communication

```mermaid
graph LR
    subgraph "CatalogAPI"
        CA_Domain[Domain Events]
        CA_Interceptor[Domain Event Interceptor]
        CA_Wolverine[Wolverine]
    end

    subgraph "RabbitMQ"
        Exchange[catalog.exchange]
        Queue[ordering-catalog.queue]
    end

    subgraph "OrderingAPI"
        OA_Wolverine[Wolverine]
        OA_Handler[Event Handlers]
    end

    CA_Domain -->|Save Changes| CA_Interceptor
    CA_Interceptor -->|Publish| CA_Wolverine
    CA_Wolverine -->|catalog.category.created| Exchange
    Exchange --> Queue
    Queue --> OA_Wolverine
    OA_Wolverine --> OA_Handler
```

## Data Flow

```mermaid
graph TB
    subgraph "Request Flow"
        Request[HTTP Request]
        Middleware[Middleware Pipeline]
        Controller[Controller]
        Mediator[Mediator]
        Handler[Handler]
        Domain[Domain Logic]
        Repository[Repository/DbContext]
        Database[(Database)]
    end

    Request --> Middleware
    Middleware -->|1. Exception Handler| Middleware
    Middleware -->|2. Authentication| Middleware
    Middleware -->|3. Token Blacklist| Middleware
    Middleware -->|4. Authorization| Controller
    Controller -->|Send Command/Query| Mediator
    Mediator -->|Route to| Handler
    Handler -->|Validation| Handler
    Handler -->|Business Logic| Domain
    Domain -->|Persist| Repository
    Repository --> Database
    Database -->|Return| Repository
    Repository -->|Return| Domain
    Domain -->|Return| Handler
    Handler -->|Return| Mediator
    Mediator -->|Return| Controller
    Controller -->|ApiResponse| Request
```

## Shared Components

```mermaid
graph TB
    subgraph "Shared Library"
        Auth[Authentication<br/>JWT Config<br/>CurrentUser]
        Middleware[Middleware<br/>Exception Handler<br/>Token Blacklist]
        Common[Common<br/>ApiResponse<br/>Exceptions]
        Services[Services<br/>TokenBlacklistService]
        Interceptors[Interceptors<br/>Domain Events]
    end

    subgraph "All Microservices"
        Identity[IdentityAPI]
        Catalog[CatalogAPI]
        Ordering[OrderingAPI]
    end

    Auth --> Identity
    Auth --> Catalog
    Auth --> Ordering
    
    Middleware --> Identity
    Middleware --> Catalog
    Middleware --> Ordering
    
    Common --> Identity
    Common --> Catalog
    Common --> Ordering
    
    Services --> Identity
    Services --> Catalog
    Services --> Ordering
    
    Interceptors --> Catalog
    Interceptors --> Ordering
```

## Technology Stack

```mermaid
mindmap
  root((Project))
    Backend
      .NET 9.0
      ASP.NET Core
      C#
    Architecture
      Microservices
      CQRS
      DDD
      Event-Driven
    Communication
      REST API
      gRPC
      GraphQL
      RabbitMQ
    Authentication
      JWT
      ASP.NET Identity
      Redis Blacklist
    Databases
      SQL Server
      MongoDB
      Redis
    Patterns
      Mediator
      Repository
      Unit of Work
      Domain Events
    Libraries
      Mediator
      FluentValidation
      Wolverine
      Mapster
      Entity Framework
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Docker Compose"
        subgraph "Services"
            Identity_Container[identity-api:8088/8089]
            Catalog_Container[catalog-api:8080/8081]
            Ordering_Container[ordering-api:8084/8085]
        end
        
        subgraph "Infrastructure"
            SQL_Container[sqlserver:1433]
            Mongo_Container[mongodb:27017]
            Redis_Container[redis:6379]
            Rabbit_Container[rabbitmq:5672/15672]
        end
        
        subgraph "Network"
            Network[microservices-net]
        end
    end

    Identity_Container -.-> Network
    Catalog_Container -.-> Network
    Ordering_Container -.-> Network
    SQL_Container -.-> Network
    Mongo_Container -.-> Network
    Redis_Container -.-> Network
    Rabbit_Container -.-> Network

    Catalog_Container --> SQL_Container
    Ordering_Container --> SQL_Container
    Identity_Container --> Mongo_Container
    Identity_Container --> Redis_Container
    Catalog_Container --> Redis_Container
    Ordering_Container --> Redis_Container
    Catalog_Container --> Rabbit_Container
    Ordering_Container --> Rabbit_Container
```

## Key Features

### IdentityAPI
- User registration & authentication
- JWT token generation
- Token blacklist management
- Role-based authorization
- MongoDB for user storage

### CatalogAPI
- Product & category management
- GraphQL API
- gRPC services for inter-service communication
- Domain events publishing
- SQL Server storage

### OrderingAPI
- Order management
- Event-driven architecture
- gRPC client for product info
- Hybrid caching (Redis + Memory)
- SQL Server storage

### Shared Components
- JWT authentication middleware
- Global exception handler
- Token blacklist service
- API response wrapper
- Domain event interceptor
