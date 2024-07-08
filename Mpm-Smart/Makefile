
ifeq ($(OS), Windows_NT)
    PLATFORM = "windows"
    ARCH := $(shell echo %PROCESSOR_ARCHITECTURE%)
    WSL = "wsl"
else
    UNAME := $(shell uname -s)
    WSL = ""
    ifeq ($(UNAME), Linux)
        PLATFORM = "linux"
        ARCH := $(shell uname -m)
        ifeq ($(ARCH), x86_64)
			ARCH = x86
		endif
    endif
    ifeq ($(UNAME), Darwin)
        PLATFORM = mac
        ARCH := $(shell uname -m)
        ifeq ($(ARCH), x86_64)
			ARCH = x86
		endif
    endif
endif

all: info proto

clean: FORCE
	@echo "Cleaning..."
	$(WSL) rm -rf build/Packages

info: FORCE
	@echo "System Info:"
	@echo "Platform: $(PLATFORM)"
	@echo "Architecture: $(ARCH)"

proto: FORCE
	@echo "Building Proto..."
	dotnet build --no-incremental Proto/Proto.csproj

build-arm: FORCE
	@echo "Building ARM Container..."
	$(WSL) docker run --rm --privileged multiarch/qemu-user-static --reset -p yes
	$(WSL) docker buildx build --platform linux/arm64 --no-cache -t mpm-arm-build -f build/Dockerfile.arm .
	$(WSL) docker run --rm mpm-arm-build

VirtualHost-Package: FORCE
	@echo "Building VirtualHost Package..."
	dotnet publish --self-contained -c Release -o build/Packages/VirtualHost VirtualHost/VirtualHost.csproj
	dotnet publish --self-contained -c Release -o build/Packages/VirtualHost/Modules/CoreModule CoreModule/CoreModule.csproj
	dotnet publish -c Release LedPlugin/LedPlugin.csproj
	$(WSL) mkdir -p build/Packages/VirtualHost/Plugins
	$(WSL) cp LedPlugin/bin/Release/net8.0/publish/LedPlugin.dll build/Packages/VirtualHost/Plugins/LedPlugin.dll

# Plugins

PROJECT_NAME := $(or $(project), VirtualHost)
DEV_DIR := $(PROJECT_NAME)/Plugins
UPDATE_SUBDIR := Update


SRC_FILE := LedPlugin/bin/Release/net8.0/publish/LedPlugin.dll
Plugin-Led: FORCE
	@echo "Building Led Plugin..."
	dotnet publish -c Release LedPlugin/LedPlugin.csproj
	@$(WSL) if cp $(SRC_FILE) $(DEV_DIR); then \
		echo "File copied successfully to $(DEV_DIR)"; \
	else \
		echo "Access denied. Checking for lock and moving to $(DEV_DIR)/$(UPDATE_SUBDIR)"; \
		mkdir -p $(DEV_DIR)/$(UPDATE_SUBDIR); \
		cp $(SRC_FILE) $(DEV_DIR)/$(UPDATE_SUBDIR); \
		echo "File moved to $(DEV_DIR)/$(UPDATE_SUBDIR)"; \
	fi

FORCE: ;