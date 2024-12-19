#include "file.h"

#include <esp_log.h>
#include <stdio.h>
#include <string.h>

#define TAG "File"

void file_write(const char* path, const char* data) {
    FILE *file = fopen(path, "w");
    if (file == NULL) {
        ESP_LOGE(TAG, "Failed to open file for writing");
        return;
    }
    fprintf(file, "%s", data);
    fclose(file);
    ESP_LOGI(TAG, "File written: %s", path);
}

void file_read(const char* path, char* buffer, size_t size) {
    FILE *file = fopen(path, "r");
    if (file == NULL) {
        ESP_LOGE(TAG, "Failed to open file for reading");
        return;
    }

    size_t pos = 0;
    while (fgets(buffer + pos, sizeof(buffer - pos), file) != NULL) {
        pos += strlen(buffer + pos);
        if (pos >= size - 1) {
            ESP_LOGW(TAG, "File too large for buffer");
            break;
        }
    }
    fclose(file);
}
