using MqttSpeedTester;

// Códigos de saída: 0 sucesso, 1 falha no teste, 2 configuração inválida, 3 falha ao publicar.
try
{
    AppSettings settings = AppSettings.FromEnvironment();

    Console.WriteLine("Executando o teste de velocidade...");
    var runner = new SpeedTestRunner(settings.SpeedtestPath, settings.SpeedtestServerId);
    SpeedTestResult result = await runner.RunAsync();

    Console.WriteLine($"Provedor:  {result.Isp}");
    Console.WriteLine($"Servidor:  {result.Server}");
    Console.WriteLine($"Latência:  {result.LatencyMs} ms (jitter {result.JitterMs} ms)");
    Console.WriteLine($"Download:  {result.DownloadMbps} Mbps");
    Console.WriteLine($"Upload:    {result.UploadMbps} Mbps");
    Console.WriteLine($"Perda:     {(result.PacketLossPercent is null ? "não informada" : $"{result.PacketLossPercent}%")}");
    Console.WriteLine($"Resultado: {result.ResultUrl}");

    if (settings.Mqtt is null)
    {
        Console.WriteLine("MQTT_HOST não definido: resultado não publicado.");
        return 0;
    }

    try
    {
        await new MqttPublisher(settings.Mqtt).PublishAsync(result);
        Console.WriteLine($"Publicado em {settings.Mqtt.Host}:{settings.Mqtt.Port}, tópico \"{settings.Mqtt.Topic}\".");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Falha ao publicar no MQTT: {ex.Message}");
        return 3;
    }
}
catch (ConfigurationException ex)
{
    Console.Error.WriteLine($"Configuração inválida: {ex.Message}");
    return 2;
}
catch (SpeedTestException ex)
{
    Console.Error.WriteLine($"Falha no teste de velocidade: {ex.Message}");
    return 1;
}
