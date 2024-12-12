#include "application.h"

#include <esp_chip_info.h>
#include <esp_idf_version.h>
#include <esp_log.h>
#include <esp_spiffs.h>
#include <system_config.h>
#include <esp_wifi.h>
#include <nvs_flash.h>
#include <string.h>

#define TAG "Application"

esp_vfs_spiffs_conf_t conf = {
    .base_path = "/spiffs",
    .partition_label = NULL,
    .max_files = 5,
    .format_if_mount_failed = true
};

void init_spiffs() {
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
}

void wifi_event_handler(void* arg, const esp_event_base_t event_base,
                               const int32_t event_id, void* event_data) {
    if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_START) {
        esp_wifi_connect();
    } else if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_DISCONNECTED) {
        ESP_LOGI(TAG, "Disconnected, retrying...");
        esp_wifi_connect();
    } else if (event_base == IP_EVENT && event_id == IP_EVENT_STA_GOT_IP) {
        const ip_event_got_ip_t* event = event_data;
        ESP_LOGI(TAG, "Got IP: " IPSTR, IP2STR(&event->ip_info.ip));
    }
}

void setup_wifi(wifi_config_t* wifi_config, const system_config_t* config) {
    ESP_ERROR_CHECK(esp_netif_init());
    ESP_ERROR_CHECK(esp_event_loop_create_default());

    esp_netif_create_default_wifi_sta();

    const wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_wifi_init(&cfg));

    ESP_ERROR_CHECK(esp_event_handler_instance_register(WIFI_EVENT, ESP_EVENT_ANY_ID, &wifi_event_handler, NULL, NULL));
    ESP_ERROR_CHECK(esp_event_handler_instance_register(IP_EVENT, IP_EVENT_STA_GOT_IP, &wifi_event_handler, NULL, NULL));

    ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
    ESP_ERROR_CHECK(esp_wifi_start());

    strncpy((char*)wifi_config->sta.password, config->password, sizeof(wifi_config->sta.password) - 1);
    strncpy((char*)wifi_config->sta.ssid, config->ssid, sizeof(wifi_config->sta.ssid) - 1);
    wifi_config->sta.threshold.authmode = WIFI_AUTH_WPA2_PSK;
    wifi_config->sta.threshold.rssi = -127;
    wifi_config->sta.failure_retry_cnt = 5;
    wifi_config->sta.pmf_cfg.capable = true;
    wifi_config->sta.pmf_cfg.required = false;
    wifi_config->sta.scan_method = WIFI_ALL_CHANNEL_SCAN;
    wifi_config->sta.sort_method = WIFI_CONNECT_AP_BY_SIGNAL;

    ESP_LOGI(TAG, "Connecting to Wi-Fi: %s %s", (char*)wifi_config->sta.ssid, (char*)wifi_config->sta.password);
    ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, wifi_config));
    ESP_ERROR_CHECK(esp_wifi_start());
}

void application_run(void) {
    system_config_t config = {};
    wifi_config_t wifi_config = {};

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
    init_spiffs();

    // System Config
    char config_buffer[512];
    config = system_config_from_file("/spiffs/system.cfg", config_buffer, sizeof(config_buffer));
    system_config_print(&config);

    // NVS
    esp_err_t ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND) {
        // NVS partition was truncated and needs to be erased
        ESP_ERROR_CHECK(nvs_flash_erase());
        ret = nvs_flash_init();
    }
    ESP_ERROR_CHECK(ret);

    // Wifi
    setup_wifi(&wifi_config, &config);


    printf("Application is running...\n");


    while (true) {
        vTaskDelay(100 / portTICK_PERIOD_MS);
    }
}
