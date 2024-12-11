#include "SystemConfig.hpp"

#include <cstdio>
#include <cstring>
#include <esp_log.h>
#include <File.hpp>

#define TAG "SystemConfig"

namespace Mpm {
    SystemConfig::SystemConfig(const char *deviceName, const char *ssid,
        const char *password, const char *controllerAddress, const char *token,
        const char *identity, const char *username)
            : deviceName(deviceName), ssid(ssid), identity(identity), username(username),
                password(password), controllerAddress(controllerAddress), token(token){}

    static char configBuffer[512];
    SystemConfig SystemConfig::FromFile(const char *path) {
        File::Read(path, configBuffer, sizeof(configBuffer));

        char* deviceName = strtok(configBuffer, "\n");
        char* ssid = strtok(nullptr, "\n");
        char* password = strtok(nullptr, "\n");
        char* controllerAddress = strtok(nullptr, "\n");
        char* token = strtok(nullptr, "\n");
        char* identity = strtok(nullptr, "\n");
        char* username = strtok(nullptr, "\n");

        return {
            deviceName,
            ssid,
            password,
            controllerAddress,
            token,
            identity,
            username,
        };
    }

    void SystemConfig::SaveToFile(const char *path) const {
        char buffer[512];
        snprintf(buffer, sizeof(buffer), "%s\n%s\n%s\n%s\n%s\n%s\n%s\n",
            deviceName, ssid, password, controllerAddress, token, identity, username);
        File::Write(path, buffer);
    }

    void SystemConfig::Print() const {
        ESP_LOGI(TAG, "Device Name: %s", deviceName);
        ESP_LOGI(TAG, "SSID: %s", ssid);
        ESP_LOGI(TAG, "Password: %s", password);
        ESP_LOGI(TAG, "Controller Address: %s", controllerAddress);
        ESP_LOGI(TAG, "Identity: %s", identity);
        ESP_LOGI(TAG, "Username: %s", username);
        ESP_LOGI(TAG, "Token: %s", token);
    }
}
