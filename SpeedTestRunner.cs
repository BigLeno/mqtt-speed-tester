using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace MqttSpeedTester;

/// <summary>
/// Executa o Speedtest CLI (Ookla) e converte a saída JSON em <see cref="SpeedTestResult"/>.
/// </summary>
public sealed class SpeedTestRunner(string executablePath, string? serverId)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    public async Task<SpeedTestResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // JSON em vez de texto: não depende do layout da saída para extrair os valores.
        startInfo.ArgumentList.Add("--format=json");
        startInfo.ArgumentList.Add("--progress=no");
        startInfo.ArgumentList.Add("--accept-license");
        startInfo.ArgumentList.Add("--accept-gdpr");
        if (serverId is not null)
        {
            startInfo.ArgumentList.Add($"--server-id={serverId}");
        }

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new SpeedTestException(
                $"Não foi possível executar \"{executablePath}\". Baixe o Speedtest CLI e informe o caminho em SPEEDTEST_PATH.", ex);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        try
        {
            Task<string> stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
            Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
            await process.WaitForExitAsync(timeout.Token);

            string output = await stdout;
            string errors = await stderr;

            if (process.ExitCode != 0)
            {
                throw new SpeedTestException(
                    $"O Speedtest CLI terminou com código {process.ExitCode}: {errors.Trim()}");
            }

            return Parse(output);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new SpeedTestException($"O teste de velocidade passou de {Timeout.TotalMinutes} minutos e foi interrompido.");
        }
    }

    internal static SpeedTestResult Parse(string output)
    {
        // Com --format=json, o resultado é a última linha não vazia da saída.
        string? json = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (json is null)
        {
            throw new SpeedTestException("O Speedtest CLI não retornou nenhum resultado.");
        }

        try
        {
            SpeedtestCliOutput? parsed = JsonSerializer.Deserialize<SpeedtestCliOutput>(json);
            return parsed?.ToResult()
                ?? throw new SpeedTestException("O resultado do Speedtest CLI veio vazio.");
        }
        catch (JsonException ex)
        {
            throw new SpeedTestException("Não foi possível ler o resultado do Speedtest CLI.", ex);
        }
    }
}

public sealed class SpeedTestException : Exception
{
    public SpeedTestException(string message) : base(message) { }

    public SpeedTestException(string message, Exception innerException) : base(message, innerException) { }
}
