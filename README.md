# Movie Night App - Backend

ASP.NET Core Web API for managing movie night schedules.

## Technologies
- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- CORS enabled for React frontend

## Setup
1. Update connection string in `appsettings.json`
2. Run migrations: `Update-Database`
3. Run the app: Press F5 in Visual Studio

## API Endpoints
- GET /api/MovieNights - Get all movie nights
- GET /api/MovieNights/upcoming - Get upcoming movies
- GET /api/MovieNights/past - Get past movies
- POST /api/MovieNights - Create new movie night
- PUT /api/MovieNights/{id} - Update movie night
- DELETE /api/MovieNights/{id} - Delete movie night
