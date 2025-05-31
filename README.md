# UBB-SE-2025-Marketplace

## 🌐 Web
![CleanShot 2025-05-31 at 8  12 03@2x-min](https://github.com/user-attachments/assets/aab603e9-444c-4354-b9b2-5d077ee313c1)


## 💻 Desktop
![CleanShot 2025-05-31 at 8  35 12@2x-min](https://github.com/user-attachments/assets/68a2d733-48ce-489b-a8cd-679d4161178c)


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
  - Shopping basket
  - Bidding system
  - Tracking orders
  - Notifications
  - Generating Contracts


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
