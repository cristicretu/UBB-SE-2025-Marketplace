# UBB-SE-2025-Marketplace

# Coding Style Rules

## Naming Conventions
1. Use PascalCase for class names, interface names, and method names.
2. Use camelCase for variable names, parameter names, and private fields.
3. Interface names must begin with the letter 'I' (e.g., ICustomerService).
4. Constant fields should be named using PascalCase.
5. Do not use Hungarian notation for variable names.
6. Do not prefix field names with underscores.

## Code Layout
7. Braces should always be on their own line for methods, classes, and control structures.
8. Each statement should be on its own line; do not place multiple statements on a single line.
9. Do not use more than one blank line in a row.
10. Code should not contain trailing whitespace at the end of lines.
11. Closing braces should not be preceded by a blank line.
12. Single-line comments should begin with a single space after the comment delimiter (// Comment).
13. Opening braces should not be preceded by a blank line.

## Syntax and Formatting
14. Use tabs for indentation instead of spaces.
15. Use string.Empty instead of "" for empty strings.
16. Use built-in type aliases (e.g., string instead of String, int instead of Int32).
17. Always include braces for control structures (if, for, while), even for single-line blocks.
18. Always declare access modifiers explicitly (public, private, protected, internal).
19. Use shorthand syntax for nullable types (int? instead of Nullable<int>).

## Organization
20. System using directives should be placed before other using directives.
21. Property accessors should follow order: get then set.
22. Use arithmetic expressions with explicit parentheses to declare precedence.
23. Use conditional expressions with explicit parentheses to declare precedence.

---

## Features

- 🛍️ **Product Management**
  - Buying products
  - Selling products
  - Borrowing products
  - Auction system
- 🔍 **Search & Filter**
  - Product filtering by tags
  - Advanced search functionality
- 📊 **User Features**
  - Product reviews
  - Product comparison
  - Shopping basket
  - Bidding system

## Dev Setup

Create an `appsettings.json` file in the `MarketMinds` and `MarketMinds.Web` directory with the following content:

```json
{
  "ImgurSettings": {
    "ClientId": "your imgur client id"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5001"
  },
  "GeminiAPI": {
    "Key": "your gemini api key"
  }
}
```

and one in the `server` directory with the following content:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiSettings": {
    "BaseUrl": "http://localhost:5001"
  },
  "LocalDataSource": "your conection string",
  "InitialCatalog": "your database table name",
}
```

You can find the pipe by running:
```cmd
SqlLocalDB.exe start
SqlLocalDB.exe info MSSQLLocalDB
```
