
## Sobre o Programa

Este programa é uma aplicação .NET Core que realiza testes de velocidade de internet utilizando a ferramenta Speedtest CLI. Os resultados dos testes são então publicados em um broker MQTT especificado.

O programa requer que você baixe o Speedtest CLI e o coloque na mesma pasta do projeto. Além disso, você precisará fornecer suas próprias credenciais MQTT e o ID do servidor de teste de velocidade.

O programa é configurado para executar o teste de velocidade, coletar os resultados e publicá-los no tópico MQTT especificado. Você pode modificar o código para alterar o comportamento conforme necessário.


## Download do arquivo necessário

1. **Baixar o arquivo do Speedtest**
    Acesse a seguinte URL para fazer o download do arquivo necessário:

    ```
    https://www.speedtest.net/apps/cli
    ```

    Após o download, certifique-se de mover o arquivo para a mesma pasta do seu projeto.

    Estando na mesma pasta do projeto, você deve verificar o arquivo como está sendo importado para o projeto.

    ```csharp
    process.StartInfo.FileName = "speedtest.exe";
    ```

    O projeto foi desenvolvido em ambiente Windows, já em outros sistemas operacionais, ficaria algo como isso:

    - Linux e MacOS
        ```csharp
        process.StartInfo.FileName = "speedtest";
        ```
    

## Configuração das credenciais e ID do servidor

1. **Modificar as credenciais do MQTT e o ID do servidor de teste de velocidade**
    Você precisará modificar as credenciais do MQTT e o ID do servidor de teste de velocidade no código do projeto. Localize as linhas de código correspondentes e substitua os valores de placeholder pelos valores corretos.

    Por exemplo, se as credenciais do MQTT estão definidas como:

    ```csharp
    .WithTcpServer("seu_broker_mqtt", 1883)
    .WithCredentials("seu_usuario", "sua_senha")
    ```

    Substitua `"seu_broker_mqtt", "seu_usuario"` e `"sua_senha"` pelas suas credenciais reais do MQTT.

    Você pode modificar o tópico que será publicado no mqtt.

    ```csharp
    .WithTopic("seu_tópico")
    ```

    Substitua `"seu_tópico"` pelas suas credenciais reais do MQTT.

    Da mesma forma, se o ID do servidor de teste de velocidade está definido como:

    ```csharp
    process.StartInfo.Arguments = "--server-id=seu_id_de_servidor";
    ```

    Substitua `"seu_id_de_servidor"` pelo ID real do seu servidor de teste de velocidade.

    Ou pode comentar a linha de código que ele irá buscar automaticamente o melhor servidor para o teste.



# Como utilizar o projeto

Siga os passos abaixo para configurar e executar o projeto:

1. **Verificar a instalação do .NET Core**
    Abra o terminal e digite o seguinte comando:

    ```bash
        dotnet --version
    ```
    
    Se o .NET Core estiver instalado corretamente, você verá a versão instalada.

2. **Navegar até o diretório do projeto**
    Utilize o comando `cd` para navegar até o diretório do seu projeto. Por exemplo:

    ```bash
        cd caminho/para/seu/projeto
    ```

3. **Adicionar as bibliotecas Newtonsoft.Json e MQTTnet**
    No terminal, execute os seguintes comandos para adicionar as bibliotecas ao seu projeto:

    ```bash
        dotnet add package Newtonsoft.Json 
        dotnet add package MQTTnet
    ```

4. **Executar o projeto**
    Finalmente, você pode executar o projeto com o comando `dotnet run`:

    ```bash
        dotnet run
    ```

## Direitos Autorais

Copyright 2024 Rutileno Gabriel

