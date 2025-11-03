## Environment Differences

When running locally (either by visual studio code or by `dotnet run`), `ASPNETCORE_ENVIRONMENT=development`.
In this case console output includes verbose logs, while log files capture entries up to the Debug level.

When running published release build e.g. from `dotnet publish ...`, `ASPNETCORE_ENVIRONMENT=production`.
Only Information level logs and above are written to both the console and log files.

### Packages added

---

-   Serilog.AspNetCore — Structured logging support
-   Serilog.Sinks.File — File-based log output
-   Serilog.Sinks.Console — Console log output
