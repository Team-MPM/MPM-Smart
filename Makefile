
all:

restore:
	dotnet restore -f

add-migration:
	dotnet ef migrations add $(migrationName) --output-dir SystemMigrations --startup-project src/controller/backend/Backend.csproj --project src/controller/data/Data.csproj

clean:
	rm -rf build/

.PHONY: all clean