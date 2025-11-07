# User Management System - Technical Exercise

This is my solution to the UMS Technical Exercise.
I implemented all the standard and advanced features to the best of my ability.
I altered the architecture project (working on the expert tasks), introducing a separate UI layer made in Blazor.

- UserManagement.Data: Data access layer with EF Core
- UserManagement.Services: Business logic and domain services
- UserManagement.Web: Controller layer hosting the API
- UserManagement.Data: Separate UI front end that accesses the API

## Architectural Changes

Overall my intention with this project was to ensure there was good separation between layers.
I ensure distinction between the Data Layer and Domain Service layer for business logic, and carry this idea up through each stacks.
The reason for this approach is:

-   It makes it easier to scale each layer independently.
-   I get extra flexibility, e.g. I could swap Blazor for React/Angular without touching the API
-   Extra security, I can Validate inputs and requests at each layer.

### UI Layer

-   The application now separates UI and Web API responsibilities.
-   The Web API (UserManagement.Web) hosts controllers and receives Dtos from the API layer.
-   The UI (UserManagement.UI) is implemented as a Blazor WebAssembly client that communicates with the Web API over HTTP.

### Dev Environment vs Production Environment

This repo can be run in a Development Environment or production Environment, this is determined by the
`ASPNETCORE_ENVIRONMENT` variable.

### MySQL DB

Our Development Environment uses an in memory DB, whereas the Production Environment is expected to use a MySQL DB. The details to connect to this MySQL DB must be passed via environment variables.

### EF Migration

EF Migration is configured for this project - meaning we can generate SQL scripts to create or update database schemas... in line with the Entities present in our `/Data` Project.

### Additional Packages/Tools

-   BlazorWebAssembly
-   Serilog Logging

## How to Setup

Begin by cloning the repository

```
git clone https://github.com/Gabriel-Wright/User-Management-Technical-Exercise
```

## Developer Environment -- FAST SETUP

For the developer environment, simply open two terminals.

In terminal one, navigate from the root of the project to `UserManagement.Web` for the API app.
This will be hosted on `https://localhost:7084` by default.
**NOTE:** Swagger is available in debug mode under `swagger/index.html`

```
cd UserManagement.Web
dotnet run
```

In terminal two, navigate from the root of the project to `UserManagement.UI` for the Blazor front end.
This will be hosted on `http://localhost:5086` by default.

```
cd UserManagement.UI
dotnet run
```

Working in the Developer environment - these two different projects should connect.

## Production Environment -- LONGER SETUP

The Production Environment uses a MySQL DB layer. Setup for production is more involved,
there are 3 main steps.

### 1. Creating UMS Schema for MySQL Production DB.

#### 1.1. Define the Connection String we will be using

Set Default_UMS_Connection env variable.

```bash
   export DEFAULT_UMS_CONNECTION="Server=localhost;Database=UserManagement;Port=3306;User=user;Password=your_password;"
```

#### 1.2. Define the Migration Connection String used for creating Schema

```bash
   export MIGRATION_UMS_CONNECTION="Server=localhost;Database=UserManagementMigration;Port=3306;User=user;Password=your_password;"
```

Why is there a separate migration connection?

-   Can be used for testing.
-   Safe database used for generating migrations.
-   Avoids connecting directly to the production database while generating or testing migrations.
-   Intended to generate SQL scripts from this connection and safely apply them to your production database.

#### 1.3. Creating Database Schema using EF

EF can generate our DB Schema based on Entities defined in our `Data` Layer.
The `dotnet ef migrations` commands below, generate the InitialCreate.sql script needed to create our schema.

```
cd UserManagement.Data
dotnet ef migrations add InitialCreate --startup-project ../UserManagement.Web
dotnet ef migrations script --startup-project ../UserManagement.Web --output ../InitialCreate.sql
```

#### 1.4. Run this script in the relevant Database of your MySQL Server, Schema should be generated:

For example,

```
mysql> USE USER_MANAGEMENT;
Database changed

mysql> SOURCE InitialCreate.sql;
Query OK, 0 rows affected (0.010 sec)
... etc ...
```

#### 1.5. (Optional) If there are future schema changes, can be run again to update schema

```
cd UserManagement.data
dotnet ef migrations script --idempotent -o UpdateDatabase.sql --startup-project ../UserManagement.Web
```

### 2. Assign Environment Variables Values and Config File values to allow our two layers to connect

#### 2.1 Set UI_URL Environment variable

The environment variables used by the API layer to allow incoming connections from the UI layer are:

Development: `UI_UMS_URL_DEV`  
Production: `UI_UMS_URL_PROD`

set `UI_UMS_URL_PROD` e.g.

```
export UI_UMS_URL_PROD="http://localhost:5085"
```

#### 2.2 SET ApiBaseUrl

Within `appsettings.Production.json` in `UserManagement.UI`, set `ApiBaseUrl` equal to the expected
you will be hosting the api app on + `/api/`

e.g.

```
    "ApiBaseUrl": "https://localhost:7084/api/"
```

### 3. Publishing the Apps and run them with the appropriate ports

#### 3.1 Publishing API Layer

Navigate to the root of this repo, and publish the API (Web) project as follows:

```
dotnet publish UserManagement.Web/UserManagement.Web.csproj -c Release -o ./publish
```

#### 3.2 Running API Layer on appropriate URL

Navigate to the generated `.dll` files under `/publish`.
Run `UserManagement.Web.dll` with `dotnet`, using the same URL as we used for `UI_API_PROD`.

```
dotnet UserManagement.Web.dll --urls {INSERT MATCHING URL TO UI_UMS_API_PROD}
```

If there are issues at this point, they will most likely be related to connecting to the Production DB.
e.g.

```
dotnet UserManagement.Web.dll
[04:22:56 INF] Starting web host...
[04:22:56 INF] Running in Production environment.
[04:22:56 FTL] Cannot connect to production database.
[04:22:56 FTL] Database connection validation failed. Application will stop.
```

If you encounter this, I would recommend reviewing your connection string environment variable `DEFAULT_UMS_CONNECTION`.

#### 3.3 Publishing UI Layer

Navigate to the root of this repo again, and publish the UI Project as follows:

```
dotnet publish UserManagement.UI/UserManagement.UI.csproj -c Release -o ./publish
```

#### 3.4 Run UI on appropriate URL

Static files can be found under `/publish/wwwroot`.
Hosted on the same URL as we used for `API_UMS_URL_PROD`.

If testing locally (i.e. local host)

```
cd /publish/wwwroot
dotnet serve --p {INSERT MATCHING PORT TO THAT USED IN UI_UMS_API_PROD}
```

### Success!

Hopefully your two layers should be connected.
I have left in the test data that originally came with this solution.

### Thank you

Thank you for reviewing my project. I enjoyed working on it :) . I look forward to any feedback I receive.

-   Gabriel
