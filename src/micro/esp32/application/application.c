#include "application.h"

#include <crypto.h>
#include <dht.h>
#include <esp_chip_info.h>
#include <esp_http_server.h>
#include <esp_idf_version.h>
#include <esp_log.h>
#include <esp_spiffs.h>
#include <esp_task_wdt.h>
#include <system_config.h>
#include <esp_wifi.h>
#include <file.h>
#include <nvs_flash.h>
#include <string.h>
#include <driver/gpio.h>
#include <lwip/sockets.h>

#define TAG "Application"

esp_vfs_spiffs_conf_t conf = {
    .base_path = "/spiffs",
    .partition_label = NULL,
    .max_files = 5,
    .format_if_mount_failed = true
};

unsigned char public_key[1600];
unsigned char private_key[1600];
const char* public_key_path = "/spiffs/rsa_public_key.pem";
const char* private_key_path = "/spiffs/rsa_private_key.pem";

char* serial = SERIAL;
const char* token = "full-access";
const char* config_path = "/spiffs/system.cfg";

system_config_t config = {};
wifi_config_t wifi_config = {};

void init_spiffs() {
    esp_err_t ret = esp_vfs_spiffs_register(&conf);

    if (ret != ESP_OK) {
        if (ret == ESP_FAIL) {
            ESP_LOGE("SPIFFS", "Failed to mount or format filesystem");
        } else if (ret == ESP_ERR_NOT_FOUND) {
            ESP_LOGE("SPIFFS", "Failed to find SPIFFS partition");
        } else {
            ESP_LOGE("SPIFFS", "Failed to initialize SPIFFS (%s)", esp_err_to_name(ret));
        }
        return;
    }

    size_t total = 0, used = 0;
    ret = esp_spiffs_info(NULL, &total, &used);
    if (ret == ESP_OK) {
        ESP_LOGI("SPIFFS", "Partition size: total: %d, used: %d", total, used);
    } else {
        ESP_LOGE("SPIFFS", "Failed to get SPIFFS partition information");
    }
}

void wifi_event_handler(void* arg, const esp_event_base_t event_base,
                               const int32_t event_id, void* event_data) {
    if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_START) {
        esp_wifi_connect();
    } else if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_DISCONNECTED) {
        ESP_LOGI(TAG, "Disconnected, retrying...");
        esp_wifi_connect();
    } else if (event_base == IP_EVENT && event_id == IP_EVENT_STA_GOT_IP) {
        const ip_event_got_ip_t* event = event_data;
        ESP_LOGI(TAG, "Got IP: " IPSTR, IP2STR(&event->ip_info.ip));
    }
}

void setup_wifi(wifi_config_t* wifi_config, const system_config_t* config) {
    ESP_ERROR_CHECK(esp_netif_init());
    ESP_ERROR_CHECK(esp_event_loop_create_default());

    esp_netif_create_default_wifi_sta();

    const wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_wifi_init(&cfg));

    ESP_ERROR_CHECK(esp_event_handler_instance_register(WIFI_EVENT, ESP_EVENT_ANY_ID, &wifi_event_handler, NULL, NULL));
    ESP_ERROR_CHECK(esp_event_handler_instance_register(IP_EVENT, IP_EVENT_STA_GOT_IP, &wifi_event_handler, NULL, NULL));

    ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
    ESP_ERROR_CHECK(esp_wifi_start());

    strncpy((char*)wifi_config->sta.password, config->password, sizeof(wifi_config->sta.password) - 1);
    strncpy((char*)wifi_config->sta.ssid, config->ssid, sizeof(wifi_config->sta.ssid) - 1);
    wifi_config->sta.threshold.authmode = WIFI_AUTH_WPA2_PSK;
    wifi_config->sta.threshold.rssi = -127;
    wifi_config->sta.failure_retry_cnt = 5;
    wifi_config->sta.pmf_cfg.capable = true;
    wifi_config->sta.pmf_cfg.required = false;
    wifi_config->sta.scan_method = WIFI_ALL_CHANNEL_SCAN;
    wifi_config->sta.sort_method = WIFI_CONNECT_AP_BY_SIGNAL;

    ESP_LOGI(TAG, "Connecting to Wi-Fi: %s %s", (char*)wifi_config->sta.ssid, (char*)wifi_config->sta.password);
    ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, wifi_config));

    // if (strcmp(config->username,  "None") == 0) {
    //     ESP_ERROR_CHECK(esp_eap_client_set_username((uint8_t *)config->username, strlen(config->username)));
    //     ESP_ERROR_CHECK(esp_eap_client_set_password((uint8_t *)config->password, strlen(config->password)));
    //     wifi_config->sta.threshold.authmode = WIFI_AUTH_WPA2_ENTERPRISE;
    //     ESP_ERROR_CHECK(esp_wifi_sta_enterprise_enable());
    // }

    ESP_ERROR_CHECK(esp_wifi_start());
}

