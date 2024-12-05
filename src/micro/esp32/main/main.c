#include <esp_chip_info.h>
#include <stdio.h>
#include "esp_system.h"

void app_main(void) {
    printf("ESP32 System Information:\n");
    printf("Chip Model: %s\n", esp_get_idf_version());

    esp_chip_info_t chip_info;
    esp_chip_info(&chip_info);

    printf("Chip Features: %d CPU cores, WiFi%s%s, ",
           chip_info.cores,
           (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
           (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");
    printf("Silicon Revision: %d\n", chip_info.revision);
}