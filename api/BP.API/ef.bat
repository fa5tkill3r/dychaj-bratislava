@echo off
dotnet ef --startup-project Dashboard.API.csproj --project ../Dashboard.API.Data/Dashboard.API.Data.csproj %*