#define ASYNC_WORKER_TASK_PRIORITY      5
#define ASYNC_WORKER_TASK_STACK_SIZE    2048
#define MAX_ASYNC_REQUESTS              5

// Async reqeusts are queued here while they wait to
// be processed by the workers
static QueueHandle_t async_req_queue;

// Track the number of free workers at any given time
static SemaphoreHandle_t worker_ready_count;

// Each worker has its own thread
static TaskHandle_t worker_handles[MAX_ASYNC_REQUESTS];

typedef esp_err_t (*httpd_req_handler_t)(httpd_req_t *req);

typedef struct {
    httpd_req_t* req;
    httpd_req_handler_t handler;
} httpd_async_req_t;

static bool is_on_async_worker_thread(void)
{
    // is our handle one of the known async handles?
    TaskHandle_t handle = xTaskGetCurrentTaskHandle();
    for (int i = 0; i < MAX_ASYNC_REQUESTS; i++) {
        if (worker_handles[i] == handle) {
            return true;
        }
    }
    return false;
}

static esp_err_t submit_async_req(httpd_req_t *req, httpd_req_handler_t handler)
{
    // must create a copy of the request that we own
    httpd_req_t* copy = NULL;
    const esp_err_t err = httpd_req_async_handler_begin(req, &copy);
    if (err != ESP_OK) {
        return err;
    }

    const httpd_async_req_t async_req = {
        .req = copy,
        .handler = handler,
    };

    if (xSemaphoreTake(worker_ready_count, 0) == false) {
        ESP_LOGE(TAG, "No workers are available");
        httpd_req_async_handler_complete(copy); // cleanup
        return ESP_FAIL;
    }

    if (xQueueSend(async_req_queue, &async_req, pdMS_TO_TICKS(50)) == false) {
        ESP_LOGE(TAG, "worker queue is full");
        httpd_req_async_handler_complete(copy); // cleanup
        return ESP_FAIL;
    }

    return ESP_OK;
}

static void async_req_worker_task(void *p)
{
    ESP_LOGI(TAG, "starting async req task worker");

    while (true) {
        xSemaphoreGive(worker_ready_count);

        // wait for a request
        httpd_async_req_t async_req;
        if (xQueueReceive(async_req_queue, &async_req, portMAX_DELAY)) {

            ESP_LOGI(TAG, "invoking %s", async_req.req->uri);

            // call the handler
            async_req.handler(async_req.req);

            // Inform the server that it can purge the socket used for
            // this request, if needed.
            if (httpd_req_async_handler_complete(async_req.req) != ESP_OK) {
                ESP_LOGE(TAG, "failed to complete async req");
            }
        }
    }

    ESP_LOGW(TAG, "worker stopped");
    vTaskDelete(NULL);
}


static void start_async_req_workers(void)
{
    worker_ready_count = xSemaphoreCreateCounting(MAX_ASYNC_REQUESTS, 0);
    if (worker_ready_count == NULL) {
        ESP_LOGE(TAG, "Failed to create workers counting Semaphore");
        return;
    }

    // create queue
    async_req_queue = xQueueCreate(1, sizeof(httpd_async_req_t));
    if (async_req_queue == NULL){
        ESP_LOGE(TAG, "Failed to create async_req_queue");
        vSemaphoreDelete(worker_ready_count);
        return;
    }

    // start worker tasks
    for (int i = 0; i < MAX_ASYNC_REQUESTS; i++) {

        const bool success = xTaskCreate(async_req_worker_task, "async_req_worker",
                                    ASYNC_WORKER_TASK_STACK_SIZE,
                                    (void *)0, // argument
                                    ASYNC_WORKER_TASK_PRIORITY,
                                    &worker_handles[i]);

        if (!success) {
            ESP_LOGE(TAG, "Failed to start asyncReqWorker");
        }
    }
}

static esp_err_t index_handler(httpd_req_t *req)
{
    ESP_LOGI(TAG, "uri: /");
    const char* html = "<div>Hello from ESP</div>";
    httpd_resp_sendstr(req, html);
    return ESP_OK;
}

