#pragma once
#include <stddef.h>

void crypto_init();
void crypto_cleanup();
void generate_keys(unsigned char* public_key, unsigned char* private_key);
int load_keys(const char* private_key_pem, const char* public_key_pem);
int sign_token(const char* token, unsigned char* signature, size_t signature_size, size_t* sig_len);
int verify_token(const char* token, const unsigned char *signature, size_t sig_len);