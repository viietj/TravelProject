# TravelProject

## Overview
TravelProject is a full-stack travel booking web application that allows users to explore destinations, book tours, reserve hotels, and pay online. The system is developed in C# using ASP.NET Core MVC and Entity Framework Core, with SQL Server as the database and VNPay integrated as the online payment gateway.

## Features
- Browse travel destinations across Vietnam
- Tour listing and tour booking
- Hotel search and hotel booking
- Online payment via VNPay
- User registration, login, and profile management
- Favorites (save tours/hotels for later)
- Comments and comment likes on tours/destinations
- Food blog with regional Vietnamese cuisine
- Chatbot for basic user support
- Custom tour request submission
- Admin dashboard for managing tours, destinations, hotels, users, and bookings

## Technologies
- C# (.NET 10)
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- VNPay Payment Gateway
- ClosedXML (Excel export)
- Bootstrap, SCSS/CSS, JavaScript

## Installation
Clone the repository:
```bash
git clone https://github.com/viietj/TravelProject.git
cd TravelProject
```
Update the database connection string in `appsettings.json` to match your SQL Server instance, then apply the migrations:
```bash
dotnet ef database update
```

## Usage
Run the application:
```bash
dotnet run
```
Open your browser and navigate to the local URL shown in the terminal (e.g. `https://localhost:xxxx`).

## Architecture
The project follows the MVC pattern, separating Controllers, Models, and Views. Business logic such as payment processing is isolated in a dedicated Services layer (`VnpayService`), and database access is handled through EF Core with migration-based schema management.

## Future Improvements
- Unit and integration testing
- Tour recommendation based on user preferences
- Multi-language support (English/Vietnamese)
- Responsive redesign for mobile devices
- Deployment to a cloud hosting environment

## Author
Phan Viet Anh