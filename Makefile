BUILD_DIR := ./build
IMAGE_NAME := raspbian-custom.img
DEBIAN_MIRROR := http://raspbian.raspberrypi.org/raspbian
RELEASE := buster
ARCH := armhf
PACKAGE_FILES_DIR := ./packages
LIVE_BUILD_CONFIG := config
IMAGE_SIZE := 4G

$(BUILD_DIR):
	mkdir -p $(BUILD_DIR)

.PHONY: install-tools
install-tools:
	sudo apt-get update
	sudo apt-get install -y live-build debootstrap qemu-user-static

.PHONY: install-tools-arch
install-tools-arch:
	sudo pacman -S debootstrap qemu-user-static

.PHONY: bootstrap
bootstrap: $(BUILD_DIR)
	sudo debootstrap --arch=$(ARCH) --foreign $(RELEASE) $(BUILD_DIR) $(DEBIAN_MIRROR)
	sudo cp /usr/bin/qemu-arm-static $(BUILD_DIR)/usr/bin/
	sudo chroot $(BUILD_DIR) /bin/bash -c "/debootstrap/debootstrap --second-stage"

.PHONY: copy-packages
copy-packages: $(BUILD_DIR)
# sudo cp $(PACKAGE_FILES_DIR)/*.deb $(BUILD_DIR)/var/cache/apt/archives/

.PHONY: install-packages
install-packages: copy-packages
	sudo chroot $(BUILD_DIR) /bin/bash -c "dpkg -i /var/cache/apt/archives/*.deb || apt-get install -f -y"

.PHONY: configure
configure:

.PHONY: build-image
build-image: bootstrap install-packages configure
	dd if=/dev/zero of=$(IMAGE_NAME) bs=1M count=$(IMAGE_SIZE)
	mkfs.ext4 $(IMAGE_NAME)
	sudo mount -o loop $(IMAGE_NAME) /mnt
	sudo cp -r $(BUILD_DIR)/* /mnt
	sudo umount /mnt

.PHONY: clean
clean:
	rm -rf $(BUILD_DIR)
	rm -f $(IMAGE_NAME)
	dotnet clean

.PHONY: all
all: bootstrap install-packages configure build-image	build		

.PHONY: build
build:
	dotnet build --no-restore

.PHONY: restore
restore:
	dotnet restore -f

.PHONY: add-migration
add-migration:
	dotnet ef migrations add $(migrationName) --output-dir SystemMigrations --startup-project src/controller/backend/Backend.csproj --project src/controller/data/Data.csproj
