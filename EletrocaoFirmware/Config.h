// config.h
#ifndef CONFIG_H
#define CONFIG_H

// Credenciais Wi-Fi
const char* WIFI_SSID     = "******";
const char* WIFI_PASSWORD = "******";

// MQTT
const char* MQTT_BROKER   = "******";
const int   MQTT_PORT     = 8883;
const char* MQTT_USERNAME = "******";
const char* MQTT_PASSWORD = "******";

// Tópicos MQTT
const char* TopicoParaControlePorPwmEsp32 = "ControlePwmEsp32";
const char* TopicoParaControlePorSequenciaDeComandosEsp32 = "ControleSequenciaDeComandosEsp32";
const char* TopicoParaInformacoesDoRobo = "InformacoesDoRobo";
const char* TopicoParaConfigurarAsJuntasDoRoboEsp32 = "ConfiguracoesJuntasRoboEsp32";
const char* TopicoParaInstrucoes = "Instrucoes";
// Telemetria
const long PUBLISH_DATA_INTERVAL = 2000;
const unsigned int NUM_AMOSTRAS = 30;
const unsigned long INTERVALO_ENTRE_AMOSTRAS = 10; // ms entre leituras

#endif
