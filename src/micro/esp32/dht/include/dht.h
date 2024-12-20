#pragma once

#define DHT11_MAX_TIMINGS 85
#define DHT_DATA_SIZE 5
#include <stddef.h>

// Param 1: Pin
// Param 2: Data
// Param 3: Delay
void dht11_task(void *pvParameter);

void dht11_to_json(int *data, char *json_buffer, size_t buffer_size);