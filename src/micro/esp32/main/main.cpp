#include <freertos/FreeRTOS.h>
#include <FreeRTOSConfig.h>
#include <freertos/task.h>
#include <esp_chip_info.h>
#include <portmacro.h>
#include <cstdio>
#include "esp_system.h"

void test1();
void test2();

extern "C" [[noreturn]] void app_main(void) {
    printf("ESP32 System Information:\n");
    printf("Chip Model: %s\n", esp_get_idf_version());

    esp_chip_info_t chip_info;
    esp_chip_info(&chip_info);

    printf("Chip Features: %d CPU cores, WiFi%s%s, ",
           chip_info.cores,
           (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
           (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");
    printf("Silicon Revision: %d\n", chip_info.revision);

    while (true) {
        vTaskDelay(100 / portTICK_PERIOD_MS);

#if CONFIG_COMPONENT1
        test1();
#endif
#if CONFIG_COMPONENT2
        test2();
#endif
    }
}