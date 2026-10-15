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

