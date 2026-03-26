# Project Architecture

## System Overview

This repository currently contains the four backend services and shared libraries shown below. The gateway in the first diagram is conceptual only; there is no API gateway project in this solution today.

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Browser/Mobile App]
    end

    subgraph "Optional Gateway / Load Balancer"
        Gateway[API Gateway]
    end

    subgraph "Microservices"
        Identity[IdentityAPI<br/>Port: 8088/8089]
        Catalog[CatalogAPI<br/>Port: 8080/8081]
        Ordering[OrderingAPI<br/>Port: 8084/8085]
        Payment[PaymentAPI<br/>Port: 9092/9093]
    end

    subgraph "Infrastructure"
        Redis[(Redis<br/>Token Blacklist<br/>Cache)]
        MongoDB[(MongoDB<br/>Identity Data)]
        SQLServer[(SQL Server<br/>Catalog, Orders & Payments)]
        RabbitMQ[RabbitMQ<br/>Message Bus]
    end

    Browser -->|HTTP/REST| Gateway
    Gateway --> Identity
    Gateway --> Catalog
    Gateway --> Ordering
    Gateway --> Payment

    Identity -->|JWT Auth| Redis
    Identity -->|User Data| MongoDB
    
    Catalog -->|Product Data| SQLServer
    Catalog -->|Events| RabbitMQ
    Catalog -->|Validate Token| Redis
    
    Ordering -->|Order Data| SQLServer
    Ordering -->|Subscribe Events| RabbitMQ
    Ordering -->|gRPC| Catalog
    Ordering -->|Validate Token| Redis

    Payment -->|Payment Data| SQLServer
    Payment -->|Events| RabbitMQ
    Payment -->|gRPC| Ordering
    Payment -->|Validate Token| Redis

    style Identity fill:#e1f5ff
    style Catalog fill:#fff4e1
    style Ordering fill:#f0e1ff
    style Payment fill:#e1ffe8
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

    subgraph "PaymentAPI"
        PA_Controller[Controllers]
        PA_Handler[CQRS Handlers]
        PA_Domain[Domain Models]
        PA_EF[Entity Framework]
        PA_gRPC_Client[gRPC Client]

        PA_Controller --> PA_Handler
        PA_Handler --> PA_Domain
        PA_Domain --> PA_EF
        PA_Handler --> PA_gRPC_Client
    end

    OA_gRPC_Client -.->|Product Info| CA_gRPC
    PA_gRPC_Client -.->|Order Info| OA_Controller
```

## Service Layering

Each microservice is implemented as a single deployable project with folder-based layering:

- `Controllers`, gRPC services, and GraphQL resolvers form the presentation edge
- `Features` contains CQRS-style commands, queries, handlers, and request models
- `Domain` contains entities, value objects, domain events, EF mappings, and migrations
- `Services` plus `Program.cs` wire infrastructure such as auth, caching, messaging, and database access

This is a pragmatic layered microservice structure rather than a strict multi-assembly Clean Architecture implementation.

## Layered View

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
    participant Google as Google OAuth
    participant Redis
    participant Catalog
    participant Ordering

    Client->>Identity: POST /api/auth/login
    Identity->>Identity: Validate credentials
    Identity->>Redis: Check if token blacklisted
    Identity-->>Client: Return JWT Token

    Client->>Google: Google Sign-In (web/mobile SDK)
    Google-->>Client: Google ID Token
    Client->>Identity: POST /api/auth/google-login { idToken }
    Identity->>Google: Validate ID token (GoogleJsonWebSignature)
    Google-->>Identity: Verified payload (email, sub, name)
    Identity->>Identity: FindByEmail — existing user → link GoogleId if needed<br/>new user → provision account + assign "User" role
    Identity-->>Client: Return JWT Token (same shape as password login)

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
    subgraph "OrderingAPI"
        OA_Domain[Order Domain Events]
        OA_Wolverine[Wolverine]
        OA_Handler[Payment Event Handler]
    end

    subgraph "PaymentAPI"
        PA_Handler[Order Event Handler]
        PA_Domain[Payment Domain Events]
        PA_Wolverine[Wolverine]
    end

    subgraph "RabbitMQ"
        OrderingExchange[ordering.exchange]
        PaymentExchange[payment.exchange]
        PaymentQueue[payment-ordering.queue]
        OrderingQueue[ordering-payment.queue]
    end

    OA_Domain -->|ordering.order.created| OA_Wolverine
    OA_Wolverine --> OrderingExchange
    OrderingExchange --> PaymentQueue
    PaymentQueue --> PA_Handler

    PA_Domain -->|payment.completed| PA_Wolverine
    PA_Wolverine --> PaymentExchange
    PaymentExchange --> OrderingQueue
    OrderingQueue --> OA_Handler
```

## Payment Flow

```mermaid
sequenceDiagram
    participant Client
    participant Ordering
    participant RabbitMQ
    participant Payment
    participant OrderingGrpc as Ordering gRPC

    Client->>Ordering: Create order
    Ordering->>RabbitMQ: ordering.order.created
    RabbitMQ->>Payment: OrderCreatedEvent
    Payment->>OrderingGrpc: GetById(orderId)
    OrderingGrpc-->>Payment: Order snapshot
    Payment->>Payment: Create pending payment

    Client->>Payment: Complete payment
    Payment->>RabbitMQ: payment.completed
    RabbitMQ->>Ordering: PaymentCompletedEvent
    Ordering->>Ordering: Mark order as paid
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
      .NET 10.0
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
      Google OAuth (ID token)
    Databases
      SQL Server
      MongoDB
      Redis
    Patterns
      Mediator
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
- **Google OAuth login** — server-side ID token validation via `Google.Apis.Auth`; auto-provisions local accounts on first login; links Google identity to existing accounts by verified email

  → See [Google Login](./google-login.md) for full documentation, configuration, and security details.

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
- Shared request validation filter

## Current Gaps

- No dedicated API gateway project is included in this repository
- Service boundaries are still enforced mostly by conventions and folder structure, not separate domain/application/infrastructure assemblies
- Test coverage is present but currently focused on key regression paths rather than full end-to-end scenarios
