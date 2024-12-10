#pragma once

namespace Mpm {
    class SystemConfig {
    public:
        const char* deviceName;
        const char* ssid;
        const char* password;
        const char* controllerAddress;
        const char* token;

        SystemConfig() = default;
        static SystemConfig FromFile(const char* path);
        void SaveToFile(const char* path) const;
      private:
          SystemConfig(const char* deviceName, const char* ssid, const char* password,
                     const char* controllerAddress, const char* token);
    };
}