#pragma once
#include <esp_wifi_types.h>
#include <SystemConfig.hpp>

namespace Mpm {
    class Application {
    public:
        Application();
        void Run();
    private:
        SystemConfig config = {};
        wifi_config_t wifi_config = {};
    };
}
