#pragma once

namespace Mpm {
    class SystemConfig {
    public:
        const char* deviceName;
        const char* ssid;
        const char* identity;
        const char* username;
        const char* password;
        const char* controllerAddress;
        const char* token;

        SystemConfig() = default;
        static SystemConfig FromFile(const char* path);
        void SaveToFile(const char* path) const;
        void Print() const;
      private:
          SystemConfig(const char* deviceName, const char* ssid, const char* password,
                     const char* controllerAddress, const char* token,
                     const char* identity, const char* username);
    };
}