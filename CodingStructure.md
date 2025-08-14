# NopStation Coding Conventions

This document outlines the coding conventions used in NopStation, which are based on the nopCommerce guidelines. Adherence to these guidelines ensures consistent and high-quality source code.

## Code Style Enforcement

The coding guidelines are enforced through the `.editorconfig` file in Visual Studio, which helps maintain the desired code style across different development environments.

## Supported .NET Coding Convention Categories

1. **Language Conventions**
2. **Formatting Conventions**
3. **Naming Conventions**

### 1. Language Conventions

#### .NET Code Style Settings

- **"this." Qualifiers**
  - Avoid using `this.` for fields, properties, methods, or events.
    
    ✅ Correct: `capacity = 0;`
    
    ❌ Incorrect: `this.capacity = 0;`

- **Language Keywords Instead of Framework Type Names**
  - Use language keywords for type references when possible.
    
    ✅ Correct: `private int _member;`
    
    ❌ Incorrect: `private Int32 _member;`

- **Modifier Preferences**
  - Always declare accessibility modifiers except for public interface members.
    
    ✅ Correct: `private const string thisFieldIsConst = "constant";`
    
    ❌ Incorrect: `const string thisFieldIsConst = "constant";`
    
  - Follow the specified order for modifiers: `public, private, protected, internal, static, extern, new, virtual, abstract, sealed, override, readonly, unsafe, volatile, async:silent`

- **Parentheses Preferences**
  - Use parentheses to clarify operator precedence.
    
    ✅ Correct: `var v = a + (b * c);`
    
    ❌ Incorrect: `var v = a + b * c;`

### Expression-Level Preferences

- **Use object initializers where possible.**
    
    ✅ Correct: `var c = new Customer() { Age = 21 };`
    
    ❌ Incorrect: `var c = new Customer(); c.Age = 21;`

- **Use collection initializers where possible.**
    
    ✅ Correct: `var list = new List<int> { 1, 2, 3 };`
    
    ❌ Incorrect: `var list = new List<int>(); list.Add(1); list.Add(2); list.Add(3);`

- **Prefer tuple names over `ItemX` properties.**
    
    ✅ Correct: `(string name, int age) customer = GetCustomer(); var name = customer.name;`
    
    ❌ Incorrect: `(string name, int age) customer = GetCustomer(); var name = customer.Item1;`

- **Use inferred tuple element names.**
    
    ✅ Correct: `var tuple = (age, name);`
    
    ❌ Incorrect: `var tuple = (age: age, name: name);`

- **Use explicit anonymous type member names.**
    
    ✅ Correct: `var anon = new { age = age, name = name };`
    
    ❌ Incorrect: `var anon = new { age, name };`

- **Prefer auto-properties over properties with private backing fields.**
    
    ✅ Correct: `private int Age { get; }`
    
    ❌ Incorrect: `private int age; public int Age { get { return age; } }`

- **Use null-check with pattern-matching over `object.ReferenceEquals`.**
    
    ✅ Correct: `if (value is null) return;`
    
    ❌ Incorrect: `if (object.ReferenceEquals(value, null)) return;`

- **Use ternary conditional for assignments and return statements.**
    
    ✅ Correct: `string s = expr ? "hello" : "world";`
    
    ❌ Incorrect: `string s; if (expr) { s = "hello"; } else { s = "world"; }`

- **Prefer compound assignment expressions.**
    
    ✅ Correct: `x += 1;`
    
    ❌ Incorrect: `x = x + 1;`

### Null-Checking Preferences

- **Use null-coalescing expressions over ternary operator checking.**
    
    ✅ Correct: `var v = x ?? y;`
    
    ❌ Incorrect: `var v = x != null ? x : y;`

- **Use null-conditional operator when possible.**
    
    ✅ Correct: `var v = o?.ToString();`
    
    ❌ Incorrect: `var v = o == null ? null : o.ToString();`

### Formatting Conventions

#### .NET Formatting Settings

- **Organize Using Directives**
  - Sort `System.*` directives alphabetically and place them before other directives.
    
    ✅ Correct: `using System.Collections.Generic; using System.Threading.Tasks; using Octokit;`
    
    ❌ Incorrect: `using System.Collections.Generic; using Octokit; using System.Threading.Tasks;`

### Naming Conventions

#### .NET Naming Conventions

- **Prefix and Suffix**
  - Do not use Hungarian notation for variables or fields.
    
    ✅ Correct: `public DateTime DateTimeProvider { get; }`
    
    ❌ Incorrect: `public DateTime dtProvider { get; }`
  - Do not use `_` or `m_` prefixes for member variables.
    
    ✅ Correct: `private string name;`
    
    ❌ Incorrect: `private string _name;`

- **Capitalization**
  - Use PascalCase for public members, types, and non-private fields.
    
    ✅ Correct: `public DateTime DateTimeProvider { get; }`
    
    ❌ Incorrect: `public DateTime dateTimeProvider { get; }`
  - Use camelCase for private and internal fields.
    
    ✅ Correct: `private DateTime dateTimeProvider;`
    
    ❌ Incorrect: `private DateTime DateTimeProvider;`

### Additional Notes

- **Use Regions Sparingly**: While regions can be helpful in organizing code, overuse can make code more difficult to navigate. Use them judiciously to group related pieces of code, such as fields, properties, and methods.

- **Commenting and Documentation**: Use XML comments (`///`) for public members to facilitate automatic documentation generation. Ensure comments are clear, concise, and provide value beyond what can be inferred from the code itself.

## NopCommerce Source Code Organization

- **Libraries**
  - `Nop.Core`: Contains core classes for nopCommerce like caching, events, helpers, and business objects (e.g., Order and Customer entities).
  - `Nop.Data`: Handles data access using the Linq2DB Code-First approach, separating data-access logic from business objects.
  - `Nop.Services`: Contains business logic, validations, or calculations related to the data.

- **Plugins**
  - Plugins are contained within a Visual Studio solution folder and are physically located at the root of your solution. They are automatically copied to the `\Presentation\Nop.Web\Plugins` directory.

- **Presentation**
  - `Nop.Web`: MVC web application project for the public store, including an administration panel.
  - `Nop.Web.Framework`: Contains common presentation components for the `Nop.Web` project.

- **Tests**
  - Various test projects for different layers of the application, ensuring the reliability and robustness of the code.

### Adherence to Guidelines

Ensuring all team members adhere to these coding standards is essential for maintaining code quality, readability, and consistency. Regular code reviews, automated code analysis tools, and adherence to the `.editorconfig` settings can help enforce these standards effectively.

### References
1. NopCommerce coding standards
2. nopStation coding standards
