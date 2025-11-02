## Environment Differences

When running either using visual studio code or by `dotnet run` - `ASPNETCORE_ENVIRONMENT=development`.

In this case there is more verbose logging: showing debugs.

A published release build i.e. from `dotnet publish ...` with `ASPNETCORE_ENVIRONMENT=production`. Will remove debug logs.
