# MarketMinds.Web - ASP.NET Core MVC with React and Tailwind

This project is the web version of the MarketMinds application, built with ASP.NET Core MVC, React, and Tailwind CSS.

## Prerequisites

- .NET 8.0 SDK
- Node.js (v18+) and npm
- Visual Studio 2022 or Visual Studio Code

## Project Structure

- **ASP.NET Core MVC**: Provides the server-side functionality and basic views
- **React**: Provides rich client-side UI components
- **Tailwind CSS**: For styling both MVC views and React components

## Setup Instructions

### Setting Up the .NET Project

1. Clone the repository
2. Open the solution in Visual Studio or VS Code
3. Restore the .NET packages:
   ```
   dotnet restore
   ```

### Setting Up the React App

1. Navigate to the ClientApp directory:
   ```
   cd MarketMinds.Web/ClientApp
   ```

2. Install the npm packages:
   ```
   npm install
   ```

3. Build the React app:
   ```
   npm run build
   ```

### Running the Application

#### Development Mode

From the MarketMinds.Web directory, run:
```
dotnet run
```

This will start the ASP.NET Core application. By default, it will be available at:
- https://localhost:5001
- http://localhost:5000

#### Using Visual Studio

Simply press F5 to start debugging the application.

## Authentication

The application uses JWT for authentication. To access protected routes, users must first authenticate.

## API Integration

This web application connects to the same backend API as the WPF application, reusing the business logic and data access layers through the MarketMinds.Shared project.

## Migration Notes

This web project implements Option 2 from the migration plan, which involves:

- Creating class libraries for shared code
- Referencing them from both the desktop and web applications
- Using dependency injection for all services 