const char info_json_template[] = "{"
        "   \"name\": \"%s\","
        "   \"description\": \"%s\","
        "   \"status\": \"%s\","
        "   \"serial\": \"%s\","
        "   \"publicKey\": \"%s\""
        "}";

char info_json_buffer[3000];

static esp_err_t info_handler(httpd_req_t *req)
{
    ESP_LOGI(TAG, "uri: /info");
    httpd_resp_set_type(req, "application/json");
    sniprintf(info_json_buffer, sizeof(info_json_buffer), info_json_template,
        "Environment Sensor", "ESP32 Environment Sensor, MPM-Smart-Device", config.status, serial, "not implemented"/*(char*)public_key*/);
    httpd_resp_sendstr(req, info_json_buffer);
    return ESP_OK;
}

unsigned char signature_out_buffer[256];
char* client_ip_buffer[128];

static esp_err_t sensor_connect_handler(httpd_req_t *req)
{
    ESP_LOGI(TAG, "uri: /connect");
    if (strcmp(config.status, "ready") == 0) {
        int sockfd = httpd_req_to_sockfd(req);
        if (sockfd < 0) {
            ESP_LOGE(TAG, "Failed to get socket from request");
            httpd_resp_send_500(req);
            return ESP_FAIL;
        }

        struct sockaddr_in client_addr;
        socklen_t addr_len = sizeof(client_addr);

        if (getpeername(sockfd, (struct sockaddr *)&client_addr, &addr_len) < 0) {
            ESP_LOGE(TAG, "Failed to get peer address");
            httpd_resp_send_500(req);
            return ESP_FAIL;
        }

        const char *client_ip = inet_ntoa(client_addr.sin_addr);
        strcpy(client_ip_buffer, client_ip);

        ESP_LOGI(TAG, "Client IP trying to connect: %s", client_ip);

        httpd_resp_set_type(req, "text/plain");
        size_t sig_len;
        sign_token(token, signature_out_buffer, sizeof(signature_out_buffer), &sig_len);
        httpd_resp_sendstr(req, (char*)signature_out_buffer);
    } else {
        httpd_resp_send_err(req, 403, "Device is already connected");
    }
    return ESP_OK;
}

unsigned char signature_in_buffer[256];

static esp_err_t sensor_connect_akk_handler(httpd_req_t *req)
{
    ESP_LOGI(TAG, "uri: /connect_akk");

    int sockfd = httpd_req_to_sockfd(req);
    if (sockfd < 0) {
        ESP_LOGE(TAG, "Failed to get socket from request");
        httpd_resp_send_500(req);
        return ESP_FAIL;
    }

    struct sockaddr_in client_addr;
    socklen_t addr_len = sizeof(client_addr);

    if (getpeername(sockfd, (struct sockaddr *)&client_addr, &addr_len) < 0) {
        ESP_LOGE(TAG, "Failed to get peer address");
        httpd_resp_send_500(req);
        return ESP_FAIL;
    }

    const char *client_ip = inet_ntoa(client_addr.sin_addr);

    if (strcmp(client_ip, client_ip_buffer) != 0) {
        ESP_LOGE(TAG, "Client IP does not match");
        httpd_resp_send_err(req, 403, "Client IP does not match");
        return ESP_FAIL;
    }

    config.status = "connected";
    system_config_save_to_file(&config, config_path);

    httpd_resp_sendstr(req, "success");

    return ESP_OK;
}

#ifdef CONFIG_DHT
static int dht11_data[DHT_DATA_SIZE];
static int dht11_data_valid[DHT_DATA_SIZE];
static char* dht_json_buffer[64];

static esp_err_t dht_handler(httpd_req_t *req)
{
    ESP_LOGI(TAG, "uri: /dht");
    dht11_to_json(dht11_data, dht_json_buffer, sizeof(dht_json_buffer));
    httpd_resp_set_type(req, "application/json");
    httpd_resp_sendstr(req, dht_json_buffer);
    return ESP_OK;
}
#endif

