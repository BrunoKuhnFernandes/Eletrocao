#include <WiFi.h>
#include <PubSubClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>
#include "Config.h"
#include "Modelos.h"
#include "Handlers.h"
#include <Wire.h>
#include <Adafruit_PWMServoDriver.h>
#include <Adafruit_ADS1X15.h>
#include <time.h>

Adafruit_PWMServoDriver pwmDriver = Adafruit_PWMServoDriver(0x40); // endereço padrão
 Adafruit_ADS1115 ads;

using namespace Modelos;

char* estado = "Conectado";
char* mensagem = "Robô quadrupede conectado com sucesso!";
unsigned long lastTelemetryTime = 0;
unsigned long ultimoTempoAmostra = 0;
unsigned int contadorAmostras = 0;
unsigned int contadorDeMensagens = 0;
float somaTensaoBateriaServomotores = 0.0;
float somaTensaoBateriaMicro = 0.0;
float somaCorrente = 0.0;


WiFiClientSecure espClient;
PubSubClient client(espClient);

// Estrutura para mapear tópicos para funções
typedef void (*HandlerFunc)(char*, unsigned int, PubSubClient&);
struct TopicHandler {
    const char* topic;
    HandlerFunc handler;
};

// Lista de tópicos
TopicHandler topicHandlers[] = {
    { TopicoParaControlePorSequenciaDeComandosEsp32, HandleSequenciaDeComandosDasJunta },
    { TopicoParaControlePorPwmEsp32, HandlePwmParaJunta },
    { TopicoParaInstrucoes, HandleInstrucoes }
    };

void setup_wifi() {
  delay(10);
  Serial.println();
  Serial.print("Conectando à rede: ");
  Serial.println(WIFI_SSID);

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
  }

  Serial.println("\nWiFi conectado");
  Serial.print("Endereço IP: ");
  Serial.println(WiFi.localIP());
}

void callback(char* topic, byte* payload, unsigned int length) {
    Serial.print("Mensagem recebida no tópico: ");
    Serial.println(topic);
    for (auto th : topicHandlers) {
        if (strcmp(topic, th.topic) == 0) {
            th.handler((char*)payload, length, client);
            return;
        }
    }
    Serial.print("Nenhum handler para o tópico: ");
    Serial.println(topic);
}

void reconnect() {
  while (!client.connected()) {
    Serial.print("Conectando ao broker MQTT...");
    if (client.connect("ESP32ClientSub", MQTT_USERNAME, MQTT_PASSWORD)) {
      Serial.println(" conectado!");

      // Inscrever em todos os tópicos
      for (auto th : topicHandlers) {
          client.subscribe(th.topic);
          Serial.print("Inscrito no tópico: ");
          Serial.println(th.topic);
      }

    } else {
      Serial.printf(" falha, rc=%d tentando novamente em 5 segundos\n", client.state());
      delay(5000);
    }
  }
}

void publishTelemetry(const char* estado, const char* mensagem, float tensaoBateriaServomotores, float tensaoBateriaMicro, float corrente, int contadorDeMensagens, int tempoMedioDeAtrasoComandos, int desvioPadrao)
 {
  StaticJsonDocument<512> doc;
  doc["TensaoBateriaServomotores"] = tensaoBateriaServomotores;
  doc["TensaoBateriaMicro"] = tensaoBateriaMicro;
  doc["Corrente"] = corrente;
  doc["Estado"] = estado;
  doc["Mensagem"] = mensagem;
  doc["ContadorDeMensagens"] = contadorDeMensagens;
  doc["TempoMedioDeAtrasoComandos"] = tempoMedioDeAtrasoComandos;
  doc["DesvioPadrao"] = desvioPadrao;


  // Timestamp em ISO 8601
  char timestamp[9];
  time_t now = time(nullptr);
  struct tm* tm_info = localtime(&now);
  strftime(timestamp, sizeof(timestamp), "%H:%M:%S", tm_info);
  doc["Timestamp"] = timestamp;

  char jsonBuffer[512];
  serializeJson(doc, jsonBuffer);
  client.publish(TopicoParaInformacoesDoRobo, jsonBuffer);
}

