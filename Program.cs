using System;
using System.Collections.Generic;
using System.Diagnostics;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;


public class Program
{
    public static Dictionary<string, string> GetInternetSpeedInfo()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        Process process = new Process();

        try
        {
            process.StartInfo.FileName = "speedtest.exe";
            process.StartInfo.Arguments = "--accept-gdpr --server-id=seu_id_de_servidor"; // Substitua pelo ID do servidor que você deseja usar ou remova para usar o servidor padrão
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {   
                string line = process.StandardOutput.ReadLine();

                if (line.Contains("Server:"))
                {   
                    string server = line.Substring(line.IndexOf("Server:") + "Server:".Length).Trim();
                    result["Server"] = server;
                }
                else if (line.Contains("ISP:"))
                {
                    result["ISP"] = line.Split(':')[1].Trim();
                }
                else if (line.Contains("Idle Latency:"))
                {
                    string latencyInfo = line.Split(':')[1].Trim();
                    int index = latencyInfo.IndexOf(" (jitter");
                    if (index > 0)
                    {
                        latencyInfo = latencyInfo.Substring(0, index);
                    }
                    result["Idle Latency"] = latencyInfo;
                }
                else if (line.Contains("Download:"))
                {
                    string downloadInfo = line.Split(':')[1].Trim();
                    int index = downloadInfo.IndexOf(" (data used");
                    if (index > 0)
                    {
                        downloadInfo = downloadInfo.Substring(0, index);
                    }
                    result["Download"] = downloadInfo;
                }
                else if (line.Contains("Upload:"))
                {
                   string uploadInfo = line.Split(':')[1].Trim();
                   int index = uploadInfo.IndexOf(" (data used");
                   if (index > 0)
                   {
                        uploadInfo = uploadInfo.Substring(0, index);
                   }
                   result["Upload"] = uploadInfo;
                }
                else if (line.Contains("Packet Loss:"))
                {
                    result["Packet Loss"] = line.Split(':')[1].Trim();
                }
                else if (line.Contains("Result URL:"))
                {
                    string url = line.Substring(line.IndexOf("Result URL:") + "Result URL:".Length).Trim();
                    result["Result URL"] = url;
                }
            }

            process.WaitForExit();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (process != null) process.Dispose(); 
        }
        return result;
    }


    static async Task Main(string[] args)
    {
        Dictionary<string, string> result = GetInternetSpeedInfo();

        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithClientId("cliente_mqtt") // Substitua pelo ID do cliente que você deseja usar
            .WithTcpServer("seu_broker_mqtt", 1883) // Substitua pelo endereço do seu broker MQTT
            .WithCredentials("seu_usuario", "sua_senha") // Substitua pelo usuário e senha do seu broker MQTT
            .WithCleanSession()
            .Build();

        await mqttClient.ConnectAsync(options, CancellationToken.None); // Lembre-se de lidar com exceções aqui

        // Converta o dicionário result para JSON
        string json = JsonConvert.SerializeObject(result);

        // Crie a mensagem
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("seu_tópico") // Substitua pelo tópico que você deseja usar
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            .WithRetainFlag()
            .Build();

        // Publique a mensagem
        await mqttClient.PublishAsync(message, CancellationToken.None); // Lembre-se de lidar com exceções aqui

        foreach (var item in result)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }

        // Encerra o programa
        Environment.Exit(0);
    }
}