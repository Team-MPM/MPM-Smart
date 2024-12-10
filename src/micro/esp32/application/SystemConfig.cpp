#include "SystemConfig.hpp"

#include <cstdio>
#include <esp_log.h>
#include <File.hpp>

#define TAG "SystemConfig"

namespace Mpm {
    SystemConfig::SystemConfig(const char *deviceName, const char *ssid,
        const char *password, const char *controllerAddress, const char *token)
            : deviceName(deviceName), ssid(ssid), password(password),
                controllerAddress(controllerAddress), token(token){}

    SystemConfig SystemConfig::FromFile(const char *path) {
        char buffer[512];
        File::Read(path, buffer, sizeof(buffer));
        ESP_LOGI(TAG, "Config: %s", buffer);
        return SystemConfig(buffer, buffer, buffer, buffer, buffer);
    }

    void SystemConfig::SaveToFile(const char *path) const {
        char buffer[512];
        snprintf(buffer, sizeof(buffer), "%s\n%s\n%s\n%s\n%s\n",
            deviceName, ssid, password, controllerAddress, token);
        File::Write(path, buffer);
    }
}
