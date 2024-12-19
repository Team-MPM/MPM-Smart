#include "system_config.h"

#include <esp_log.h>
#include <file.h>
#include <stdio.h>
#include <string.h>


#define TAG "SystemConfig"


system_config_t system_config_from_file(const char *path, char *buffer, const size_t buffer_size) {
    file_read(path, buffer, buffer_size);

    system_config_t config = {};

    config.deviceName = strtok(buffer, "\n");
    config.ssid = strtok(NULL, "\n");
    config.password = strtok(NULL, "\n");
    config.controllerAddress = strtok(NULL, "\n");
    config.token = strtok(NULL, "\n");
    config.identity = strtok(NULL, "\n");
    config.username = strtok(NULL, "\n");

    return config;
}

void system_config_save_to_file(const system_config_t* config, const char *path) {
    char buffer[512];
    snprintf(buffer, sizeof(buffer), "%s\n%s\n%s\n%s\n%s\n%s\n%s\n",
             config->deviceName, config->ssid, config->password, config->controllerAddress,
             config->token, config->identity, config->username);
    file_write(path, buffer);
}

void system_config_print(const system_config_t* config) {
    ESP_LOGI(TAG, "Device Name: %s", config->deviceName);
    ESP_LOGI(TAG, "SSID: %s", config->ssid);
    ESP_LOGI(TAG, "Password: %s", config->password);
    ESP_LOGI(TAG, "Controller Address: %s", config->controllerAddress);
    ESP_LOGI(TAG, "Identity: %s", config->identity);
    ESP_LOGI(TAG, "Username: %s", config->username);
    ESP_LOGI(TAG, "Token: %s", config->token);
}