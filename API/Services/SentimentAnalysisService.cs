using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
namespace API.Services;

public class SentimentAnalysisService
{
    private readonly TextAnalyticsClient _client;

    public SentimentAnalysisService(IConfiguration configuration)
    {
        string endpoint = configuration["AzureTextAnalytics:Endpoint"];
        string apiKey = configuration["AzureTextAnalytics:ApiKey"];

        AzureKeyCredential credentials = new AzureKeyCredential(apiKey);
        _client = new TextAnalyticsClient(new Uri(endpoint), credentials);
    }

    public async Task<string> AnalyzeSentiment(string text)
    {
        // Nếu text rỗng hoặc quá ngắn, trả về Neutral
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
        {
            return "Neutral";
        }

        try
        {
            DocumentSentiment sentimentResponse = await _client.AnalyzeSentimentAsync(text);
            return sentimentResponse.Sentiment.ToString();
        }
        catch (Exception)
        {
            // Nếu có lỗi, trả về Neutral
            return "Neutral";
        }
    }
}