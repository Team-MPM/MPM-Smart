cmake_minimum_required(VERSION 3.29)

string(TIMESTAMP NOW "%s")
math(EXPR RANDOM_NUMBER "${NOW} * 32719 + 3")
string(SUBSTRING ${RANDOM_NUMBER} 0 12 RANDOM_NUMBER)
message(STATUS "Generated Serial: ${RANDOM_NUMBER}")
add_compile_definitions(SERIAL=\"${RANDOM_NUMBER}\")

set(EXTRA_COMPONENT_DIRS
    application
    shared
    dht)
include($ENV{IDF_PATH}/tools/cmake/project.cmake)

project(mpm_smart)

idf_build_set_property(PARTITION_TABLE_CUSTOM_FILENAME partitions.csv)
idf_build_set_property(PARTITION_TABLE_CUSTOM yes)
spiffs_create_partition_image(storage spiffs_image FLASH_IN_PROJECT)