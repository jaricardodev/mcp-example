# mcp-example

A .NET 10 Web API demonstrating clean architecture with both a traditional REST interface and a [Model Context Protocol (MCP)](https://spec.modelcontextprotocol.io/) interface, so LLMs can interact with the same domain logic.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [Getting Started](#getting-started)
4. [Web API](#web-api)
   - [Endpoints](#endpoints)
   - [JWT Authentication](#jwt-authentication)
5. [MCP Interface](#mcp-interface)
   - [Available Tools](#available-tools)
   - [API Key Authentication](#api-key-authentication)
6. [Security Summary](#security-summary)
7. [Configuration Reference](#configuration-reference)

---

## Architecture Overview

The solution follows **Clean Architecture** with a strict separation of concerns across four layers:

```
┌────────────────────────────────────────────────────┐
│  API Layer  (McpExample.API)                       │
│  • REST Controllers  (JWT-secured)                 │
│  • MCP Tools         (API Key-secured)             │
│  • Middleware        (McpApiKeyMiddleware)         │
│  • Security helpers  (JwtTokenService)            │
└──────────────┬───────────────────────────────────┘
               │ depends on
┌──────────────▼───────────────────────────────────┐
│  Application Layer  (McpExample.Application)     │
│  • IProductService interface + ProductService    │
│  • DTOs (ProductDto, CreateProductDto, …)        │
│  NO dependency on infrastructure or frameworks   │
└──────────────┬───────────────────────────────────┘
               │ depends on
┌──────────────▼───────────────────────────────────┐
│  Domain Layer  (McpExample.Domain)               │
│  • Product entity (rich domain model)            │
│  • IProductRepository interface                  │
│  Pure C# — no external dependencies              │
└──────────────────────────────────────────────────┘
               ▲
┌──────────────┴───────────────────────────────────┐
│  Infrastructure Layer  (McpExample.Infrastructure)│
│  • InMemoryProductRepository                     │
│  Implements domain interfaces                    │
└──────────────────────────────────────────────────┘
```

Key design points:

- **Domain** knows nothing about ASP.NET, EF, or MCP.
- **Application** only depends on Domain abstractions.
- **Infrastructure** provides concrete implementations (swap to EF Core / SQL without touching domain or application code).
- **API** wires everything together; both the REST controllers and the MCP tools call the *same* `IProductService`.

---

## Project Structure

```
mcp-example/
├── McpExample.slnx
├── src/
│   ├── McpExample.Domain/
│   │   ├── Entities/Product.cs
│   │   └── Interfaces/IProductRepository.cs
│   ├── McpExample.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/IProductService.cs
│   │   ├── Services/ProductService.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   ├── McpExample.Infrastructure/
│   │   ├── Repositories/InMemoryProductRepository.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   └── McpExample.API/
│       ├── Controllers/
│       │   ├── AuthController.cs      ← issues JWT tokens
│       │   └── ProductsController.cs  ← CRUD, [Authorize]
│       ├── McpTools/
│       │   └── ProductMcpTools.cs     ← MCP tools for LLMs
│       ├── Middleware/
│       │   └── McpApiKeyMiddleware.cs ← guards /mcp/*
│       ├── Security/JwtTokenService.cs
│       ├── Program.cs
│       └── appsettings*.json
└── tests/
    └── McpExample.Application.Tests/
        ├── ProductServiceTests.cs
        └── ProductEntityTests.cs
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

```bash
cd src/McpExample.API
dotnet run
```

The API listens on `https://localhost:7254` (HTTPS) and `http://localhost:5109` (HTTP) by default when using the `https` launch profile. The actual port is printed in the console on startup.

### Test

```bash
dotnet test
```

---

## Web API

### Endpoints

All product endpoints require a valid **JWT Bearer token** (see [JWT Authentication](#jwt-authentication) below).

#### Auth

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| `POST` | `/api/auth/token` | Issue a JWT token | ❌ Public |

**Request body:**

```json
{
  "username": "alice",
  "password": "any-password"
}
```

> ⚠️ The demo accepts any non-empty username/password. In production, validate against a real user store.

**Response:**

```json
{
  "token": "<jwt-token>"
}
```

#### Products

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/products` | List all products |
| `GET` | `/api/products/{id}` | Get product by ID |
| `GET` | `/api/products/category/{category}` | Filter by category |
| `POST` | `/api/products` | Create a product |
| `PUT` | `/api/products/{id}` | Update a product |
| `DELETE` | `/api/products/{id}` | Delete a product |

**Create / Update body:**

```json
{
  "name": "Mechanical Keyboard",
  "description": "Tactile switches, RGB backlit",
  "price": 129.99,
  "stockQuantity": 40,
  "category": "Electronics"
}
```

### JWT Authentication

1. **Obtain a token:**

```bash
curl -X POST https://localhost:7254/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"secret"}'
```

2. **Use the token in subsequent requests:**

```bash
curl https://localhost:7254/api/products \
  -H "Authorization: Bearer <jwt-token>"
```

Requests without a valid token receive `401 Unauthorized`.

---

## MCP Interface

The MCP endpoint is exposed at `/mcp` using the `ModelContextProtocol.AspNetCore` library. LLM clients that support MCP (e.g. Claude Desktop, VS Code Copilot, or any custom MCP client) can connect to this URL to discover and call the available tools.

### Available Tools

| Tool Name | Description |
|-----------|-------------|
| `list_products` | Returns all available products |
| `get_product` | Returns a single product by GUID |
| `get_products_by_category` | Returns products in a category |
| `create_product` | Creates a new product |
| `update_product` | Updates an existing product |
| `delete_product` | Deletes a product by GUID |

All tools delegate to the same `IProductService` used by the REST controllers — the domain logic is shared and never duplicated.

### API Key Authentication

MCP requests to `/mcp` must include the `X-Api-Key` header.

> **Note:** The MCP endpoint uses the HTTP+SSE transport (Server-Sent Events). Requests must first `initialize` the session, then use the returned `Mcp-Session-Id` for subsequent calls.

```bash
# Step 1 – Initialize a session (captures the Mcp-Session-Id header)
SESSION_ID=$(curl -si -X POST https://localhost:7254/mcp \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-mcp-api-key-do-not-use-in-production" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' \
  | grep -i "mcp-session-id" | awk '{print $2}' | tr -d '\r')

# Step 2 – List available tools
curl -X POST https://localhost:7254/mcp \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-mcp-api-key-do-not-use-in-production" \
  -H "Mcp-Session-Id: $SESSION_ID" \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
```

Without the `X-Api-Key` header → `401 Unauthorized`  
With a wrong key → `403 Forbidden`

#### Configuring an MCP client (Claude Desktop example)

```json
{
  "mcpServers": {
    "product-api": {
      "url": "https://localhost:7254/mcp",
      "headers": {
        "X-Api-Key": "<your-mcp-api-key>"
      }
    }
  }
}
```

---

## Security Summary

| Interface | Mechanism | Header / Token |
|-----------|-----------|----------------|
| Web API (`/api/**`) | JWT Bearer (HS256) | `Authorization: Bearer <token>` |
| MCP (`/mcp`) | API Key | `X-Api-Key: <key>` |

### How JWT works (Web API)

1. Client calls `POST /api/auth/token` with credentials.
2. `JwtTokenService` issues a signed HS256 token (configurable expiry, issuer, audience).
3. Client attaches the token to every request via the `Authorization` header.
4. ASP.NET Core's `JwtBearer` middleware validates the signature, issuer, audience, and expiry on each request before the controller executes.

### How API Key works (MCP)

1. `McpApiKeyMiddleware` intercepts **every** request whose path starts with `/mcp`.
2. It reads the `X-Api-Key` header and performs a constant-time string comparison against the configured key.
3. Missing key → `401`; wrong key → `403`; correct key → request passes through to the MCP handler.

### Production checklist

- [ ] Replace the `JwtSettings:SecretKey` with a cryptographically strong random value (≥ 32 bytes).
- [ ] Replace `McpSettings:ApiKey` with a securely generated random value.
- [ ] Store secrets in environment variables, Azure Key Vault, or another secret manager — **never hard-code them**.
- [ ] Add real credential validation in `AuthController` (database lookup, password hashing, etc.).
- [ ] Enable HTTPS-only in production (`UseHsts`, proper TLS certificate).
- [ ] Consider adding rate limiting to the `/api/auth/token` endpoint to prevent brute-force attacks.

---

## Configuration Reference

`appsettings.json` (production placeholder values):

```json
{
  "JwtSettings": {
    "SecretKey": "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "McpExample",
    "Audience": "McpExampleClients",
    "ExpiryMinutes": "60"
  },
  "McpSettings": {
    "ApiKey": "CHANGE_ME_TO_A_SECURE_MCP_API_KEY"
  }
}
```

`appsettings.Development.json` (safe development defaults already populated):

```json
{
  "JwtSettings": {
    "SecretKey": "dev-secret-key-do-not-use-in-production-must-be-at-least-32-chars!",
    "Issuer": "McpExample",
    "Audience": "McpExampleClients",
    "ExpiryMinutes": "60"
  },
  "McpSettings": {
    "ApiKey": "dev-mcp-api-key-do-not-use-in-production"
  }
}
```
