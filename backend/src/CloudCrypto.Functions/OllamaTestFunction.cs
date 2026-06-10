using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace CloudCrypto_Functions;

public class OllamaTestFunction
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OllamaTestFunction(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this._httpClientFactory = httpClientFactory;
        this._configuration = configuration;
    }

    [Function("OllamaTest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test/ollama")] HttpRequest req
        )
    {
        var client = this._httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _configuration["OllamaApiKey"]);
        var payload = new
        {
            model = _configuration["OllamaModel"],
            messages = new[] { new { role = "user", content = "Hello! How are you?" } },
            stream = false
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var ollamaResponse = await client.PostAsync(
            $"{_configuration["OllamaApiUrl"]}/chat",
            requestContent
        );

        if (!ollamaResponse.IsSuccessStatusCode)
            return new StatusCodeResult((int)ollamaResponse.StatusCode);
        
        var responseJson = await ollamaResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var message = doc.RootElement.GetProperty("message").GetProperty("content").GetString();
        return new JsonResult(new { message });
    }
}