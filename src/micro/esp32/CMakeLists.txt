cmake_minimum_required(VERSION 3.29)

set(EXTRA_COMPONENT_DIRS
    application
    shared
    dht)
include($ENV{IDF_PATH}/tools/cmake/project.cmake)

project(esp_system_info)

idf_build_set_property(PARTITION_TABLE_CUSTOM_FILENAME partitions.csv)
idf_build_set_property(PARTITION_TABLE_CUSTOM yes)
spiffs_create_partition_image(storage spiffs_image FLASH_IN_PROJECT)