using System.Text.Encodings.Web;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace MqttSpeedTester;

/// <summary>
/// Publica o resultado do teste como JSON num tópico MQTT.
/// </summary>
public sealed class MqttPublisher(MqttSettings settings)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Mantém acentos e "&" legíveis no payload (ex.: "Telefônica"), em vez de ô.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task PublishAsync(SpeedTestResult result, CancellationToken cancellationToken = default)
    {
        var factory = new MqttFactory();
        using IMqttClient client = factory.CreateMqttClient();

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithClientId(settings.ClientId)
            .WithTcpServer(settings.Host, settings.Port)
            .WithCleanSession();

        if (settings.Username is not null)
        {
            optionsBuilder.WithCredentials(settings.Username, settings.Password);
        }

        await client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(settings.Topic)
                .WithPayload(JsonSerializer.Serialize(result, JsonOptions))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                // Retido: quem assinar o tópico depois recebe logo o último resultado.
                .WithRetainFlag()
                .Build();

            await client.PublishAsync(message, cancellationToken);
        }
        finally
        {
            await client.DisconnectAsync(new MqttClientDisconnectOptions(), CancellationToken.None);
        }
    }
}
