idf_component_register(
        SRCS
            "application.c"
            "system_config.c"
            "file.c"
            "crypto.c"
        INCLUDE_DIRS "include"
        REQUIRES
            "dht"
            "spiffs"
            "esp_wifi"
            "esp_http_server"
            "esp_https_ota"
            "esp_https_server"
            "esp_http_client"
            "nvs_flash")