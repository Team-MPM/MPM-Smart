#include "sound.h"

#include <esp_log.h>
#include <driver/adc.h>

#define TAG "SOUND"

void setup_sound_pin(adc1_channel_t channel) {
    adc1_config_width(ADC_WIDTH_BIT_12);
    adc1_config_channel_atten(channel, ADC_ATTEN_DB_12);
}

int read_sound(adc1_channel_t channel) {
    return adc1_get_raw(channel);
}