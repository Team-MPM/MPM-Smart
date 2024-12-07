cmake_minimum_required(VERSION 3.29)

set(EXTRA_COMPONENT_DIRS
    component1
    component2)
include($ENV{IDF_PATH}/tools/cmake/project.cmake)

project(esp_system_info)