#pragma once

#include <Arduino.h>

#include <FS.h>
#include <SPIFFS.h>
#include <sys/stat.h>

#define FORMAT_SPIFFS_IF_FAILED true

namespace Mpm {
    class Application
    {
    public:
        Application();
        ~Application();
    private:
    };
}