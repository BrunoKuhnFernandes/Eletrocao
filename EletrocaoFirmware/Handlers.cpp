#include "Handlers.h"
#include "Modelos.h"
#include <Arduino.h>
#include <PubSubClient.h>
#include <Adafruit_PWMServoDriver.h>
#include <ArduinoJson.h>

extern Adafruit_PWMServoDriver pwmDriver;
unsigned long ultimaInstrucaoMillis;
String instrucaoAtual;

SequenciaDeComandosDasJuntas sequencia;
static bool sequenciaAtiva = false;
static unsigned long ultimoTempo = 0;
static int comandoAtual = 0;
static int passoAtual = 0;
static int ultimoPwmPorPino[MAX_JUNTAS] = {0};

// Variáveis para armazenar quantidades reais
static int quantidadeComandos = 0;
static int quantidadeJuntasPorComando[MAX_COMANDOS] = {0};

double mediaAtraso = 0.0;
double desvioPadrao = 0.0;
double m2 = 0.0;
double delta = 0.0;
double delta2 = 0.0;

long long quantidadeAtrasos = 0;

// ------------------------ HANDLE INSTRUÇÃO ------------------------
void HandleInstrucoes(char* payload, unsigned int length, PubSubClient& client) {
    StaticJsonDocument<256> doc;
    DeserializationError error = deserializeJson(doc, payload, length);
    if (error) {
        Serial.print("Erro ao desserializar instrução: ");
        Serial.println(error.c_str());
        return;
    }

    String instrucao = doc["Nome"].as<String>();
    instrucaoAtual = instrucao;
    ultimaInstrucaoMillis = millis();

    Serial.print("Instrução recebida: ");
    Serial.println(instrucaoAtual);

    // Se for "Parar", interrompe imediatamente
    if (instrucaoAtual == "Parar") {
        sequenciaAtiva = false;
        Serial.println("Sequência interrompida (instrução Parar).");
    }
}


// ------------------------ HANDLE PWM SIMPLES ------------------------
void HandlePwmParaJunta(char* payload, unsigned int length, PubSubClient& client) {
    StaticJsonDocument<256> doc;
    DeserializationError error = deserializeJson(doc, payload, length);
    if (error) {
        Serial.print("Erro ao desserializar PWM: ");
        Serial.println(error.c_str());
        return;
    }

    JsonArray arr = doc.as<JsonArray>();
    for (JsonObject obj : arr) {
        PwmParaJunta junta;
        junta.Pino = obj["Pino"];
        junta.Pwm  = obj["Pwm"];

        Serial.printf("Aplicando no PCA9685 -> Pino: %d | PWM: %d\n", junta.Pino, junta.Pwm);

        pwmDriver.setPWM(junta.Pino, 0, junta.Pwm);
        ultimoPwmPorPino[junta.Pino] = junta.Pwm;
    }
}

