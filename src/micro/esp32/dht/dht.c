#include "dht.h"

#include <esp_log.h>
#include <stdio.h>
#include <rom/ets_sys.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/gpio.h"

#define TAG "DHT11"

static bool dht11_read(gpio_num_t pin, int* dht11_data)
{
    int counter = 0;

    for (int i = 0; i < 5; i++)
        dht11_data[i] = 0;

    gpio_set_direction(pin, GPIO_MODE_OUTPUT);
    gpio_set_level(pin, 1);
    vTaskDelay(pdMS_TO_TICKS(100));
    gpio_set_level(pin, 0);
    vTaskDelay(pdMS_TO_TICKS(18));
    gpio_set_level(pin, 1);
    gpio_set_direction(pin, GPIO_MODE_INPUT );
    ets_delay_us(140);

    for (int i = 0; i < 5; i++) {
        for (int j = 0; j < 8; j++) {
            counter = 0;
            while (gpio_get_level(pin) == 0) {
                counter++;
                if (counter == 255) {
                    return true;
                }
            }

            ets_delay_us(30);

            if (gpio_get_level(pin) == 1) {
                dht11_data[i] |= 1 << (7-j);
            }

            counter = 0;
            while (gpio_get_level(pin) == 1) {
                counter++;
                if (counter == 255) {
                    return true;
                }
            }
        }
    }

    gpio_set_direction(pin, GPIO_MODE_OUTPUT);
    gpio_set_level(pin, 0);

    // check checksum
    if (dht11_data[4] == ((dht11_data[0] + dht11_data[1] + dht11_data[2] + dht11_data[3]) & 0xFF)) {
        return true;
    }
    return false;
}

void dht11_task(void *pvParameter)
{
    vTaskDelay(pdMS_TO_TICKS(1000));
    ESP_LOGI(TAG, "Starting DHT11 Task");
    const gpio_num_t pin = ((gpio_num_t*)pvParameter)[0];
    int* data = (int*)((gpio_num_t*)pvParameter)[1];
    const int delay_ms = ((int *)pvParameter)[2];

    while (1) {
        if (dht11_read(pin, data)) {
            ESP_LOGI(TAG, "Pin %d - Temperature: %d.%d \u00B0C", pin, data[2], data[3]);
            ESP_LOGI(TAG, "Pin %d - Humidity: %d.%d %%", pin, data[0], data[1]);
        } else {
            ESP_LOGI(TAG, "Pin %d - Failed to read from DHT11 sensor", pin);
        }

        vTaskDelay(pdMS_TO_TICKS(delay_ms));
    }
}

void dht11_to_json(int *data, char *json_buffer, size_t buffer_size)
{
    snprintf(json_buffer, buffer_size,
        "{\"temperature\": %d.%d, \"humidity\": %d.%d}",
        data[2], data[3], data[0], data[1]);
}
