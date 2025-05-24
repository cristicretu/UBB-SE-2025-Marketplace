# UBB-SE-2025-Marketplace

# 🚀  Project Setup and Deadlines


## 🔄 Project Flows

### Flow 1 (Team 921)
- Login system with captcha 
- Account page cleanup (replacing Messi team implementation)
- Seller and buyer pages with update functionality
- Admin page for managing categories and conditions
- Borrowing functionality
- Contract PDF generation
- Family sharing feature
- Notifications system
- Contract renewal

### Flow 2 (Team 923)
- Chatbot implementation
- Borrowing creation
- Product purchase functionality
- Shopping cart management
- Checkout process
- Order history
- Order tracking
- Seller review system and trust score
- Auction functionality
- Wishlist management

## ⏰ **DEADLINES** ⏰

| Component | Deadline |
|-----------|----------|
| **FLOW 1** | **Wednesday, May 21, 2025, 23:59** |
| **FLOW 2** | **Friday, May 23, 2025, 23:59** |
| **TESTS** | **Saturday, May 24, 2025, 23:59** (testing begins Thursday) |

## 💻 Setup Instructions

### Main Application (Marketplace)

Follow the configuration guide below to set up the main marketplace application. This uses the configuration from the Messi team.

### Reference Projects

#### Messi Project (Reference)
Repository: https://github.com/cristicretu/UBB-SE-2025-MarketMessi

Project structure:
- Desktop: MarketMinds
- Server: server
- Shared: MarketMinds.Shared
- Web: MarketMinds.Web

#### Loanshark Project (Reference)
Repository: https://github.com/UBB-SE-921/UBB-SE-2025-921-1

Project structure:
- Desktop: MarketPlace924
- Server: Server
- Shared: SharedClassLibrary

## ⚙️ Configuration

### Required Configuration Files

You need to create `appsettings.json` files in multiple locations:

#### For Messi Project:
- `MarketMinds/appsettings.json`
- `MarketMinds.Web/appsettings.json`
- `server/appsettings.json`

#### For Loanshark Project:
- `Desktop/appsettings.json`
- `Web/appsettings.json`
- `Server/appsettings.json`

### Configuration Content

#### Desktop and Web Configuration (Messi):
```json
{
  "ImgurSettings": {
    "ClientId": "yourkey"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5001"
  },
  "GeminiAPI": {
    "Key": "yourkey"
  }
}
```

#### Server Configuration (Messi):
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
  "LocalDataSource": "np:\\\\.\\pipe\\LOCALDB#B6153E8E\\tsql\\query",
  "InitialCatalog": "MarketEF",
  "GeminiAPI": {
    "Key": "yourkey"
  }
}
```

> **Note:** For the LocalDataSource, you need to put just your server name. InitialCatalog specifies the database name.

#### Desktop and Web Configuration (Loanshark):
```json
{
  "BaseApiUrl": "https://localhost:7194/"
}
```

#### Server Configuration (Loanshark):
```json
{
  "ConnectionStrings": {
    "MyLocalDb": "Data Source=YOURSERVER;Initial Catalog=MarketPlaceDBCodeFirst;Integrated Security=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==cgXJiEqfVU2j7MyNXf16gy2g34g34g4w53r23dqjwcbiu23gduisy2fg8723ud2yuv3fVUWG&YBF3813r8g4732fh348f3qVI7A==",
    "Issuer": "https://localhost:7194/"
  }
}
```

## 🗃️ Database Setup

To set up the database for the Messi project:
1. Navigate to the server directory
2. Run the command: `dotnet ef database update`

This will create a database with the name specified in the `InitialCatalog` field of your server's `appsettings.json` file.

### ChatBot Setup (Required)

After setting up the database, you **must** run the ChatBot user creation script:

1. Open your SQL Server Management Studio (or any database client)
2. Connect to your database specified in `InitialCatalog`
3. Run the contents of `server/CreateBotUser.sql`

This creates a system user with ID 0 that the chatbot uses for sending messages. **The chatbot will not work without this step.**

For the 921 project, make sure to update the connection string in the server's `appsettings.json` with your own SQL Server instance name.

---

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
