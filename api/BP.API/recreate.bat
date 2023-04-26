@echo off

echo Dropping database...
dotnet ef database drop -f --startup-project BP.API.csproj --project ../BP.Data/BP.Data.csproj

echo Removing migrations folder...
if exist "../BP.Data/Migrations" rmdir /s /q ../BP.Data/Migrations

echo Creating new migration...
dotnet ef migrations add NewMigration --startup-project BP.API.csproj --project ../BP.Data/BP.Data.csproj --no-build
echo Updating database...
dotnet ef database update --startup-project BP.API.csproj --project ../BP.Data/BP.Data.csproj --no-build

echo Done.