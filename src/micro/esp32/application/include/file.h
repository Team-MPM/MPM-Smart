#pragma once

#include <stddef.h>

void file_write(const char* path, const char* data);
void file_read(const char* path, char* buffer, size_t size);