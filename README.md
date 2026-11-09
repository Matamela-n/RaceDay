# RaceDay
This a South African road event management system
# RaceDay Documentation

# RaceDay🏃🏾🚶🏽‍♀️🚴🏽‍♀️

## System Description

RaceDay is a web-based event management system designed for the South African road running, walking and cycling community.

The system allows Event Organisers to create and manage events, event categories and participant results. Participants can browse available events, enrol in event categories and view their performance results.

The purpose of RaceDay is to provide a more organised digital system for managing sporting events instead of relying on paper-based registration, spreadsheets and disconnected communication methods.

## User Roles

### Event Organiser

The Event Organiser is responsible for managing events on the RaceDay system.

An Event Organiser can:

- Create events
- Update event information
- Delete events
- Create and manage event categories
- Manage participant enrolments
- Record and update participant results

### Participant

The Participant is a user who enters and takes part in RaceDay events.

A Participant can:

- Register and log in to the system
- View and update their profile
- Browse upcoming events
- View event categories
- Enrol in event categories
- View their enrolments
- View their race results and performance history

## CI/CD
RaceDay uses GitHub Actions for Continuous Integration and Continuous Deployment (CI/CD).

The workflow automatically validates the structure of the repository whenever changes are pushed to GitHub. It checks that the required `/docs` folder and project documentation are included, such as the ERD, API Endpoint Plan and SQL database script.

### Successful GitHub Actions Build

The screenshot below shows that the GitHub Actions workflow completed successfully.

## YouTube Demonstration

An unlisted YouTube video demonstrating the RaceDay planning documents, ERD, API Endpoint Plan and SQL database implementation can be viewed below: https://youtu.be/2rsqBKUHUBQ

## GitHub link
https://github.com/Matamela-n/RaceDay


# RaceDay API — Part 2😝😝

## 1. Project Overview

RaceDay is an ASP.NET Core Web API developed using C# and .NET 8. The system supports race event management, participant enrolment, category management, user authentication, and race results management.

The API uses Entity Framework Core to interact with a SQL Server database and provides role-based access control for Organisers and Participants.

## 2. Technologies Used

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server / LocalDB
- Cookie-based authentication
- BCrypt password hashing
- xUnit unit testing
- Git and GitHub
- GitHub Actions for continuous integration

## 3. Main Features

### Authentication and User Management
- Register Participants and Organisers.
- Log in using registered credentials.
- Hash passwords before storing them.
- Restrict protected operations according to user roles.
- View and update the logged-in user's profile.

### Event and Category Management
- Create, retrieve, update, and delete race events.
- Manage event categories.
- Restrict event management to authorised Organisers.
- Check event ownership before allowing protected operations.

### Participant Enrolment
- Allow Participants to enrol in categories.
- View personal enrolments.
- Update or cancel enrolments.
- Validate category existence and enrolment ownership.

### Race Results
- Record and update race results.
- Retrieve results for events and Participants.
- Restrict result management to authorised Organisers.

## 4. Project Structure

```text
RaceDay/
├── Controllers/
├── Data/
├── Migrations/
├── Models/
├── Properties/
├── docs/
├── .github/
│   └── workflows/
│       └── part2-ci.yml
├── RACEDAY_API_PROJECT.Tests/
│   ├── AuthControllerTests.cs
│   ├── EnrolmentControllerTests.cs
│   ├── EventControllerTests.cs
│   ├── UnitTest1.cs
│   └── RACEDAY_API_PROJECT.Tests.csproj
├── Program.cs
├── RACEDAY_API_PROJECT.csproj
└── README.md
```

## 5. Requirements

Before running the project, install:

- .NET 8 SDK
- Visual Studio 2022 or another compatible .NET IDE
- SQL Server or SQL Server LocalDB

## 6. Database Configuration

The API uses Entity Framework Core and SQL Server.

1. Configure the connection string in `appsettings.json` for your local SQL Server instance.
2. Ensure the database configuration matches your development environment.
3. Apply the existing Entity Framework Core migrations.

Example commands, run from the API project directory:

```bash
dotnet restore
dotnet ef database update
```

If the Entity Framework command is unavailable, install the EF Core command-line tools and ensure the required provider packages are installed.

**Important:** Do not commit production database passwords or other secrets to GitHub.

## 7. Running the API

Clone the repository:

```bash
git clone https://github.com/Matamela-n/RaceDay.git
cd RaceDay
```

Restore dependencies and build the API:

```bash
dotnet restore RACEDAY_API_PROJECT.csproj
dotnet build RACEDAY_API_PROJECT.csproj
```

Run the API:

```bash
dotnet run --project RACEDAY_API_PROJECT.csproj
```

Use the URL displayed in the terminal to access the running API. The configured launch profile determines the actual URL and port.

## 8. API Endpoints

The following endpoints are available in the API. Protected endpoints require an authenticated user with the appropriate role.

| Controller | Endpoint | Purpose |
|---|---|---|
| Auth | `/api/auth/register` | Register a user |
| Auth | `/api/auth/login` | Log in |
| Auth | `/api/auth/logout` | Log out |
| User Profile | `/api/users/me` | View or update the current user's profile |
| Event | `/api/Event` | Create an event |
| Event | `/api/Event/{id}` | Retrieve, update, or delete an event |
| Event | `/api/Event/{id}/enrolments` | View enrolments for an event |
| Categories | `/api/Categories` | Retrieve categories or manage categories |
| Enrolment | `/api/Enrolment` | Create an enrolment |
| Enrolment | `/api/Enrolment/mine` | View personal enrolments |
| Enrolment | `/api/Enrolment/{id}` | Retrieve, update, or delete an enrolment |
| Results | `/api/results/mine` | View personal results |
| Results | `/api/events/{id}/results` | Retrieve results for an event |
| Results | `/api/results` | Create a result |
| Results | `/api/results/{id}` | Retrieve, update, or delete a result |

