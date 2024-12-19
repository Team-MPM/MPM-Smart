idf_component_register(
        SRCS
            "application.c"
            "system_config.c"
            "file.c"
        INCLUDE_DIRS "include"
        REQUIRES
            "spiffs"
            "esp_wifi"
            "esp_http_server"
            "esp_https_ota"
            "esp_https_server"
            "esp_http_client"
            "nvs_flash")