# Comalytics B2B/B2C NopCommerce Project Architecture

**Version:** 4.70.0 (NopCommerce)  
**Framework:** .NET 8.0  
**Company:** Comalytics Ltd.  
**Last Updated:** 2026-02-15

---

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [Technology Stack](#technology-stack)
2. [Directory Structure](#directory-structure)
3. [Layer Architecture](#layer-architecture)
4. [Core Libraries](#core-libraries)
5. [Plugin Ecosystem](#plugin-ecosystem)
6. [Key Custom Plugins](#key-custom-plugins)
7. [Database Architecture](#database-architecture)
8. [Dependency Graph](#dependency-graph)
9. [Build and Deployment](#build-and-deployment) 

---

## 🎯 System Overview

**Comalytics** is a customized NopCommerce 4.70 e-commerce platform providing **B2B buyers with B2C buying convenience** combined with enterprise-grade B2B functionality. The system integrates with an external ERP system for account management, pricing, and order synchronization.

### Key Features
- **Hybrid B2B/B2C Storefront** — Single platform serving both customer types
- **ERP Integration** — Real-time sync with external ERP for accounts, pricing, orders
- **Custom B2B Workflows** — Quick order, bulk ordering, account-based pricing
- **Multi-environment Support** — Test, Staging, Live with cherry-pick deployment

---
 
## 💻 Technology Stack

### Backend
```yaml
Framework: .NET 8.0 (LTS)
Language: C# 12
Web Framework: ASP.NET Core MVC 8.0
ORM: Linq2Db 5.4.1 (NOT Entity Framework)
Migrations: FluentMigrator 5.2.0
DI Container: Autofac 9.0.0
Validation: FluentValidation 11.3.0
```

### Database
```yaml
Primary: SQL Server 2019+
Supported: MySQL 8.0+, PostgreSQL 12+
Connection String Location: src/Presentation/Nop.Web/App_Data/appsettings.json
```

### Frontend
```yaml
View Engine: Razor Pages
CSS Framework: Bootstrap 4.6.0
Admin UI: AdminLTE 3.2.0
JavaScript: jQuery 3.7.1, vanilla JS (minimal SPA)
Build Tools: Gulp 4.0.2, npm
```
---

## 📂 Directory Structure

```
comalytics_b2b_b2c_470/
├── src/                                  # Main source code
│   ├── Build/                            # Build scripts and plugin cleanup
│   │   ├── ClearPluginAssemblies.proj    # MSBuild cleanup script
│   │   └── src/ClearPluginAssemblies/    # Plugin cleanup utility
│   │
│   ├── Libraries/                        # Core NopCommerce libraries
│   │   ├── Nop.Core/                     # Domain entities, events, caching
│   │   ├── Nop.Data/                     # Data access (FluentMigrator, Linq2Db)
│   │   └── Nop.Services/                 # Business logic layer
│   │
│   ├── Presentation/                     # Frontend layers
│   │   ├── Nop.Web/                      # Main web application (MVC)
│   │   │   ├── Controllers/              # MVC controllers
│   │   │   ├── Views/                    # Razor views
│   │   │   ├── wwwroot/                  # Static assets
│   │   │   ├── Plugins/                  # Compiled plugin output (runtime)
│   │   │   └── Themes/                   # UI themes (DefaultClean, NopSmart, etc.)
│   │   └── Nop.Web.Framework/            # Presentation utilities and base classes
│   │
│   ├── Plugins/                          # All plugin source code
│   │   ├── NopStation.Plugin.B2B.B2BB2CFeatures/        # ⭐ MAIN B2B PLUGIN
│   │   ├── NopStation.Plugin.B2B.ERPIntegrationCore/     # ⭐ ERP INTEGRATION
│   │   ├── NopStation.Plugin.Misc.Core/                  # NopStation base plugin
│   │   ├── Nop.Plugin.Tax.Avalara/                       # Tax integration
│   │   ├── Nop.Plugin.Misc.Omnisend/                     # Marketing automation
│   │   └── [70+ other plugins...]                        # Standard NopCommerce plugins
│   │
│   └── Tests/                            # Unit and integration tests
│
├── CodingStructure.md                    # NopStation coding conventions
├── PR_Review_Guideline.md                # Branch naming, PR process
├── VERSION_CONTROL_AND_RELEASE.md        # Git workflow (feature/test/release)
├── Deployment_GuideLine.md               # Deployment checklist
└── ProjectArchitecture.txt               # ASCII art architecture diagram
```

---

## 🏗️ Layer Architecture

### Standard NopCommerce 4-Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Nop.Web    │  │Nop.Web.Frame-│  │   Themes     │       │
│  │ (ASP.NET    │  │    work      │  │ • DefaultClean│      │
│  │  MVC App)   │  │ (Utilities)  │  │ • NopSmart   │       │
│  └─────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    PLUGIN LAYER                             │
│  Custom business logic via NopCommerce plugin system        │
│  ⭐ B2BB2CFeatures | ERP Integration | Tax | Payments      │
└─────────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER (Nop.Services)             │
│  Business logic: Customer, Order, Catalog, Shipping, etc.   │
└─────────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    DATA LAYER (Nop.Data)                    │
│  Database access using Linq2Db + FluentMigrator             │
│  Entities defined in Nop.Core                               │
└─────────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE (SQL Server)                    │
│  All entities, plugin tables, ERP sync tables               │
└─────────────────────────────────────────────────────────────┘
```
---

## 📚 Core Libraries

### 1. **Nop.Core** (Domain Layer)
**Purpose:** Core entities, domain models, events, caching, configuration  
**Key Namespaces:**
- `Nop.Core.Domain.*` — All domain entities (Customer, Order, Product, etc.)
- `Nop.Core.Caching` — Cache management interfaces
- `Nop.Core.Events` — Event publishing system
- `Nop.Core.Infrastructure` — DI, plugins, startup configuration

**Dependencies:**
```xml
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

---

### 2. **Nop.Data** (Data Access Layer)
**Purpose:** Database mappings, migrations, repositories  
**Technology Stack:**
- **ORM:** `Linq2Db` (Version 5.4.1)
- **Migrations:** `FluentMigrator` (Version 5.2.0)
- **Supported Databases:** SQL Server, MySQL, PostgreSQL

**Key Components:**
- `Mapping/Builders/` — FluentMigrator entity builders
- `Migrations/` — Database migrations
- `NopDataProvider` — Abstract data provider pattern



---

### 3. **Nop.Services** (Business Logic Layer)
**Purpose:** Business operations, validations, workflows  
**Key Service Categories:**
- `Catalog/` — Product, category, manufacturer services
- `Customers/` — Customer registration, authentication
- `Orders/` — Order processing, checkout, payments
- `Shipping/` — Shipping calculation, warehouse management
- `Tax/` — Tax calculation
- `Directory/` — Countries, states, currencies

**Dependencies:**
```xml
<PackageReference Include="MailKit" Version="4.5.0" />
<PackageReference Include="ClosedXML" Version="0.102.2" />  <!-- Excel export -->
<PackageReference Include="QuestPDF" Version="2022.12.15" />  <!-- PDF generation -->
<PackageReference Include="System.Linq.Dynamic.Core" Version="1.3.12" />
```

---

### 4. **Nop.Web.Framework** (Presentation Utilities)
**Purpose:** MVC base classes, routing, validation, helpers  
**Key Components:**
- `Mvc/Routing/` — Custom route providers
- `Validators/` — FluentValidation base classes
- `Factories/` — View model factories
- `Infrastructure/` — Dependency registration for presentation layer

**Dependencies:**
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="LigerShark.WebOptimizer.Core" Version="3.0.405" />
<PackageReference Include="WebMarkupMin.AspNetCore8" Version="2.16.0" />
```
---
## ⭐ Key Custom Plugins

### 1. **NopStation.Plugin.B2B.B2BB2CFeatures** (Main B2B Plugin)
**Path:** `src/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/`  
**Purpose:** Core B2B functionality (account-based pricing, quick order, bulk ordering)

**Key Features:**
- **Controllers:**
  - `B2BCustomerController` — B2B customer registration/management
  - `QuickOrderController` — Bulk SKU-based ordering
  - `ErpCheckoutController` — Custom B2B checkout flow
  - `ExportController` — Order history export

- **Services:**
  - `IErpCustomerService` — ERP account linking
  - `IB2BOrderTotalCalculationService` — Custom pricing logic
  - `IShoppingCartService` — Override for B2B cart rules
  - `IWorkflowMessageService` — Custom B2B email notifications

- **Factories:**
  - `B2BRegisterModelFactory` — B2B registration forms
  - `ErpAccountModelFactory` — ERP account selection
  - `ErpProductOverrideFactory` — Custom product display

- **View Components:**
  - `B2BProductBoxViewComponent` — Custom product cards
  - `OrderSummaryViewComponent` — B2B order summary
  - `QuickOrderViewComponent` — Quick order widget

**Database Tables (Managed by this plugin):**
- `B2BCustomerMapping` — Links Customer to ERP account
- `QuickOrderHistory` — Saved quick order templates
- `B2BPricing` — Account-specific pricing overrides

---

### 2. **NopStation.Plugin.B2B.ERPIntegrationCore** ⭐⭐⭐
**Path:** `src/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/`  
**System Name:** `NopStation.Plugin.B2B.ERPIntegrationCore`  
**Purpose:** **Critical integration layer** between NopCommerce and external ERP system

#### **Core Entities** (All in `Domain/` folder)
```
ErpAccount               — ERP customer account (maps to Company in ERP)
ErpNopUser               — ERP user record (maps to Contact in ERP)
ErpNopUserAccountMap     — Many-to-many: Users ↔ Accounts
ErpShipToAddress         — Delivery addresses from ERP
ErpShiptoAddressErpAccountMap — Many-to-many: Addresses ↔ Accounts
ErpLogs                  — Audit trail for all ERP operations
```
 

#### **Critical Pattern: Foreign Keys**
⚠️ **ALWAYS use `onDelete: Rule.None`** for cross-plugin foreign keys to prevent cascade delete issues.

```csharp
// ✅ CORRECT
.WithColumn(nameof(Entity.ForeignId))
    .AsInt32().ForeignKey<Parent>(onDelete: Rule.None)

// ❌ WRONG (Will cause ERP sync failures)
.WithColumn(nameof(Entity.ForeignId))
    .AsInt32().ForeignKey<Parent>()
```

#### **Services (in `Services/` folder)**
- `IErpSyncService` — Main sync orchestration
- `IErpAccountService` — ERP account management
- `IErpUserService` — ERP user operations
- `IErpAddressService` — Address sync
- `IErpLogService` — Logging operations

#### **ERP Sync Flow**
```
1. Customer places order → Nop.Web Order Created
2. Order event triggers → B2BB2CFeatures event handler
3. Event handler calls → ErpSyncService.SyncOrderToErp()
4. API call to ERP → External ERP REST API
5. Response logged → ErpLogs table
6. Success/failure → Customer notification
```
 

---

## 💾 Database Architecture

### Database Provider
**Primary:** SQL Server  
**Supported:** MySQL, PostgreSQL (via Linq2Db)

### Connection String Location
`src/Presentation/Nop.Web/App_Data/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SERVER\\INSTANCE;Initial Catalog=CommalyticsDB;Integrated Security=True;Trust Server Certificate=True"
  }
}
```

---

## 🔗 Dependency Graph

### Project Dependencies
```
Nop.Web
  ├─ Nop.Web.Framework
  │   ├─ Nop.Services
  │   │   ├─ Nop.Data
  │   │   │   └─ Nop.Core
  │   │   └─ Nop.Core
  │   └─ Nop.Data
  │       └─ Nop.Core
  └─ Nop.Core

Plugins (All depend on at least Nop.Web or Nop.Services)
  NopStation.Plugin.B2B.B2BB2CFeatures
    ├─ Nop.Web
    ├─ NopStation.Plugin.Misc.Core
    └─ NopStation.Plugin.B2B.ERPIntegrationCore (implicit via services)

  NopStation.Plugin.B2B.ERPIntegrationCore
    ├─ Nop.Web
    └─ NopStation.Plugin.Misc.Core
```

### Circular Dependency Prevention
- **Plugins CANNOT reference each other's projects** (circular ref issue)
- **Solution:** Plugins communicate via:
  1. Event system (`IEventPublisher`)
  2. Service layer interfaces registered in DI
  3. Direct service calls via `EngineContext.Current.Resolve<IService>()`

---

## 🛠️ Build and Deployment

### Build Process
1. **NuGet Restore:** `dotnet restore src/comalytics-nopstation-b2b-nop-470.sln`
2. **Compilation:** `dotnet build` (Triggers plugin builds automatically)
3. **Plugin Cleanup:** `ClearPluginAssemblies.proj` runs after each plugin build
4. **Output:** All plugins → `src/Presentation/Nop.Web/Plugins/`

### Plugin Cleanup Mechanism
**Problem:** .NET 8 copies ALL referenced DLLs to plugin output (bloat)  
**Solution:** `ClearPluginAssemblies` deletes core DLLs from plugin folders after build
 
---

## 📞 Quick Reference

### Important Files
| File | Purpose |
|------|---------|
| `CodingStructure.md` | C# style guide |
| `PR_Review_Guideline.md` | Branch naming, commit format |
| `VERSION_CONTROL_AND_RELEASE.md` | Git workflow |
| `Deployment_GuideLine.md` | Deployment steps |
| `App_Data/appsettings.json` | Connection string, settings |
| `App_Data/plugins.json` | Installed plugins registry |

### Key Directories
| Path | Contains |
|------|----------|
| `src/Plugins/` | Plugin source code |
| `src/Presentation/Nop.Web/Plugins/` | Compiled plugin binaries (runtime) |
| `src/Presentation/Nop.Web/Themes/` | UI themes |
| `src/Build/` | Build utilities |

### NopCommerce Version
**4.70.0** — Released December 2024  
Based on .NET 8.0 LTS

---

## 🔍 Additional Resources

- **NopCommerce Official Docs:** https://docs.nopcommerce.com
- **NopStation Docs:** See `CodingStructure.md`
- **B2B Plugin Docs:** Internal SharePoint (see `README.md` links)
- **ERP API Docs:** [Request from DevOps team]
 