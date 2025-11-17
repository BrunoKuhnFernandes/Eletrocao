#ifndef HANDLERS_H
#define HANDLERS_H

#include <ArduinoJson.h>
#include <PubSubClient.h>
#include "Modelos.h"
#include <Adafruit_PWMServoDriver.h>

using namespace Modelos;

// Extern informa que essa variável existe em outro .cpp
extern Adafruit_PWMServoDriver pwmDriver;

long long getUtcUnixTimeMs(); 

extern double mediaAtraso;
extern double desvioPadrao;
extern double m2;
extern double delta;
extern double delta2;
extern long long quantidadeAtrasos;

void HandlePwmParaJunta(char* payload, unsigned int length, PubSubClient& client);
void HandleSequenciaDeComandosDasJunta(char* payload, unsigned int length, PubSubClient& client);
void HandleInstrucoes(char* payload, unsigned int length, PubSubClient& client);
void AtualizarSequencia();

#endif
