#include "Application.h"

namespace Mpm {
    Application::Application() {
        Serial.begin(9600);
        Serial.setDebugOutput(true);
        Serial.println("\n");
        Serial.println("Booting");

        if(!SPIFFS.begin(FORMAT_SPIFFS_IF_FAILED)){
            Serial.println("SPIFFS Mount Failed");
            return;
        }

        int space = SPIFFS.totalBytes();
        Serial.println("SPIFFS Total Space: " + String(space));

        File file = SPIFFS.open("/test.txt", "r");
        
        if(!file){
            Serial.println("There was an error opening the file");
            return;
        }

        Serial.println("File Content:");
        while(file.available()){
            Serial.write(file.read());
        }

    }

    Application::~Application() {
        
    }
}