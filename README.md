# mqtt-speed-tester

Aplicação de console em **.NET 8 (C#)** que mede a velocidade da internet com o [Speedtest CLI](https://www.speedtest.net/apps/cli) e publica o resultado em JSON num broker **MQTT**. Rodando de tempos em tempos, ela permite acompanhar a qualidade da conexão num painel ou disparar alertas quando a velocidade cai.

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![MQTT](https://img.shields.io/badge/MQTT-660066?style=flat-square&logo=mqtt&logoColor=white)

## Como funciona

```
Speedtest CLI ──(JSON)──► mqtt-speed-tester ──(JSON)──► broker MQTT ──► painel, alertas, banco
```

1. Executa o Speedtest CLI com `--format=json`, o que evita depender do layout da saída em texto.
2. Converte o resultado para unidades fáceis de ler: latência e jitter em ms, download e upload em Mbps.
3. Publica no tópico configurado com QoS 2 e a flag *retain*, para que quem assinar o tópico depois receba logo o último resultado.

Exemplo de mensagem publicada:

```json
{
  "timestamp": "2026-11-03T14:30:12+00:00",
  "isp": "Provedor Exemplo",
  "server": "Servidor Exemplo - Natal",
  "latencyMs": 8.42,
  "jitterMs": 1.13,
  "downloadMbps": 487.3,
  "uploadMbps": 251.06,
  "packetLossPercent": 0,
  "resultUrl": "https://www.speedtest.net/result/c/..."
}
```

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Speedtest CLI](https://www.speedtest.net/apps/cli), da Ookla. Ele não é versionado neste repositório: baixe a versão do seu sistema e coloque o executável na pasta do projeto ou informe o caminho em `SPEEDTEST_PATH`.
- Um broker MQTT (por exemplo, Mosquitto). Opcional: sem ele, o resultado só é exibido no terminal.

## Configuração

Tudo é configurado por variáveis de ambiente. Nenhuma credencial fica no código.

| Variável | Obrigatória | Padrão | Descrição |
|---|---|---|---|
| `MQTT_HOST` | não | — | Endereço do broker. Sem ela, o resultado não é publicado. |
| `MQTT_PORT` | não | `1883` | Porta do broker |
| `MQTT_USER` / `MQTT_PASSWORD` | não | — | Credenciais do broker, se houver |
| `MQTT_TOPIC` | não | `speedtest/result` | Tópico de publicação |
| `MQTT_CLIENT_ID` | não | `mqtt-speed-tester-<máquina>` | Identificador do cliente MQTT |
| `SPEEDTEST_PATH` | não | `speedtest.exe` (Windows) ou `speedtest` | Caminho do Speedtest CLI |
| `SPEEDTEST_SERVER_ID` | não | escolha automática | Servidor fixo para os testes (`speedtest --servers` lista os próximos) |

## Como rodar

**Linux e macOS**

```bash
export MQTT_HOST=localhost
export MQTT_TOPIC=casa/internet/velocidade
dotnet run
```

**Windows (PowerShell)**

```powershell
$env:MQTT_HOST = "localhost"
$env:MQTT_TOPIC = "casa/internet/velocidade"
dotnet run
```

Para conferir a publicação, assine o tópico em outro terminal:

```bash
mosquitto_sub -h localhost -t casa/internet/velocidade -v
```

### Códigos de saída

| Código | Significado |
|---|---|
| `0` | Teste executado e, se configurado, publicado |
| `1` | Falha no teste de velocidade (CLI ausente, erro ou tempo esgotado) |
| `2` | Configuração inválida |
| `3` | Falha ao publicar no broker |

Os códigos permitem que um agendador, como o `cron` ou o Agendador de Tarefas do Windows, identifique falhas.

### Execução periódica

Exemplo com `cron`, a cada 30 minutos, a partir de um build publicado:

```bash
dotnet publish -c Release -o /opt/mqtt-speed-tester
```

```cron
*/30 * * * * MQTT_HOST=localhost SPEEDTEST_PATH=/usr/bin/speedtest /opt/mqtt-speed-tester/mqtt-speed-tester
```

## Estrutura

```
Program.cs            fluxo principal e códigos de saída
AppSettings.cs        leitura e validação das variáveis de ambiente
SpeedTestRunner.cs    execução do Speedtest CLI, tempo limite e leitura do JSON
SpeedTestResult.cs    resultado publicado e mapeamento da saída do CLI
MqttPublisher.cs      conexão e publicação no broker
```

## Direitos autorais

Copyright 2024 Rutileno Gabriel
