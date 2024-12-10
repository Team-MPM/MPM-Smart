#pragma once
#include <SystemConfig.hpp>

namespace Mpm {
    class Application {
    public:
        Application();
        void Run();
    private:
        SystemConfig config;
    };
}