// ------------------------ HANDLE SEQUÊNCIA ------------------------
void HandleSequenciaDeComandosDasJunta(char* payload, unsigned int length, PubSubClient& client) {
    StaticJsonDocument<2048> doc;
    DeserializationError error = deserializeJson(doc, payload, length);

    if (error) {
        Serial.print("Erro ao desserializar sequência: ");
        Serial.println(error.c_str());
        return;
    }

    long long timestamp = 0;
    if (doc.containsKey("Timestamp")) {
        timestamp = doc["Timestamp"].as<long long>();
    }

    long long agora = getUtcUnixTimeMs();
    long long atraso = agora - timestamp;

    if (atraso < 0 || atraso > 5000) {
        return;
    }
   quantidadeAtrasos++;

    delta = atraso - mediaAtraso;
    mediaAtraso += delta / quantidadeAtrasos;

    delta2 = atraso - mediaAtraso;
    m2 += delta * delta2;

    double variancia = (quantidadeAtrasos > 1)
        ? m2 / (quantidadeAtrasos - 1)
        : 0.0;

    desvioPadrao = sqrt(variancia);

    Serial.printf(
        "Atraso=%lld | Media=%.2f ms | DesvioPadrao=%.2f ms | N=%lld\n",
        atraso, mediaAtraso, desvioPadrao, quantidadeAtrasos
    );

    // Atualiza a instrução atual
    instrucaoAtual = doc["Nome"].as<String>();
    ultimaInstrucaoMillis = millis();

    // Preenche dados principais da sequência
    sequencia.Nome = doc["Nome"].as<const char*>();
    sequencia.Passos = doc["Passos"].isNull() ? 20 : doc["Passos"].as<int>();
    sequencia.Delay  = doc["Delay"].isNull()  ? 20 : doc["Delay"].as<int>();
    sequencia.Repeticoes = 1;

    // Extrai comandos
    JsonArray comandosArray = doc["Comandos"].as<JsonArray>();
    quantidadeComandos = min((int)comandosArray.size(), MAX_COMANDOS);

    Serial.printf("Processando sequência '%s' com %d comandos, %d passos, %d ms delay\n", 
                  sequencia.Nome, quantidadeComandos, sequencia.Passos, sequencia.Delay);

    // Limpa dados anteriores
    for (int i = 0; i < MAX_COMANDOS; i++) {
        quantidadeJuntasPorComando[i] = 0;
        for (int j = 0; j < MAX_JUNTAS; j++) {
            sequencia.Comandos[i].PwmParaJuntas[j].Pino = -1;
            sequencia.Comandos[i].PwmParaJuntas[j].Pwm = 0;
        }
    }

    for (int i = 0; i < quantidadeComandos; i++) {
        JsonArray pwmArray = comandosArray[i]["PwmParaJuntas"].as<JsonArray>();
        int qtdJuntas = min((int)pwmArray.size(), MAX_JUNTAS);
        quantidadeJuntasPorComando[i] = qtdJuntas;

        for (int j = 0; j < qtdJuntas; j++) {
            JsonObject obj = pwmArray[j];
            int pino = obj["Pino"].as<int>();
            int pwm  = obj["Pwm"].as<int>();

            sequencia.Comandos[i].PwmParaJuntas[j].Pino = pino;
            sequencia.Comandos[i].PwmParaJuntas[j].Pwm  = pwm;
            // Se não havia PWM anterior, lê o valor atual do driver ou usa padrão
            if (ultimoPwmPorPino[pino] == 0) {
                ultimoPwmPorPino[pino] = 300; // valor neutro padrão
            }
        }
    }

    // Inicializa estado de execução
    comandoAtual = 0;
    passoAtual = 0;
    ultimoTempo = millis();
    sequenciaAtiva = true;
}


void AtualizarSequencia() {
    if (!sequenciaAtiva) return;

    // Timeout de segurança (4 segundos sem nova instrução)
    if (millis() - ultimaInstrucaoMillis > 4000) {
        sequenciaAtiva = false;
        Serial.println("Sequência interrompida por timeout.");
        return;
    }

    // Delay entre passos
    unsigned long agora = millis();
    if (agora - ultimoTempo < (unsigned long)sequencia.Delay)
        return;

    ultimoTempo = agora;

    // Executa interpolação para o comando atual
    int passos = sequencia.Passos;
    int qtdJuntas = quantidadeJuntasPorComando[comandoAtual];

    for (int j = 0; j < qtdJuntas; j++) {
        int pino = sequencia.Comandos[comandoAtual].PwmParaJuntas[j].Pino;
        int destino = sequencia.Comandos[comandoAtual].PwmParaJuntas[j].Pwm;

        if (pino < 0 || pino >= MAX_JUNTAS) continue;

        // Pega último PWM conhecido
        int inicio = ultimoPwmPorPino[pino];
        if (inicio == 0) inicio = 300;

        // Interpola o PWM: inicio + (destino - inicio) * (passoAtual + 1) / passos
        int pwmInterpolado = inicio + (destino - inicio) * (passoAtual + 1) / passos;

        pwmDriver.setPWM(pino, 0, pwmInterpolado);
        
        // Atualiza apenas no último passo
        if (passoAtual + 1 >= passos) {
            ultimoPwmPorPino[pino] = pwmInterpolado;
        }
    }

    passoAtual++;

    // Se terminou os passos do comando atual
    if (passoAtual >= sequencia.Passos) {
        
        passoAtual = 0;
        comandoAtual++;

        // Se acabaram todos os comandos, reinicia do primeiro (loop contínuo)
        if (comandoAtual >= quantidadeComandos) {
            comandoAtual = 0;
        }
    }
}