// ======= FUNÇÕES DE LEITURA DO ADS1115 =======

void coletarLeiturasInstantaneas() {
  unsigned long agora = millis();
  if (agora - ultimoTempoAmostra < INTERVALO_ENTRE_AMOSTRAS) return;
  ultimoTempoAmostra = agora;

  // Lê o ADC
  int16_t adcTensaoBateriaServomotores = ads.readADC_SingleEnded(1);
  int16_t adcTensaoBateriaMicro = ads.readADC_SingleEnded(2);
  int16_t adcCorrente = ads.readADC_SingleEnded(0);

  float voltsTensaoBateriaServomotores = ads.computeVolts(adcTensaoBateriaServomotores);
  float voltsTensaoBateriaMicro = ads.computeVolts(adcTensaoBateriaMicro);
  float voltsCorrente = ads.computeVolts(adcCorrente);

  somaTensaoBateriaServomotores += voltsTensaoBateriaServomotores;
  somaTensaoBateriaMicro += voltsTensaoBateriaMicro;
  somaCorrente += voltsCorrente;
  contadorAmostras++;
}


void obterMediaLeituras(float &tensaoBateriaServomotores, float &tensaoBateriaMicro, float &corrente) {
  if (contadorAmostras == 0) {
    tensaoBateriaServomotores = 0;
    tensaoBateriaMicro = 0;
    corrente = 0;
    return;
  }

  float mediaTensaoBateriaServomotores = somaTensaoBateriaServomotores / contadorAmostras;
  float mediaTensaoBateriaMicro = somaTensaoBateriaMicro / contadorAmostras;
  float mediaCorrente = somaCorrente / contadorAmostras;

  tensaoBateriaServomotores = mediaTensaoBateriaServomotores * 5.0;   
  tensaoBateriaMicro = mediaTensaoBateriaMicro * 5.0; // ajuste divisor resistivo
  corrente = (mediaCorrente - 2.511) / 0.08; // ajuste ACS712

  // Zera para o próximo ciclo
  somaTensaoBateriaServomotores = 0;
  somaTensaoBateriaMicro = 0;
  somaCorrente = 0;
  contadorAmostras = 0;
}

long long getUtcUnixTimeMs()
{
    struct timeval tv;
    gettimeofday(&tv, nullptr);
    return (tv.tv_sec * 1000LL) + (tv.tv_usec / 1000LL);
}


void setup() {
  Serial.begin(115200);
  setup_wifi();
  espClient.setInsecure();
  client.setServer(MQTT_BROKER, MQTT_PORT);
  client.setCallback(callback);
  client.setBufferSize(2048);

  configTime(0, 0, "pool.ntp.br");;

    Serial.println("Sincronizando NTP...");
    struct tm timeinfo;
    while (!getLocalTime(&timeinfo)) {
        Serial.println("Aguardando NTP...");
        delay(500);
    }
    Serial.println("NTP sincronizado com sucesso!");
   if (!ads.begin(0x48)) {
     Serial.println("Falha ao inicializar o ADS1115!");
      while (1);
   }

   Serial.println("ADS1115 inicializado com sucesso.");

  // Inicialização do PCA9685
  pwmDriver.begin();
  pwmDriver.setOscillatorFrequency(27000000); 
  pwmDriver.setPWMFreq(50); // Frequência para servos (50Hz)
  Wire.setClock(400000);


  quantidadeAtrasos = 0;
  mediaAtraso = 0;
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

  // Coleta leituras continuamente (sem bloquear)
  coletarLeiturasInstantaneas();

  if (millis() - lastTelemetryTime >= PUBLISH_DATA_INTERVAL) {
    contadorDeMensagens++;
    lastTelemetryTime = millis();

    float tensaoBateriaServomotores, tensaoBateriaMicro, corrente;
    obterMediaLeituras(tensaoBateriaServomotores, tensaoBateriaMicro, corrente);

    publishTelemetry(
        estado,
        mensagem,
        tensaoBateriaServomotores,
        tensaoBateriaMicro,
        corrente,
        contadorDeMensagens,
        mediaAtraso,
        desvioPadrao
    );

  }

  AtualizarSequencia();
}

