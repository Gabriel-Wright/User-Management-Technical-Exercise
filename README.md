## Environment Differences

When running locally (either by visual studio code or by `dotnet run`), `ASPNETCORE_ENVIRONMENT=development`.
In this case console output includes verbose logs, while log files capture entries up to the Debug level.

When running published release build e.g. from `dotnet publish ...`, `ASPNETCORE_ENVIRONMENT=production`.
Only Information level logs and above are written to both the console and log files.

## Database Setup

Our Development Environment uses an in memory DB, whereas the Production Environment DB details must be passed through environment variables.

There are dummy Connection Strings passed in `appsettings.Production.json` but these are just templates:

```
  "ConnectionStrings": {
    "DefaultUMSConnection": "Server=prod-db-server;Database=UserManagement;User=usr;Password=secret;"
  },
```

### Development

The application uses an in-memory database - no setup required.

### Production (MySQL)

Below are the steps to complete if you wish to test a Production Deployment of this software i.e. use a MySQL DB instead of an in memory DB.

#### 1. Set Default_UMS_Connection env variable

```bash
   export DEFAULT_UMS_CONNECTION="Server=localhost;Database=UserManagement;Port=3306;User=user;Password=your_password;"
```

#### 2. Set Migration_UMS_Connection env variable

```bash
   export MIGRATION_UMS_CONNECTION="Server=localhost;Database=UserManagementMigration;Port=3306;User=user;Password=your_password;"
```

Why is there a separate migration connection?

-   Safe database used for generating migrations.
-   Avoids connecting directly to the production database while generating or testing migrations.
-   Intended to generate SQL scripts from this connection and safely apply them to your production database.

#### 3. Creating Database Schema

When first creating schema. Generate .cs files under `Migrations` folder.

```
cd UserManagement.Data
dotnet ef migrations add InitialCreate --startup-project ../UserManagement.Web
```

#### 4. Convert .cs Migration files to a .sql script

```
dotnet ef migrations script --startup-project ../UserManagement.Web --output ../InitialMigration.sql
```

#### 5. Run this script with your MySQL Server, Schema should be generated:

For example,

```
mysql> SOURCE InitialCreate.sql;
Query OK, 0 rows affected (0.010 sec)

Query OK, 0 rows affected (0.000 sec)

Query OK, 1 row affected (0.001 sec)

Query OK, 0 rows affected, 1 warning (0.004 sec)

Query OK, 11 rows affected (0.003 sec)
Records: 11  Duplicates: 0  Warnings: 0

Query OK, 1 row affected (0.017 sec)

Query OK, 0 rows affected (0.000 sec)
```

#### 6. If there are future schema changes, can be run again to update schema

```
cd UserManagement.data
dotnet ef migrations script --idempotent -o UpdateDatabase.sql --startup-project ../UserManagement.Web
```

## Packages added

---

-   Serilog.AspNetCore — Structured logging support
-   Serilog.Sinks.File — File-based log output
-   Serilog.Sinks.Console — Console log output