static httpd_handle_t start_webserver(void)
{
    httpd_handle_t server = NULL;
    httpd_config_t config = HTTPD_DEFAULT_CONFIG();
    config.lru_purge_enable = true;

    // Leave one more open socket to receive the async requests even when full
    config.max_open_sockets = MAX_ASYNC_REQUESTS + 1;

    // Start the httpd server
    ESP_LOGI(TAG, "Starting server on port: '%d'", config.server_port);
    if (httpd_start(&server, &config) != ESP_OK) {
        ESP_LOGI(TAG, "Error starting server!");
        return NULL;
    }

    const httpd_uri_t index_uri = {
        .uri       = "/",
        .method    = HTTP_GET,
        .handler   = index_handler,
    };

    httpd_register_uri_handler(server, &index_uri);

    const httpd_uri_t info_uri = {
        .uri       = "/info",
        .method    = HTTP_GET,
        .handler   = info_handler,
    };

    httpd_register_uri_handler(server, &info_uri);

    const httpd_uri_t connect_uri = {
        .uri       = "/connect",
        .method    = HTTP_GET,
        .handler   = sensor_connect_handler,
    };

    httpd_register_uri_handler(server, &connect_uri);

    const httpd_uri_t connect_akk_uri = {
        .uri       = "/connect-akk",
        .method    = HTTP_GET,
        .handler   = sensor_connect_akk_handler,
    };

    httpd_register_uri_handler(server, &connect_akk_uri);

#ifdef CONFIG_DHT
    const httpd_uri_t dht_uri = {
        .uri       = "/dht",
        .method    = HTTP_GET,
        .handler   = dht_handler,
    };

    httpd_register_uri_handler(server, &dht_uri);
#endif

    return server;
}

static esp_err_t stop_webserver(httpd_handle_t server)
{
    return httpd_stop(server);
}

static void disconnect_handler(void* arg, esp_event_base_t event_base,
                               int32_t event_id, void* event_data)
{
    httpd_handle_t* server = (httpd_handle_t*) arg;
    if (*server) {
        ESP_LOGI(TAG, "Stopping webserver");
        if (stop_webserver(*server) == ESP_OK) {
            *server = NULL;
        } else {
            ESP_LOGE(TAG, "Failed to stop http server");
        }
    }
}

static void connect_handler(void* arg, esp_event_base_t event_base,
                            int32_t event_id, void* event_data)
{
    httpd_handle_t* server = (httpd_handle_t*) arg;
    if (*server == NULL) {
        ESP_LOGI(TAG, "Starting webserver");
        *server = start_webserver();
    }
}

void application_run(void) {
    // System Info
    printf("ESP32 System Information:\n");
    printf("Chip Model: %s\n", esp_get_idf_version());

    esp_chip_info_t chip_info;
    esp_chip_info(&chip_info);

    printf("Chip Features: %d CPU cores, WiFi%s%s, ",
           chip_info.cores,
           (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
           (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");
    printf("Silicon Revision: %d\n", chip_info.revision);

    // SPIFFS Init
    init_spiffs();

    // System Config
    char config_buffer[512];
    config = system_config_from_file(config_path, config_buffer, sizeof(config_buffer));
    system_config_print(&config);

    if (strcmp("connected", config.status) != 0)
        config.status = "ready";

    // NVS
    esp_err_t ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND) {
        // NVS partition was truncated and needs to be erased
        ESP_ERROR_CHECK(nvs_flash_erase());
        ret = nvs_flash_init();
    }
    ESP_ERROR_CHECK(ret);

    // Crypto
    crypto_init();

    memset(public_key, 0, sizeof(public_key));
    file_read(public_key_path, (char*)public_key, sizeof(public_key));
    memset(private_key, 0, sizeof(private_key));
    file_read(private_key_path, (char*)private_key, sizeof(private_key));

    if (public_key[0] == 0 || private_key[0] == 0) {
        ESP_LOGI(TAG, "Generating new keys...");
        generate_keys(public_key, private_key);
        file_write(public_key_path, (char*)public_key);
        file_write(private_key_path, (char*)private_key);
    } else {
        ESP_LOGI(TAG, "Loading keys...");
        load_keys((char*)private_key, (char*)public_key);
    }

    ESP_LOGI(TAG, "Public Key: %s", (char*) public_key);
    ESP_LOGI(TAG, "Private Key: %s", (char*) private_key);

    unsigned char signature[256];
    size_t sig_len;
    sign_token(token, signature, sizeof(signature), &sig_len);

    verify_token(token, signature, sig_len);

    // Wifi
    setup_wifi(&wifi_config, &config);

    // start workers
    start_async_req_workers();

    // Start the server
    httpd_handle_t server = start_webserver();

#ifdef CONFIG_DHT
    int params_task1[3] = {GPIO_NUM_17, (int)dht11_data, 2000};
    xTaskCreate(&dht11_task, "dht11_task", 2048, params_task1, 5, NULL);
#endif

    printf("Application is running...\n");

    while (true) {
        vTaskDelay(100 / portTICK_PERIOD_MS);
    }

    crypto_cleanup();
}
