all: proto

clean: FORCE

restore: FORCE
	@echo "Restoring..."
	dotnet restore -f

proto: FORCE
	@echo "Building Proto..."
	dotnet build --no-incremental Proto/Proto.csproj --no-restore

MIGRATION_NAME = $(or $(name), Initial)
DB_CONTEXT = $(or $(context), PrimaryDbContext)
OUTPUT_DIR = Migrations/$(DB_CONTEXT)Migrations
STARTUP_PROJECT = source/Services/DbManager/DbManager.csproj

add-migration: FORCE
	@echo "Adding migration..."
	dotnet ef migrations add $(MIGRATION_NAME) --context $(DB_CONTEXT) --output-dir $(OUTPUT_DIR) --startup-project $(STARTUP_PROJECT)


FORCE: ;