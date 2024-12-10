#include <Application.hpp>
#include <freertos/FreeRTOS.h>
#include <FreeRTOSConfig.h>
#include <freertos/task.h>
#include <esp_chip_info.h>
#include <portmacro.h>
#include <cstdio>

void test1();
void test2();

extern "C" [[noreturn]] void app_main(void) {
    Mpm::Application app;

    app.Run();

    while (true) {
        vTaskDelay(100 / portTICK_PERIOD_MS);

#if CONFIG_COMPONENT1
        test1();
#endif
#if CONFIG_COMPONENT2
        test2();
#endif
    }
}