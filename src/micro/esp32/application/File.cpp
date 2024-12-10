#include "File.hpp"

#include <cstdio>
#include <cstring>
#include <esp_log.h>

#define TAG "File"

namespace Mpm {
    void File::Read(const char *path, char *buffer, size_t size) {
        FILE *file = fopen(path, "r");
        if (file == nullptr) {
            ESP_LOGE(TAG, "Failed to open file for reading");
            return;
        }

        size_t pos = 0;
        while (fgets(buffer + pos, sizeof(buffer - pos), file) != nullptr) {
            pos += strlen(buffer + pos);
        }
        fclose(file);
    }

    void File::Write(const char *path, const char *data) {
        FILE *file = fopen(path, "w");
        if (file == nullptr) {
            ESP_LOGE(TAG, "Failed to open file for writing");
            return;
        }
        fprintf(file, "%s", data);
        fclose(file);
        ESP_LOGI(TAG, "File written: %s", path);
    }
}
