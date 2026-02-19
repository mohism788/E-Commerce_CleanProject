# ⚡ Berserk Tech E-Commerce API

> "Sacrifice Everything For Power" - The backend engine driving the **Berserk Tech** e-commerce platform.

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-8.0-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?logo=swagger)](https://swagger.io/)

A robust, high-performance RESTful Web API built with **.NET 8** and **Entity Framework Core**. This backend powers the entire Berserk Tech ecosystem, handling authentication, product management, complex transactions, and order fulfillment with a focus on reliability and scalability.

---

## 🚀 Key Features

### 🔐 Multi-Role Authentication
- **Secure Identity Management:** Powered by **ASP.NET Core Identity**.
- **Role-Based Access Control (RBAC):** Distinct roles for **Admin**, **Seller**, and **Customer**.
- **JWT Authentication:** Secure stateless authentication for all protected endpoints.

### 🛒 Dynamic E-Commerce Core
- **Advanced Product Catalog:** Full CRUD operations with rich metadata, image support, and stock management.
- **Smart Search & Filtering:** Filter products by category, price range, seller, and stock status.
- **Persistent Shopping Cart:** Seamless cart management that persists across sessions.

### 💳 Transactional Integrity
- **Robust Order Processing:** Atomicity guaranteed using **EF Core Execution Strategies**.
- **Stock Validation:** Real-time inventory checks during "Add to Cart" and "Checkout" flows.
- **Safe Transactions:** Retrying execution strategies to handle transient database failures seamlessly.

### 📊 Seller Dashboard API
- **Inventory Management:** CRUD endpoints for sellers to manage their listings.
- **Sales Analytics:** Aggregated data for sales, revenue, and order history.

---

## 🛠️ Technology Stack

- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Database:** SQL Server (Production), LocalDB (Development)
- **ORM:** Entity Framework Core
- **Mapping:** AutoMapper
- **Documentation:** Swagger / OpenAPI
- **Logging:** Serilog (Structured Logging)

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express or Developer)
- Visual Studio 2022 or VS Code

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/your-username/berserk-tech-backend.git
    cd berserk-tech-backend
    ```

2.  **Configure Database**
    Update `appsettings.json` with your SQL Server connection string:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER;Database=BerserkTechDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```

3.  **Apply Migrations**
    Initialize the database schema:
    ```bash
    dotnet ef database update
    ```

4.  **Run the API**
    ```bash
    dotnet run
    ```
    The API will start at `https://localhost:7153` (or your configured port).

5.  **Explore Documentation**
    Visit `https://localhost:7153/swagger` to view the interactive API documentation.

---

## 📂 Project Structure

- **Controllers:** changing state and handling HTTP requests.
- **Models:** Entity definitions representing database tables.
- **DTOs:** Data Transfer Objects for secure and optimized data exchange.
- **Data:** `DbContext` configuration and `DbInitializer` for seeding.
- **Services:** Business logic layer (Order processing, Stock checks).
- **Repositories:** Implementation of the Repository Pattern with Unit of Work.


