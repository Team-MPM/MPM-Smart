#include "Application.hpp"

#include <cstdio>
#include <esp_chip_info.h>
#include <esp_idf_version.h>
#include <esp_log.h>
#include <esp_spiffs.h>
#include <SystemConfig.hpp>

namespace Mpm {
    Application::Application() {
        // System Info
        printf("ESP32 System Information:\n");
        printf("Chip Model: %s\n", esp_get_idf_version());

        esp_chip_info_t chip_info;
        esp_chip_info(&chip_info);

        printf("Chip Features: %d CPU cores, WiFi%s%s, ",
               chip_info.cores,
               (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
               (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");
        printf("Silicon Revision: %d\n", chip_info.revision);

        // SPIFFS Init
        esp_vfs_spiffs_conf_t conf = {
            .base_path = "/spiffs",
            .partition_label = NULL,
            .max_files = 5, // Max files that can be open at the same time
            .format_if_mount_failed = true
        };

        esp_err_t ret = esp_vfs_spiffs_register(&conf);

        if (ret != ESP_OK) {
            if (ret == ESP_FAIL) {
                ESP_LOGE("SPIFFS", "Failed to mount or format filesystem");
            } else if (ret == ESP_ERR_NOT_FOUND) {
                ESP_LOGE("SPIFFS", "Failed to find SPIFFS partition");
            } else {
                ESP_LOGE("SPIFFS", "Failed to initialize SPIFFS (%s)", esp_err_to_name(ret));
            }
            return;
        }

        size_t total = 0, used = 0;
        ret = esp_spiffs_info(NULL, &total, &used);
        if (ret == ESP_OK) {
            ESP_LOGI("SPIFFS", "Partition size: total: %d, used: %d", total, used);
        } else {
            ESP_LOGE("SPIFFS", "Failed to get SPIFFS partition information");
        }

        // System Config
        config = SystemConfig::FromFile("/spiffs/system.cfg");
    }

    void Application::Run() {

    }

}