The exact HTTP methods and authorisation requirements are defined in the respective controllers.

## 9. Authentication and Security

The API uses cookie-based authentication and role-based authorisation.

- Organiser-only operations require the Organiser role.
- Participant-only operations require the Participant role.
- Passwords are hashed using BCrypt.
- Ownership checks restrict access to another user's protected resources.
- Invalid credentials and unauthorised requests are handled by the API.

For authenticated requests, retain the authentication cookie when using a REST client.

## 10. Unit Testing

The project includes an xUnit test project named `RACEDAY_API_PROJECT.Tests`.

The tests cover database context operations, registration validation, login failures, event management, and enrolment operations.

Run the unit tests from the repository root:

```bash
dotnet test RACEDAY_API_PROJECT.Tests/RACEDAY_API_PROJECT.Tests.csproj --configuration Release
```

The tests use Entity Framework Core's InMemory database for isolated test data where configured.

## 11. Continuous Integration

GitHub Actions is configured in `.github/workflows/part2-ci.yml`.

The workflow runs when changes are pushed to `main` or a pull request targets `main`. It:

1. Checks out the repository.
2. Sets up the .NET 8 SDK.
3. Builds the API in Release configuration.
4. Runs the unit test project.

A successful workflow run confirms that the configured build and test steps have completed successfully.

## 12. Supporting Documentation

Supporting project documentation is available in the `docs/` directory. Refer to the repository for the available database scripts, API planning material, and other project resources.

## 13. Repository

GitHub: https://github.com/Matamela-n/RaceDay

## 14. Youtube link
https://youtu.be/r4v1zlMEujg
# RaceDay Web Application — Part 3 🏃🏾🚶🏽‍♀️🚴🏽‍♀️

## 1. Project Overview

Part 3 of RaceDay extends the existing RaceDay API by introducing an ASP.NET Core MVC web application.

The web application provides a user-friendly interface through which Event Organisers and Participants can interact with the RaceDay system. It communicates with the backend API to retrieve and manage event information, categories, enrolments, profiles and race results.

The purpose of this part is to integrate the API developed in Part 2 with a functional web application.

## 2. Technologies Used

- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- SQL Server
- HTML and CSS
- Razor Views
- JavaScript
- HTTP Client (`IHttpClientFactory`)
- Cookie-based authentication
- Role-based authorisation
- Git and GitHub

## 3. System Architecture

RaceDay uses a two-project architecture:

**RaceDay API**
- Handles backend operations and business logic.
- Communicates with the SQL Server database.
- Provides API endpoints for authentication, events, categories, enrolments and results.
- Enforces authentication, authorisation and ownership checks.

**RaceDay Web**
- Provides the user interface through MVC controllers and Razor views.
- Sends HTTP requests to the RaceDay API.
- Displays information returned by the API.
- Provides separate functionality for Organisers and Participants.

The web application communicates with the API using `IHttpClientFactory`.

## 4. Main Features

### Authentication and Registration
- Register as an Event Organiser or Participant.
- Log in using registered credentials.
- Communicate with the API for authentication.
- Maintain authenticated user sessions.
- Log out of the application.

### Event Organiser Dashboard
- Access the Organiser dashboard.
- Manage race events.
- Manage event categories.
- View participant enrolments.
- Manage race results.

### Participant Dashboard
- Access the Participant dashboard.
- Browse available events.
- View event categories.
- Manage enrolments.
- View race results and performance information.

### User Profile Management
- View and update profile information.
- Upload a profile image.
- Display profile information within the web application.

## 5. Project Structure

The repository contains two main application projects:

```text
RaceDay/
├── Controllers/
├── Data/
├── Migrations/
├── Models/
├── RACEDAY_API_PROJECT.csproj
│
├── RaceDay_Web/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── RaceDay_Web.csproj
│
├── RACEDAY_API_PROJECT.Tests/
├── docs/
├── .github/
│   └── workflows/
└── README.md
```

## 6. Configuration

Before running the web application:

1. Ensure the RaceDay API is configured and can connect to the database.
2. Open the web application's `appsettings.json`.
3. Configure the API base URL to match the address where the API is running.
4. Ensure the API and web application are using the correct environment settings.

Do not commit database passwords, API secrets or other sensitive configuration information.

## 7. Running the Application

Clone the repository:

```bash
git clone https://github.com/Matamela-n/RaceDay.git
cd RaceDay
```

Restore and build the API:

```bash
dotnet restore RACEDAY_API_PROJECT.csproj
dotnet build RACEDAY_API_PROJECT.csproj
```

Run the API:

```bash
dotnet run --project RACEDAY_API_PROJECT.csproj
```

In another terminal, restore and run the MVC web application:

```bash
dotnet restore RaceDay_Web/RaceDay_Web.csproj
dotnet run --project RaceDay_Web/RaceDay_Web.csproj
```

Open the web application using the local URL displayed in the terminal.

## 8. Security

RaceDay uses authentication and role-based authorisation to separate Organiser and Participant functionality.

- Organisers access authorised event management functionality.
- Participants access participant-related functionality.
- The web application communicates with the API rather than directly managing API database operations.
- The API remains responsible for validating protected requests and resource ownership.

## 9. Integration

Part 3 integrates the MVC web application with the existing RaceDay API.

The integration allows users to interact with backend functionality through a web interface while keeping the API and database operations separate from the presentation layer.

## 10. Repository

**GitHub:** https://github.com/Matamela-n/RaceDay

**Part 3 Demonstration Youtube link:** https://youtu.be/UpNM00x_6JU
