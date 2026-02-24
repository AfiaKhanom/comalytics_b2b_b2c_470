# AI Context File for Comalytics B2B/B2C NopCommerce Project

**Purpose:** This file provides AI coding assistants with critical project context to generate accurate, compliant code that follows Comalytics standards.

**Target Audience:** GitHub Copilot, ChatGPT, Claude, and other AI development tools

**Last Updated:** 2026-02-15

---

## 🎯 Quick Start for AI

When asked to generate code for this project:
1. ✅ **READ THIS FILE FIRST** before making suggestions
2. ✅ **Follow NopStation coding conventions** (see [Coding Rules](#coding-rules))
3. ✅ **Never modify core NopCommerce files** (only work in `Plugins/`)
4. ✅ **Use FluentMigrator** for database changes (NOT Entity Framework)
5. ✅ **Always check branch naming** before creating features

---

## 📖 Table of Contents
1. [Project Identity](#project-identity) 
3. [Critical Constraints](#critical-constraints)
4. [Coding Rules](#coding-rules)
5. [Plugin Development Patterns](#plugin-development-patterns)
6. [Database Patterns](#database-patterns)
7. [Common AI Mistakes to Avoid](#common-ai-mistakes-to-avoid)
8. [Request Templates](#request-templates)
9. [File Location Guide](#file-location-guide)
10. [Testing Guidelines](#testing-guidelines)

---

## 🏢 Project Identity

### Project Name
**Comalytics B2B/B2C E-commerce Platform**

### Business Context
- **Domain:** Hybrid B2B/B2C e-commerce
- **Primary Users:** Business buyers (B2B) and retail consumers (B2C)
- **Key Differentiator:** ERP-integrated B2B features on B2C storefront
- **Industry:** Manufacturing/Distribution with complex account-based pricing

### Core Platform
- **Base:** NopCommerce 4.70 (Open Source E-commerce)
- **Customization:** NopStation B2B plugin suite + Custom ERP integration
- **Architecture:** Plugin-based extensibility (DO NOT modify core)

--- 

### Key NuGet Packages
```xml
<!-- Data Access -->
<PackageReference Include="linq2db" Version="5.4.1" />
<PackageReference Include="FluentMigrator" Version="5.2.0" />

<!-- PDF/Excel Export -->
<PackageReference Include="QuestPDF" Version="2022.12.15" />
<PackageReference Include="ClosedXML" Version="0.102.2" />

<!-- Email -->
<PackageReference Include="MailKit" Version="4.5.0" />

<!-- Mapping -->
<PackageReference Include="AutoMapper" Version="13.0.1" />
```

---

## 🚫 Critical Constraints

### ❌ NEVER DO THESE (Hard Failures)

#### 1. **DO NOT Modify Core NopCommerce Libraries**
```
❌ NEVER EDIT:
- src/Libraries/Nop.Core/**
- src/Libraries/Nop.Data/**
- src/Libraries/Nop.Services/**
- src/Presentation/Nop.Web.Framework/**
```

**Why:** Core files are from NopCommerce upstream. Modifications prevent updates and break support.

**Solution:** All customizations MUST go in `src/Plugins/NopStation.*` or `src/Plugins/Nop.Plugin.*`

---

#### 2. **DO NOT Use Entity Framework or EF Core**
```csharp
❌ WRONG (Do not suggest this):
public class MyContext : DbContext
{
    public DbSet<MyEntity> MyEntities { get; set; }
}

✅ CORRECT (Use FluentMigrator):
public class MyEntityBuilder : NopEntityBuilder<MyEntity>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(MyEntity.Name)).AsString(200).NotNullable();
    }
}
```

**Why:** NopCommerce uses Linq2Db + FluentMigrator for performance and flexibility.

---

#### 3. **DO NOT Create Cascade Delete Foreign Keys**
```csharp
❌ WRONG:
.WithColumn(nameof(Entity.ParentId))
    .AsInt32().ForeignKey<Parent>()  // Defaults to CASCADE

✅ CORRECT:
.WithColumn(nameof(Entity.ParentId))
    .AsInt32().ForeignKey<Parent>(onDelete: Rule.None)
```

**Why:** ERP sync operations fail when cascade deletes trigger unexpectedly across plugins.

---

#### 4. **DO NOT Use `this.` Qualifiers**
```csharp
❌ WRONG:
this.capacity = 0;
this.ProcessOrder();

✅ CORRECT:
capacity = 0;
ProcessOrder();
```

**Why:** NopStation coding standard (enforced via `.editorconfig`)
 
---

## 📏 Coding Rules

 
### File and Folder Naming

```
✅ CORRECT Structure:
src/Plugins/NopStation.Plugin.B2B.MyFeature/
├── Controllers/
│   └��─ MyFeatureController.cs          (Pascal case, Controller suffix)
├── Services/
│   ├── IMyFeatureService.cs            (Interface with I prefix)
│   └── MyFeatureService.cs
├── Domain/
│   └── MyFeatureEntity.cs              (Entity suffix optional)
├── Data/
│   └── Builders/
│       └── MyFeatureEntityBuilder.cs   (Builder suffix)
├── Models/
│   └── MyFeatureModel.cs               (Model suffix)
└── Views/
    └── MyFeature/
        └── Index.cshtml                (Pascal case)
```

---

## 🔌 Plugin Development Patterns

### Plugin `.csproj` Template

**Use this structure for ALL new plugins:**

```xml name=YourPlugin.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Copyright>Copyright © Comalytics</Copyright>
    <Company>Comalytics</Company>
    <Authors>Comalytics</Authors>
    <OutputPath>..\..\Presentation\Nop.Web\Plugins\YOUR_PLUGIN_SYSTEM_NAME</OutputPath>
    <OutDir>$(OutputPath)</OutDir>
    
    <!-- ⚠️ CRITICAL: Prevent NuGet DLL duplication -->
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
    
    <!-- ⚠️ Set to TRUE only if plugin has unique NuGet packages -->
    <!-- <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies> -->
  </PropertyGroup>

  <ItemGroup>
    <!-- Minimum required reference -->
    <ProjectReference Include="..\..\Presentation\Nop.Web\Nop.Web.csproj">
      <Private>False</Private>  <!-- Don't copy Nop.Web.dll to plugin folder -->
    </ProjectReference>
    
    <!-- If this is a NopStation plugin -->
    <ProjectReference Include="..\NopStation.Plugin.Misc.Core\NopStation.Plugin.Misc.Core.csproj">
      <Private>False</Private>
    </ProjectReference>
    
    <!-- Build cleanup script -->
    <ClearPluginAssemblies Include="..\..\Build\ClearPluginAssemblies.proj" />
  </ItemGroup>

  <!-- Views and static files -->
  <ItemGroup>
    <Content Include="plugin.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="logo.png">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="Views\**\*.cshtml">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- ⚠️ REQUIRED: Auto-cleanup after build -->
  <Target Name="NopTarget" AfterTargets="Build">
    <MSBuild Projects="@(ClearPluginAssemblies)" 
             Properties="PluginPath=$(MSBuildProjectDirectory)\$(OutDir)" 
             Targets="NopClear" />
  </Target>

</Project>
```

---

### Plugin Class Template

```csharp name=YourPlugin.cs
using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Localization;
using Nop.Services.Common;

namespace YourNamespace.Plugin.YourPlugin
{
    /// <summary>
    /// Brief description of what this plugin does
    /// </summary>
    public class YourPluginClass : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public YourPluginClass(
            IWebHelper webHelper,
            ILocalizationService localizationService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/YourController/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            // Add localization resources
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                "Plugins.YourPlugin.Fields.Setting1", 
                "Setting 1 Label");

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            // Remove localization resources
            await _localizationService.DeleteLocaleResourceAsync(
                "Plugins.YourPlugin.Fields.Setting1");

            await base.UninstallAsync();
        }
    }
}
```

**Critical Rules:**
- `SystemName` MUST match your namespace and DLL name
- `SupportedVersions` MUST include current NopCommerce version (`4.70`)
- `FileName` MUST match your compiled DLL name

---

## 💾 Database Patterns

### FluentMigrator Entity Builder Template

```csharp name=YourEntityBuilder.cs
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using YourNamespace.Domain;

namespace YourNamespace.Data.Builders
{
    /// <summary>
    /// Represents a YOUR_ENTITY entity builder
    /// </summary>
    public partial class YourEntityBuilder : NopEntityBuilder<YourEntity>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                // Primary key (auto-configured by NopEntityBuilder)
                
                // String columns
                .WithColumn(nameof(YourEntity.Name))
                    .AsString(200).NotNullable()
                
                // String columns (unlimited)
                .WithColumn(nameof(YourEntity.Description))
                    .AsString(int.MaxValue).Nullable()
                
                // Integer columns
                .WithColumn(nameof(YourEntity.Quantity))
                    .AsInt32().NotNullable().WithDefaultValue(0)
                
                // Decimal columns (for money)
                .WithColumn(nameof(YourEntity.Price))
                    .AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
                
                // Boolean columns
                .WithColumn(nameof(YourEntity.IsActive))
                    .AsBoolean().NotNullable().WithDefaultValue(true)
                
                // DateTime columns
                .WithColumn(nameof(YourEntity.CreatedOnUtc))
                    .AsDateTime2().NotNullable()
                
                // Foreign Keys (⚠️ ALWAYS use Rule.None)
                .WithColumn(nameof(YourEntity.CustomerId))
                    .AsInt32().ForeignKey<Customer>(onDelete: Rule.None);
        }

        #endregion
    }
}
```

---

### Foreign Key Rules (CRITICAL)

```csharp
// ✅ CORRECT: Always specify onDelete: Rule.None for cross-plugin references
.WithColumn(nameof(ErpAccount.CustomerId))
    .AsInt32().ForeignKey<Customer>(onDelete: Rule.None)

// ✅ ACCEPTABLE: Cascade delete ONLY within same plugin entities
.WithColumn(nameof(OrderItem.OrderId))
    .AsInt32().ForeignKey<Order>(onDelete: Rule.Cascade)

// ❌ WRONG: Default cascade causes ERP sync failures
.WithColumn(nameof(ErpAccount.CustomerId))
    .AsInt32().ForeignKey<Customer>()  // Defaults to CASCADE
```

**Why `Rule.None`?**
- Prevents accidental cascade deletes across plugin boundaries
- ERP integration requires manual cleanup to sync with external system
- Avoids orphaned records when NopCommerce Customer is deleted

---

### Data Type Mapping

| C# Type | FluentMigrator Method | SQL Server Type | Notes |
|---------|----------------------|-----------------|-------|
| `string` | `.AsString(200)` | `NVARCHAR(200)` | Fixed length |
| `string` | `.AsString(int.MaxValue)` | `NVARCHAR(MAX)` | Unlimited |
| `int` | `.AsInt32()` | `INT` | 32-bit integer |
| `long` | `.AsInt64()` | `BIGINT` | 64-bit integer |
| `decimal` | `.AsDecimal(18, 4)` | `DECIMAL(18,4)` | Money fields |
| `bool` | `.AsBoolean()` | `BIT` | True/False |
| `DateTime` | `.AsDateTime2()` | `DATETIME2` | Preferred over `DATETIME` |
| `Guid` | `.AsGuid()` | `UNIQUEIDENTIFIER` | UUIDs |

---

## ⚠️ Common AI Mistakes to Avoid

### Mistake #1: Suggesting Entity Framework
```
❌ AI Often Says:
"Let's add a DbContext and use EF migrations..."

✅ CORRECT Response:
"This project uses FluentMigrator for database changes. 
Create a builder class in Data/Builders/ folder."
```

---

### Mistake #2: Editing Core Files
```
❌ AI Often Says:
"Modify Nop.Services/Customers/CustomerService.cs to add..."

✅ CORRECT Response:
"Cannot modify core files. Create a plugin in src/Plugins/ 
and use event handlers or decorators to extend functionality."
```

---

### Mistake #3: Wrong Dependency Injection
```csharp
✅ AI Often Suggests:
services.AddScoped<IMyService, MyService>();  // In Startup.cs

✅  CORRECT (Plugin Pattern):
// In YourPlugin/Infrastructure/DependencyRegistrar.cs
builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();
```

---

### Mistake #4: Incorrect Async Patterns
```csharp
❌ WRONG:
public Task<Order> GetOrderById(int id)  // Missing "Async" suffix
{
    return Task.Run(() => _repository.GetById(id));  // Don't use Task.Run
}

✅ CORRECT:
public async Task<Order> GetOrderByAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}
```

---

### Mistake #5: Hardcoding Strings Instead of Localization
```csharp
❌ WRONG:
ViewBag.Message = "Order created successfully";

✅ CORRECT:
ViewBag.Message = await _localizationService
    .GetResourceAsync("Plugins.YourPlugin.OrderCreated");
```

---

## 📝 Request Templates

### When Creating a New Feature
```
Please create a [FEATURE NAME] for the Comalytics B2B plugin.

Context:
- Plugin Name: NopStation.Plugin.B2B.YourFeature
- Purpose: [Describe what this feature does]
- Integration Points: [Which services/entities it interacts with]

Requirements:
1. Follow PROJECT_ARCHITECTURE.md structure
2. Use FluentMigrator for database changes
3. Follow NopStation coding conventions from CONTEXT.md
4. Include localization resources
5. Create proper FluentValidation validators

Please generate:
[ ] Plugin class
[ ] Domain entities
[ ] FluentMigrator builders
[ ] Service interfaces and implementations
[ ] Controllers and view models
[ ] Razor views
[ ] plugin.json
[ ] .csproj file
```

---

### When Debugging an Issue
```
I'm encountering [ERROR MESSAGE] when [ACTION].

Project Context:
- See CONTEXT.md for coding rules
- See PROJECT_ARCHITECTURE.md for structure
- This is a NopCommerce 4.70 plugin system

Relevant Code:
[Paste code snippet]

Please:
1. Identify the root cause
2. Suggest a fix following NopStation conventions
3. Explain why the error occurred
```

---

### When Requesting Database Changes
```
I need to [DESCRIBE CHANGE] in the database.

Current Schema:
[Paste current entity/table structure]

Requirements:
- Use FluentMigrator (NOT Entity Framework)
- Follow Foreign Key rules (onDelete: Rule.None for cross-plugin refs)
- Include both Up() and Down() migrations

Please generate:
[ ] Entity class in Domain/
[ ] FluentMigrator builder in Data/Builders/
[ ] Migration script (if schema change)
```

---

## 📁 File Location Guide

### "Where do I put this file?"

| File Type | Location | Example |
|-----------|----------|---------|
| **Plugin Main Class** | `src/Plugins/{PluginName}/` | `NopStation.Plugin.B2B.MyFeature.cs` |
| **Domain Entities** | `src/Plugins/{PluginName}/Domain/` | `MyFeatureEntity.cs` |
| **Entity Builders** | `src/Plugins/{PluginName}/Data/Builders/` | `MyFeatureEntityBuilder.cs` |
| **Services (Interface)** | `src/Plugins/{PluginName}/Services/` | `IMyFeatureService.cs` |
| **Services (Implementation)** | `src/Plugins/{PluginName}/Services/` | `MyFeatureService.cs` |
| **Controllers** | `src/Plugins/{PluginName}/Controllers/` | `MyFeatureController.cs` |
| **View Models** | `src/Plugins/{PluginName}/Models/` | `MyFeatureModel.cs` |
| **Validators** | `src/Plugins/{PluginName}/Validators/` | `MyFeatureValidator.cs` |
| **Views** | `src/Plugins/{PluginName}/Views/` | `Index.cshtml` |
| **DI Registration** | `src/Plugins/{PluginName}/Infrastructure/` | `DependencyRegistrar.cs` |
| **Route Provider** | `src/Plugins/{PluginName}/Infrastructure/` | `RouteProvider.cs` |
| **Event Handlers** | `src/Plugins/{PluginName}/Events/` | `OrderPlacedEventConsumer.cs` |

---
  

## 🔐 Security Rules

### Input Validation
```csharp
// ✅ ALWAYS validate user input
[HttpPost]
public async Task<IActionResult> Create(MyModel model)
{  
    // ✅ ALWAYS check permissions
    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        return AccessDeniedView();

    if (ModelState.IsValid){
       // Process...
    }

    //prepare model
        return View(model); 
}
```


---

## 📚 Additional Resources

### Internal Documentation
- `CodingStructure.md` — Full NopStation C# style guide
- `PROJECT_ARCHITECTURE.md` — Complete system architecture
- `PR_Review_Guideline.md` — Branch naming, commit messages
- `VERSION_CONTROL_AND_RELEASE.md` — Git workflow
- `Deployment_GuideLine.md` — Deployment checklist

### External Documentation
- [NopCommerce Official Docs](https://docs.nopcommerce.com)
- [NopCommerce Plugin Development](https://docs.nopcommerce.com/en/developer/plugins/)
- [FluentMigrator Documentation](https://fluentmigrator.github.io/)
- [Linq2Db Documentation](https://linq2db.github.io/)

---

## 🎓 Learning Examples

### Example 1: Complete Plugin with Database Table

See: `src/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/`

**What to study:**
- ✅ Plugin structure (`NopStation.Plugin.B2B.ERPIntegrationCore.csproj`)
- ✅ Domain entities (`Domain/ErpAccount.cs`)
- ✅ FluentMigrator builders (`Data/Builders/ErpLogsBuilder.cs`)
- ✅ Service pattern (`Services/IErpAccountService.cs`)
- ✅ Dependency registration (`Infrastructure/DependencyRegistrar.cs`)

---

### Example 2: Controller with Views

See: `src/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Controllers/`

**What to study:**
- ✅ Admin controller pattern
- ✅ Permission checks
- ✅ Model validation
- ✅ View model factories
- ✅ Localization usage

---


## ✅ Checklist: Before Committing Code

- [ ] Code follows NopStation conventions (no `this.` qualifiers)
- [ ] All async methods have `Async` suffix
- [ ] FluentMigrator used for database changes (NOT EF)
- [ ] Foreign keys use `onDelete: Rule.None`
- [ ] Plugin `.csproj` includes cleanup target
- [ ] Localization resources added
- [ ] No hardcoded strings in views
- [ ] No modifications to core NopCommerce files
- [ ] Branch name follows `feature/FFN-{ticket}-{description}`
- [ ] Commit message follows `FFN-{ticket}-{message}`
 