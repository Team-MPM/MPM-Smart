#pragma once

#include <stddef.h>

typedef struct system_config {
    const char* deviceName;
    const char* ssid;
    const char* identity;
    const char* username;
    const char* password;
    const char* controllerAddress;
    const char* token;
    const char* status;
} system_config_t;

system_config_t system_config_from_file(const char* path, char* buffer, size_t buffer_size);
void system_config_save_to_file(const system_config_t* config, const char* path);
void system_config_print(const system_config_t* config);