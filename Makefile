BUILD_DIR := ./build
LIVE_BUILD_DIR := $(BUILD_DIR)/live
IMAGE_NAME := raspbian-custom.img
DEBIAN_MIRROR := http://raspbian.raspberrypi.org/raspbian
RELEASE := bookworm
ARCH := armhf
PACKAGE_FILES_DIR := ./packages
LIVE_BUILD_CONFIG := config
TEMP_DIR := $(BUILD_DIR)/temp

$(BUILD_DIR):
	mkdir -p $(BUILD_DIR)

$(LIVE_BUILD_DIR):
	mkdir -p $(LIVE_BUILD_DIR)

$(TEMP_DIR):
	mkdir -p $(TEMP_DIR)

.PHONY: install-tools
install-tools:
	sudo apt-get update
	sudo apt-get install -y live-build debootstrap qemu-user-static

.PHONY: install-tools-arch
install-tools-arch:
	sudo pacman -S debootstrap qemu-user-static

.PHONY: bootstrap
bootstrap: $(LIVE_BUILD_DIR)
	sudo debootstrap --no-check-gpg --arch=$(ARCH) --foreign  $(RELEASE) $(LIVE_BUILD_DIR) $(DEBIAN_MIRROR)
	sudo cp /usr/bin/qemu-arm-static $(LIVE_BUILD_DIR)/usr/bin/
	sudo chroot $(LIVE_BUILD_DIR) /bin/bash -c "/debootstrap/debootstrap --second-stage"

.PHONY: copy-packages
copy-packages: $(LIVE_BUILD_DIR)
# sudo cp $(PACKAGE_FILES_DIR)/*.deb $(BUILD_DIR)/var/cache/apt/archives/

.PHONY: install-packages
install-packages: copy-packages
	sudo chroot $(LIVE_BUILD_DIR) /bin/bash -c "dpkg -i /var/cache/apt/archives/*.deb || apt-get install -f -y"

.PHONY: configure
configure:

.PHONY: build-image
build-image: configure $(TEMP_DIR)
#	dd if=/dev/zero of=$(BUILD_DIR)/$(IMAGE_NAME) bs=256k count=32768
	sudo umount $(TEMP_DIR)
	truncate -s 8G $(BUILD_DIR)/$(IMAGE_NAME)
	sudo mkfs.ext4 $(BUILD_DIR)/$(IMAGE_NAME)
	sudo mount -o loop $(BUILD_DIR)/$(IMAGE_NAME) $(TEMP_DIR)
	sudo cp -r $(LIVE_BUILD_DIR)/* $(TEMP_DIR)
	sudo umount $(TEMP_DIR)

all:

.PHONY: all
all: bootstrap install-packages configure build-image restore build		

.PHONY: build
build-dotnet:
	dotnet build --no-restore

.PHONY: restore
restore:		
	dotnet restore -f

add-migration:
	dotnet ef migrations add $(migrationName) --output-dir SystemMigrations --startup-project src/controller/backend/Backend.csproj --project src/controller/data/Data.csproj

clean:
	rm -rf build/

.PHONY: all clean