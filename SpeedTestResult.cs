using System.Text.Json.Serialization;

namespace MqttSpeedTester;

/// <summary>
/// Resultado publicado no MQTT, já em unidades fáceis de ler (ms e Mbps).
/// </summary>
public sealed record SpeedTestResult(
    DateTimeOffset Timestamp,
    string? Isp,
    string? Server,
    double LatencyMs,
    double JitterMs,
    double DownloadMbps,
    double UploadMbps,
    double? PacketLossPercent,
    string? ResultUrl);

/// <summary>
/// Formato da saída do Speedtest CLI com <c>--format=json</c>.
/// Só os campos usados aqui são mapeados.
/// </summary>
internal sealed record SpeedtestCliOutput(
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp,
    [property: JsonPropertyName("ping")] SpeedtestCliPing? Ping,
    [property: JsonPropertyName("download")] SpeedtestCliTransfer? Download,
    [property: JsonPropertyName("upload")] SpeedtestCliTransfer? Upload,
    [property: JsonPropertyName("packetLoss")] double? PacketLoss,
    [property: JsonPropertyName("isp")] string? Isp,
    [property: JsonPropertyName("server")] SpeedtestCliServer? Server,
    [property: JsonPropertyName("result")] SpeedtestCliResultLink? Result)
{
    public SpeedTestResult ToResult() => new(
        Timestamp: Timestamp ?? DateTimeOffset.UtcNow,
        Isp: Isp,
        Server: Server is null ? null : $"{Server.Name} - {Server.Location}",
        LatencyMs: Math.Round(Ping?.Latency ?? 0, 2),
        JitterMs: Math.Round(Ping?.Jitter ?? 0, 2),
        DownloadMbps: ToMbps(Download?.Bandwidth),
        UploadMbps: ToMbps(Upload?.Bandwidth),
        PacketLossPercent: PacketLoss,
        ResultUrl: Result?.Url);

    // O CLI informa a banda em bytes por segundo.
    private static double ToMbps(long? bytesPerSecond) =>
        Math.Round((bytesPerSecond ?? 0) * 8 / 1_000_000.0, 2);
}

internal sealed record SpeedtestCliPing(
    [property: JsonPropertyName("latency")] double? Latency,
    [property: JsonPropertyName("jitter")] double? Jitter);

internal sealed record SpeedtestCliTransfer(
    [property: JsonPropertyName("bandwidth")] long? Bandwidth);

internal sealed record SpeedtestCliServer(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("location")] string? Location);

internal sealed record SpeedtestCliResultLink(
    [property: JsonPropertyName("url")] string? Url);
