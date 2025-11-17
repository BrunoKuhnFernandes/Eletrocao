// Modelos.h
#ifndef MODELOS_H
#define MODELOS_H

#include <Arduino.h>

namespace Modelos {

    // Limites máximos (ajuste conforme o robô)
    #define MAX_JUNTAS 12
    #define MAX_COMANDOS 10

    struct PwmParaJunta {
        int Pino;
        int Pwm;
    };

    struct ComandosJuntas {
        PwmParaJunta PwmParaJuntas[MAX_JUNTAS];
    };

    struct SequenciaDeComandosDasJuntas {
        const char* Nome;
        ComandosJuntas Comandos[MAX_COMANDOS];
        int Passos;
        int Delay;
        int Repeticoes;
    };

    struct InformacaoDoRobo {
        float Tensao;
        float Corrente;
        const char* Timestamp;
        int ContadorDeMensagens;
    };
};


#endif
