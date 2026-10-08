# Tagalong - Ride Sharing Application

## Overview

Tagalong is a full-featured ride-sharing application built with ASP.NET Core MVC and SQL Server. The application allows users to offer rides, find available rides, book seats, communicate with ride partners, and manage their profiles. It includes role-based access control (Driver, Rider, Admin) and implements security best practices.

## Features

### User Features
- **User Registration & Login**: Secure authentication with ASP.NET Core Identity
- **Offer Rides**: Drivers can create ride offerings with vehicle details, route information, timing, and pricing
- **Find Rides**: Riders can search for rides using location-based matching with Haversine distance calculation
- **Booking System**: Riders can book seats on offered rides with confirmation workflow
- **In-App Chat**: Secure messaging between ride partners for coordination
- **Profile Management**: Users can view/edit profile, add vehicles (for drivers), and manage ride history
- **Rating & Feedback**: Post-ride ratings and comments for quality control
- **Complaint System**: Users can submit complaints which admins can resolve

### Admin Features
- **User Management**: View and manage all registered users
- **Ride Oversight**: Monitor all active and completed rides
- **Complaint Resolution**: Address user complaints and take appropriate actions
- **Analytics Dashboard**: View key metrics and statistics about the platform

## Technical Architecture

### Backend
- **Framework**: ASP.NET Core MVC 7.0
- **Database**: SQL Server with Entity Framework Core (Code-First approach)
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Services**: Dependency injection for business logic (distance calculation service)
- **Validation**: Server-side and client-side validation using Data Annotations

### Database Schema
The application uses the following core entities:
- **ApplicationUser**: Extended Identity user with profile information
- **Vehicle**: Belongs to a driver, used for rides
- **Ride**: Offered by drivers, linked to vehicle and driver
- **RideRequest**: Search criteria from riders
- **Booking**: Links riders to rides (many-to-many through booking entity)
- **Rating**: Post-ride feedback from riders to drivers
- **Complaint**: User-submitted issues for admin resolution
- **Message**: Chat messages between ride partners

### Key Services
- **DistanceService**: Implements Haversine formula for geographic distance calculation to match rides based on proximity

## Implementation Details

### Ride Matching Algorithm
The application uses the Haversine formula to calculate the distance between geographical coordinates. When a rider searches for rides:
1. All active rides are retrieved from the database
2. For each ride, the distance between the rider's pickup/drop locations and the ride's start/end locations is calculated
3. Rides within a configurable distance threshold are considered matches
4. Match quality is determined based on distance and timing compatibility

### Security Features
- **Authentication & Authorization**: ASP.NET Core Identity with role checks
- **Anti-Forgery Tokens**: Applied to all POST requests
- **Input Validation**: Server-side validation with Data Annotations
- **Parameterized Queries**: EF Core prevents SQL injection
- **Secure Passwords**: Hashed using industry-standard algorithms
- **Role-Based Access Control**: Different functionality available based on user role

### Client-Side
- **Bootstrap 5**: Responsive UI framework
- **jQuery Validation**: Client-side form validation
- **Razor Views**: Server-side rendering with clean separation of concerns

## Setup Instructions

### Prerequisites
- .NET 7.0 SDK
- SQL Server (or SQL Server Express)
- Git (for version control)

### Installation
1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd Tagalong
   ```

2. Configure the database connection:
   - Copy `.env.example` to `.env`
   - Update the connection string in `.env` with your SQL Server credentials
   - Alternatively, update the connection string in `appsettings.json`

3. Apply database migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Access the application at `https://localhost:5001` (or `http://localhost:5000`)

### Default Admin Account
After initial migration, you can create an admin user by registering through the website and then manually updating the user's role in the database, or by seeding the database with an initial admin user.

## Project Structure

```
/Tagalong
  /Controllers       - MVC Controllers handling HTTP requests
  /Data              - Database context and migrations
  /Models
    /Entities        - Database entity classes
    /ViewModels      - Models for passing data to views
  /Services          - Business logic services
  /Views             - Razor view templates
  /wwwroot           - Static files (CSS, JS, images)
  appsettings.json   - Application configuration
  .env               - Environment variables (not committed to git)
  Tagalong.csproj    - Project file
```

## API Endpoints (Key Routes)

### Authentication
- `GET /Account/Register` - Show registration form
- `POST /Account/Register` - Process registration
- `GET /Account/Login` - Show login form
- `POST /Account/Login` - Process login
- `POST /Account/Logout` - Logout user

### Rides
- `GET /Rides/Offer` - Show ride offering form (Drivers only)
- `POST /Rides/Offer` - Process ride offering
- `GET /Rides/Find` - Show ride search form
- `POST /Rides/Find` - Process ride search and show results
- `GET /Rides/Results` - Display search results
- `POST /Rides/RequestToJoin/{rideId}` - Book a seat on a ride

### Bookings
- `GET /Bookings/MyRides` - View user's rides (as driver/rider)
- `POST /Bookings/Cancel/{bookingId}` - Cancel a booking

### Chat
- `GET /Chat/Thread/{bookingId}` - View chat thread for a booking
- `POST /Chat/SendMessage` - Send a message in a chat thread

### Profile
- `GET /Profile/Index` - View user profile
- `GET /Profile/Edit` - Edit profile form
- `POST /Profile/Edit` - Process profile updates
- `GET /Profile/AddVehicle` - Add vehicle form (Drivers only)
- `POST /Profile/AddVehicle` - Process vehicle addition
- `GET /Profile/SubmitRating` - Submit rating form
- `POST /Profile/SubmitRating` - Process rating submission

### Admin
- `GET /Admin/Index` - Admin dashboard
- `GET /Admin/ManageUsers` - View and manage users
- `GET /Admin/ManageRides` - View and manage all rides
- `GET /Admin/ViewComplaints` - View complaints
- `POST /Admin/ResolveComplaint/{id}` - Resolve a complaint
- `GET /Admin/Analytics` - View analytics dashboard

## Deployment

### To GitHub
1. Initialize git repository if not already done:
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   ```

2. Create repository on GitHub and push:
   ```bash
   git remote add origin <repository-url>
   git push -u origin main
   ```

### To Hosting Providers
The application can be deployed to any ASP.NET Core compatible hosting provider including:
- Azure App Services
- AWS Elastic Beanstalk
- Docker containers
- IIS on Windows Server

### Environment Variables
Important configuration should be stored in environment variables:
- `DB_CONNECTION_STRING`: SQL Server connection string
- Other API keys or secrets as needed

## Future Enhancements

### Planned Features
- Real-time location tracking during rides
- Payment integration for fare collection
- Ride scheduling for future dates
- Enhanced matching algorithm with multiple waypoints
- Multi-language support
- Mobile-responsive improvements
- Push notifications for ride updates

### Technical Improvements
- Unit and integration testing
- Caching layer for improved performance
- Rate limiting for API protection
- Detailed logging and monitoring
- Docker containerization

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with ASP.NET Core MVC and Entity Framework Core
- Uses Bootstrap 5 for responsive design
- Implements Haversine formula for geographic calculations
- Authentication provided by ASP.NET Core Identity