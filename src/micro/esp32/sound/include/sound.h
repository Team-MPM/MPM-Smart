#pragma once

#include <driver/adc.h>

void setup_sound_pin(adc1_channel_t channel);
int read_sound(adc1_channel_t channel);
