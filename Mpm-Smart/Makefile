
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

info: FORCE
	@echo "System Info:"
	@echo "Platform: $(PLATFORM)"
	@echo "Architecture: $(ARCH)"

proto: FORCE
	@echo "Building Proto..."
	dotnet build --no-incremental Proto/Proto.csproj

build-arm:
	@echo "Building ARM Container..."
	$(WSL) docker run --rm --privileged multiarch/qemu-user-static --reset -p yes
	$(WSL) docker buildx build --platform linux/arm64 --no-cache -t mpm-arm-build -f build/Dockerfile.arm .
	$(WSL) docker run --rm mpm-arm-build

FORCE: ;