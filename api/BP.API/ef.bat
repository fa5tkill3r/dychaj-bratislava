@echo off
dotnet ef --startup-project BP.API.csproj --project ../BP.Data/BP.Data.csproj %* --context BpContext