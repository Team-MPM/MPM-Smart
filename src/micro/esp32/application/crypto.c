#include "crypto.h"

#include <esp_log.h>
#include <string.h>
#include <mbedtls/ctr_drbg.h>
#include <mbedtls/entropy.h>
#include <mbedtls/pk.h>
#include <mbedtls/rsa.h>

#define TAG "Crypto"

#define KEY_SIZE 1024
#define EXPONENT 65537

static mbedtls_rsa_context rsa;
static mbedtls_pk_context pk;
static mbedtls_ctr_drbg_context ctr_drbg;
static mbedtls_entropy_context entropy;

void crypto_init() {
    ESP_LOGI(TAG, "Initializing crypto system");
    mbedtls_entropy_init(&entropy);
    mbedtls_ctr_drbg_init(&ctr_drbg);
    const char *pers = "esp32_rsa";
    mbedtls_ctr_drbg_seed(&ctr_drbg, mbedtls_entropy_func, &entropy, (const unsigned char *) pers, strlen(pers));
}

void crypto_cleanup() {
    mbedtls_rsa_free(&rsa);
    mbedtls_ctr_drbg_free(&ctr_drbg);
    mbedtls_entropy_free(&entropy);
}

// Buffer should be at least 1600 bytes
void generate_keys(unsigned char* public_key, unsigned char* private_key) {
    ESP_LOGI(TAG, "Generating RSA key pair...");

    mbedtls_pk_init(&pk);

    int ret = mbedtls_pk_setup(&pk, mbedtls_pk_info_from_type(MBEDTLS_PK_RSA));
    if (ret != 0) {
        ESP_LOGE(TAG, "Failed to setup pk context! Error: -0x%04x", -ret);
        return;
    }

    ret = mbedtls_rsa_gen_key(mbedtls_pk_rsa(pk), mbedtls_ctr_drbg_random, &ctr_drbg, KEY_SIZE, EXPONENT);
    if (ret != 0) {
        ESP_LOGE(TAG, "Failed to generate key pair! Error: -0x%04x", -ret);
        return;
    }

    ESP_LOGI(TAG, "Key pair generated successfully!");

    ret = mbedtls_pk_write_key_pem(&pk, private_key, 1600);
    if (ret != 0) {
        ESP_LOGE(TAG, "Failed to write private key! Error: -0x%04x", -ret);
        return;
    }

    ret = mbedtls_pk_write_pubkey_pem(&pk, public_key, 1600);
    if (ret != 0) {
        ESP_LOGE(TAG, "Failed to write public key! Error: -0x%04x", -ret);
        return;
    }

    ESP_LOGI(TAG, "Private and public keys generated successfully!");
}

int load_keys(const char *private_key_pem, const char *public_key_pem) {
    mbedtls_pk_init(&pk);

    int ret = mbedtls_pk_parse_key(&pk, (const unsigned char *) private_key_pem,
        strlen(private_key_pem) + 1, NULL, 0, mbedtls_ctr_drbg_random, &ctr_drbg);
    if (ret != 0) {
        ESP_LOGE(TAG, "Failed to load private key! Error: -0x%04x", -ret);
        return ret;
    }

    // ret = mbedtls_pk_parse_public_key(&pk, (const unsigned char *) public_key_pem,
    //     strlen(public_key_pem) + 1);
    // if (ret != 0) {
    //     ESP_LOGE(TAG, "Failed to load public key! Error: -0x%04x", -ret);
    //     return ret;
    // }

    ESP_LOGI(TAG, "Public and Private keys loaded successfully!");
    return 0; // Success
}


int sign_token(const char* token, unsigned char* signature, const size_t signature_size, size_t* sig_len) {
    const size_t token_len = strlen(token);
    unsigned char hash[32];
    mbedtls_md_context_t md_ctx;

    // Hash the token (SHA-256)
    mbedtls_md_init(&md_ctx);
    mbedtls_md_setup(&md_ctx, mbedtls_md_info_from_type(MBEDTLS_MD_SHA256), 0);
    mbedtls_md_starts(&md_ctx);
    mbedtls_md_update(&md_ctx, (unsigned char *) token, token_len);
    mbedtls_md_finish(&md_ctx, hash);
    mbedtls_md_free(&md_ctx);

    // Sign the hash
    const int ret = mbedtls_pk_sign(&pk, MBEDTLS_MD_SHA256, hash, 0,
                                    signature, signature_size, sig_len, mbedtls_ctr_drbg_random, &ctr_drbg);
    if (ret == 0) {
        ESP_LOGI(TAG, "Token signed successfully!\n");
    } else {
        ESP_LOGE(TAG, "Failed to sign token! Error: -0x%04x\n", -ret);
    }
    return ret;
}

int verify_token(const char* token, const unsigned char* signature, const size_t sig_len) {
    const size_t token_len = strlen(token);
    unsigned char hash[32];
    mbedtls_md_context_t md_ctx;

    // Hash the token
    mbedtls_md_init(&md_ctx);
    mbedtls_md_setup(&md_ctx, mbedtls_md_info_from_type(MBEDTLS_MD_SHA256), 0);
    mbedtls_md_starts(&md_ctx);
    mbedtls_md_update(&md_ctx, (unsigned char *) token, token_len);
    mbedtls_md_finish(&md_ctx, hash);
    mbedtls_md_free(&md_ctx);

    // Verify the signature
    const int ret = mbedtls_pk_verify(&pk, MBEDTLS_MD_SHA256, hash, 0, signature, sig_len);
    if (ret == 0) {
        ESP_LOGI(TAG, "Token verified successfully!\n");
    } else {
        ESP_LOGW(TAG, "Failed to verify token! Error: -0x%04x\n", -ret);
    }
    return ret;
}
