#pragma once

#include <cstddef>

namespace Mpm {
    class File {
    public:
        static void Read(const char* path, char* buffer, size_t size);
        static void Write(const char* path, const char* data);
    };
}