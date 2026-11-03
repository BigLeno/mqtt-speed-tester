namespace MqttSpeedTester;

/// <summary>
/// Configuração lida das variáveis de ambiente. Nenhuma credencial fica no código.
/// </summary>
public sealed record AppSettings(
    string SpeedtestPath,
    string? SpeedtestServerId,
    MqttSettings? Mqtt)
{
    public static AppSettings FromEnvironment()
    {
        string defaultPath = OperatingSystem.IsWindows() ? "speedtest.exe" : "speedtest";

        string? host = Read("MQTT_HOST");
        MqttSettings? mqtt = host is null
            ? null
            : new MqttSettings(
                Host: host,
                Port: ParsePort(Read("MQTT_PORT")),
                Username: Read("MQTT_USER"),
                Password: Read("MQTT_PASSWORD"),
                Topic: Read("MQTT_TOPIC") ?? "speedtest/result",
                ClientId: Read("MQTT_CLIENT_ID") ?? $"mqtt-speed-tester-{Environment.MachineName}");

        return new AppSettings(
            SpeedtestPath: Read("SPEEDTEST_PATH") ?? defaultPath,
            SpeedtestServerId: Read("SPEEDTEST_SERVER_ID"),
            Mqtt: mqtt);
    }

    private static string? Read(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int ParsePort(string? value)
    {
        if (value is null)
        {
            return 1883;
        }

        if (int.TryParse(value, out int port) && port is > 0 and <= 65535)
        {
            return port;
        }

        throw new ConfigurationException($"MQTT_PORT inválida: \"{value}\". Use um número entre 1 e 65535.");
    }
}

public sealed record MqttSettings(
    string Host,
    int Port,
    string? Username,
    string? Password,
    string Topic,
    string ClientId);

public sealed class ConfigurationException(string message) : Exception(message);
